import { useQueries } from '@tanstack/react-query'
import { RefreshCw } from 'lucide-react'
import type { DaySchedule } from '@/api/schedule'
import { tasksApi } from '@/api/tasks'
import type { Task, WorkStatus } from '@/api/tasks'
import { useTaskSession } from '@/contexts/TaskSessionContext'
import { cn } from '@/lib/utils'

const STATUS_CONFIG: Record<Exclude<WorkStatus, 'deleted'>, { label: string; cls: string }> = {
  new: { label: 'Новая', cls: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400' },
  todo: { label: 'Запланирована', cls: 'bg-blue-50 text-blue-600 dark:bg-blue-950 dark:text-blue-400' },
  running: { label: 'В работе', cls: 'bg-amber-50 text-amber-600 dark:bg-amber-950 dark:text-amber-400' },
  completed: { label: 'Выполнена', cls: 'bg-green-50 text-green-600 dark:bg-green-950 dark:text-green-400' },
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

function blockDurationMinutes(startAt: string, endAt: string) {
  return Math.round((new Date(endAt).getTime() - new Date(startAt).getTime()) / 60000)
}

function blockDuration(startAt: string, endAt: string) {
  return `${blockDurationMinutes(startAt, endAt)} мин`
}

interface Props {
  schedule: DaySchedule
  onRegenerate: () => void
}

export function DayScheduleTimeline({ schedule, onRegenerate }: Props) {
  const blocks = schedule.blocks
  const explanations = schedule.explanations ?? []
  const { current } = useTaskSession()
  const isRegenerationBlocked = current != null

  const taskQueries = useQueries({
    queries: blocks.map(block => ({
      queryKey: ['task', block.taskId],
      queryFn: () => tasksApi.getById(block.taskId),
      retry: false,
    })),
  })

  const statusMap: Record<string, WorkStatus> = {}
  const taskMap: Record<string, Task> = {}

  taskQueries.forEach((query, index) => {
    const task = query.data?.data.payload
    if (!task) {
      return
    }

    statusMap[blocks[index].taskId] = task.workStatus
    taskMap[blocks[index].taskId] = task
  })

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-sm font-semibold text-foreground">Расписание на сегодня</h2>
          <p className="mt-0.5 text-xs text-muted-foreground">
            {blocks.length} {blocks.length === 1 ? 'задача' : blocks.length >= 2 && blocks.length <= 4 ? 'задачи' : 'задач'}
          </p>
        </div>
        <button
          onClick={() => {
            if (!isRegenerationBlocked) {
              onRegenerate()
            }
          }}
          disabled={isRegenerationBlocked}
          title={isRegenerationBlocked ? 'Завершите или отмените активную сессию перед перегенерацией' : undefined}
          className={cn(
            'flex items-center gap-1 text-xs transition-colors',
            isRegenerationBlocked
              ? 'cursor-not-allowed text-muted-foreground/60'
              : 'text-muted-foreground hover:text-foreground'
          )}
        >
          <RefreshCw className="h-3.5 w-3.5" />
          Перегенерировать
        </button>
      </div>

      {isRegenerationBlocked && (
        <p className="text-xs text-muted-foreground">
          Пока идет активная сессия, перегенерация расписания заблокирована.
        </p>
      )}

      {explanations.length > 0 && (
        <div className="rounded-lg border border-border/40 bg-muted/30 px-3 py-2">
          <p className="mb-1 text-xs font-medium text-foreground">Почему такой план</p>
          <ul className="space-y-1">
            {explanations.slice(0, 3).map((explanation, index) => (
              <li key={`${index}-${explanation}`} className="text-xs leading-relaxed text-muted-foreground">
                {explanation}
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="relative space-y-0">
        {blocks.map((block, index) => {
          const status = statusMap[block.taskId]
          const task = taskMap[block.taskId]
          const statusInfo = status && status !== 'deleted' ? STATUS_CONFIG[status] : null
          const plannedMinutes = task?.estimatedMinutes ?? blockDurationMinutes(block.startAt, block.endAt)
          const isRunning = status === 'running' && task != null
          const isOverrun = isRunning && task.actualSpentMinutes >= task.estimatedMinutes

          const durationLabel = isRunning
            ? isOverrun
              ? `${plannedMinutes} мин план / время вышло`
              : `${plannedMinutes} мин план / ${task.remainingMinutes} мин осталось`
            : blockDuration(block.startAt, block.endAt)

          return (
            <div key={block.scheduleId} className="flex gap-3">
              <div className="flex flex-col items-center">
                <div
                  className={cn(
                    'mt-3 h-2.5 w-2.5 shrink-0 rounded-full',
                    status === 'completed' ? 'bg-green-500' : status === 'running' ? 'bg-amber-500' : 'bg-indigo-500'
                  )}
                />
                {index < blocks.length - 1 && <div className="my-1 w-px flex-1 bg-border" />}
              </div>

              <div className="flex-1 pb-3">
                <div className="rounded-xl border border-border/50 bg-card p-3">
                  <div className="mb-1 flex items-center justify-between gap-2">
                    <span className="shrink-0 text-xs font-mono text-indigo-600 dark:text-indigo-400">
                      {formatTime(block.startAt)} – {formatTime(block.endAt)}
                    </span>
                    <div className="flex items-center gap-2">
                      {statusInfo && (
                        <span className={cn('rounded-full px-1.5 py-0.5 text-xs font-medium', statusInfo.cls)}>
                          {statusInfo.label}
                        </span>
                      )}
                      <span className="shrink-0 text-xs text-muted-foreground">{durationLabel}</span>
                    </div>
                  </div>

                  <p className="text-sm font-medium leading-snug text-foreground">{block.taskTitle}</p>

                  {block.taskDescription && (
                    <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">{block.taskDescription}</p>
                  )}

                  {isOverrun && task != null && (
                    <p className="mt-1 text-xs text-muted-foreground">Потрачено {task.actualSpentMinutes} мин</p>
                  )}
                </div>
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
