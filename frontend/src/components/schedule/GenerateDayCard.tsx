import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { ArrowLeft, Clock, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { scheduleApi } from '@/api/schedule'
import { tasksApi } from '@/api/tasks'
import { useTaskSession } from '@/contexts/TaskSessionContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

function toLocalISOString(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  const offsetMin = -d.getTimezoneOffset()
  const sign = offsetMin >= 0 ? '+' : '-'
  const absMin = Math.abs(offsetMin)
  const oh = pad(Math.floor(absMin / 60))
  const om = pad(absMin % 60)

  return (
    `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
    `T${pad(d.getHours())}:${pad(d.getMinutes())}:00${sign}${oh}:${om}`
  )
}

function buildLocalDateTime(scheduleDate: string, startTime: string): Date {
  const [year, month, day] = scheduleDate.split('-').map(Number)
  const [hours, minutes] = startTime.split(':').map(Number)

  return new Date(year, month - 1, day, hours, minutes, 0, 0)
}

function defaultStartTime(): string {
  const d = new Date()
  const totalMinutes = d.getHours() * 60 + d.getMinutes()
  const rounded = Math.ceil(totalMinutes / 30) * 30
  const h = Math.floor(rounded / 60) % 24
  const m = rounded % 60

  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`
}

interface Props {
  scheduleDate: string
  onGenerated: () => void
  onCancel?: () => void
  preselectedTaskIds?: string[]
}

export function GenerateDayCard({
  scheduleDate,
  onGenerated,
  onCancel,
  preselectedTaskIds = [],
}: Props) {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [startTime, setStartTime] = useState(defaultStartTime)
  const preselectedTaskIdsKey = preselectedTaskIds.join('|')
  const { current } = useTaskSession()

  const { data, isLoading } = useQuery({
    queryKey: ['tasks', 'all'],
    queryFn: () => tasksApi.getAll({ pageSize: 100 }),
  })

  const tasks = useMemo(
    () =>
      (data?.data.payload.tasks ?? []).filter(
        task =>
          (task.workStatus === 'new' || task.workStatus === 'todo' || task.workStatus === 'running') &&
          task.remainingMinutes > 0
      ),
    [data?.data.payload.tasks]
  )

  const allowedTaskIdsKey = useMemo(
    () => tasks.map(task => task.id).join('|'),
    [tasks]
  )

  useEffect(() => {
    setSelectedIds(prev => {
      const allowedIds = new Set(tasks.map(task => task.id))
      const next = new Set([...prev].filter(id => allowedIds.has(id)))
      return next.size === prev.size ? prev : next
    })
  }, [tasks])

  useEffect(() => {
    if (preselectedTaskIds.length === 0) {
      return
    }

    setSelectedIds(prev => {
      const allowedIds = new Set(tasks.map(task => task.id))
      const next = new Set(prev)
      for (const taskId of preselectedTaskIds) {
        if (allowedIds.has(taskId)) {
          next.add(taskId)
        }
      }

      return next.size === prev.size ? prev : next
    })
  }, [allowedTaskIdsKey, preselectedTaskIdsKey, tasks])

  const toggle = (id: string) =>
    setSelectedIds(prev => {
      const next = new Set(prev)
      if (next.has(id)) {
        next.delete(id)
      } else {
        next.add(id)
      }

      return next
    })

  const selectAll = () => setSelectedIds(new Set(tasks.map(task => task.id)))
  const clearAll = () => setSelectedIds(new Set())

  const mutation = useMutation({
    mutationFn: () => {
      const startAt = toLocalISOString(buildLocalDateTime(scheduleDate, startTime))
      const allowedIds = new Set(tasks.map(task => task.id))
      const taskIds = [...selectedIds].filter(taskId => allowedIds.has(taskId))
      return scheduleApi.generate({ scheduleDate, startAt, taskIds })
    },
    onSuccess: () => onGenerated(),
    onError: error => {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        toast.info('Завершите или отмените активную сессию перед генерацией расписания')
        return
      }

      toast.error('Не удалось сгенерировать расписание')
    },
  })

  return (
    <div className="space-y-5">
      <div className="flex items-start justify-between gap-2">
        <div>
          <h2 className="mb-0.5 text-sm font-semibold text-foreground">Расписание на сегодня</h2>
          <p className="text-xs text-muted-foreground">
            Выберите задачи и время начала — план составится автоматически
          </p>
        </div>
        {onCancel && (
          <button
            onClick={onCancel}
            className="mt-0.5 flex shrink-0 items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            Назад
          </button>
        )}
      </div>

      <div className="space-y-1.5">
        <Label className="flex items-center gap-1.5 text-sm font-medium">
          <Clock className="h-3.5 w-3.5" />
          Начало дня
        </Label>
        <Input
          type="time"
          value={startTime}
          onChange={e => setStartTime(e.target.value)}
          className="h-10 w-36 bg-background/80"
        />
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label className="text-sm font-medium">Задачи в план</Label>
          <div className="flex gap-2">
            <button
              onClick={selectAll}
              className="text-xs font-medium text-indigo-600 hover:text-indigo-700"
            >
              Все
            </button>
            <span className="text-xs text-muted-foreground">·</span>
            <button
              onClick={clearAll}
              className="text-xs text-muted-foreground hover:text-foreground"
            >
              Сбросить
            </button>
          </div>
        </div>

        {isLoading && (
          <div className="flex justify-center py-8">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-indigo-600 border-t-transparent" />
          </div>
        )}

        {!isLoading && tasks.length === 0 && (
          <p className="py-6 text-center text-sm text-muted-foreground">
            Нет задач, доступных для планирования
          </p>
        )}

        <div className="space-y-1.5">
          {tasks.map(task => (
            <button
              key={task.id}
              type="button"
              onClick={() => toggle(task.id)}
              className={cn(
                'w-full rounded-xl border px-3 py-2.5 text-left transition-colors',
                selectedIds.has(task.id)
                  ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-950/40'
                  : 'border-border bg-card hover:border-border/80'
              )}
            >
              <div className="flex items-start gap-2.5">
                <span
                  className={cn(
                    'mt-0.5 flex h-4 w-4 shrink-0 items-center justify-center rounded border text-xs',
                    selectedIds.has(task.id)
                      ? 'border-indigo-500 bg-indigo-500 text-white'
                      : 'border-border'
                  )}
                >
                  {selectedIds.has(task.id) && '✓'}
                </span>
                <div className="min-w-0">
                  <p className="text-sm font-medium leading-snug text-foreground">{task.title}</p>
                  <p className="mt-0.5 text-xs text-muted-foreground">
                    {task.remainingMinutes === task.estimatedMinutes
                      ? `${task.remainingMinutes} мин`
                      : `${task.remainingMinutes} мин осталось из ${task.estimatedMinutes}`}
                  </p>
                </div>
              </div>
            </button>
          ))}
        </div>
      </div>

      <Button
        onClick={() => mutation.mutate()}
        disabled={selectedIds.size === 0 || mutation.isPending || !startTime || current != null}
        className="w-full bg-indigo-600 text-white hover:bg-indigo-500"
      >
        {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
        Сгенерировать план
        {selectedIds.size > 0 && ` (${selectedIds.size})`}
      </Button>

      {current != null && (
        <p className="text-xs text-muted-foreground">
          Сначала завершите или отмените активную сессию, затем пересоберите расписание.
        </p>
      )}
    </div>
  )
}
