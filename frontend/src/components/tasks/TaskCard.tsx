import { useEffect, useState } from 'react'
import { motion } from 'framer-motion'
import { Calendar, ChevronRight, Clock, Pencil, Play, Square, Trash2, X } from 'lucide-react'
import type { ComplexityLevel, Priority, Task, WorkStatus } from '@/api/tasks'
import { cn } from '@/lib/utils'

const CANCEL_TIMEOUT_SECONDS = 120

const STATUS_CONFIG: Record<Exclude<WorkStatus, 'deleted'>, { label: string; cls: string }> = {
  new: { label: 'Новая', cls: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400' },
  todo: { label: 'Запланирована', cls: 'bg-blue-50 text-blue-600 dark:bg-blue-950 dark:text-blue-400' },
  running: { label: 'В работе', cls: 'bg-amber-50 text-amber-600 dark:bg-amber-950 dark:text-amber-400' },
  completed: { label: 'Выполнена', cls: 'bg-green-50 text-green-600 dark:bg-green-950 dark:text-green-400' },
}

const NEXT_STATUS: Partial<Record<WorkStatus, { status: Exclude<WorkStatus, 'deleted'>; label: string }>> = {
  new: { status: 'todo', label: 'Запланировать' },
  todo: { status: 'running', label: 'В работу' },
  running: { status: 'completed', label: 'Завершить' },
}

function getPriorityInfo(priority: Priority) {
  if (priority <= 3) {
    return { label: 'Низкий', cls: 'bg-green-50 text-green-700' }
  }
  if (priority <= 6) {
    return { label: 'Средний', cls: 'bg-yellow-50 text-yellow-700' }
  }
  if (priority <= 8) {
    return { label: 'Высокий', cls: 'bg-orange-50 text-orange-700' }
  }

  return { label: 'Критический', cls: 'bg-red-50 text-red-700' }
}

function getComplexityLabel(complexity: ComplexityLevel) {
  if (complexity <= 3) {
    return 'Простая'
  }
  if (complexity <= 6) {
    return 'Средняя'
  }
  if (complexity <= 8) {
    return 'Сложная'
  }

  return 'Очень сложная'
}

function formatDeadline(iso: string) {
  return new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

function isOverdue(iso: string, status: WorkStatus) {
  return status !== 'completed' && status !== 'deleted' && new Date(iso) < new Date()
}

export type SessionState = 'none' | 'active' | 'other'

interface Props {
  task: Task
  onStatusChange: (taskId: string, status: Exclude<WorkStatus, 'deleted'>) => void
  onDelete: (taskId: string) => void
  onFeedback: (task: Task) => void
  onEdit: (task: Task) => void
  feedbackDone?: boolean
  sessionState?: SessionState
  sessionSyncing?: boolean
  activeSessionStartedAt?: string
  onStartSession?: () => void
  onStopSession?: () => void
  onCancelSession?: () => void
}

export function TaskCard({
  task,
  onStatusChange,
  onDelete,
  onFeedback,
  onEdit,
  feedbackDone,
  sessionState = 'none',
  sessionSyncing = false,
  activeSessionStartedAt,
  onStartSession,
  onStopSession,
  onCancelSession,
}: Props) {
  const [elapsed, setElapsed] = useState(0)
  const statusInfo = STATUS_CONFIG[task.workStatus as Exclude<WorkStatus, 'deleted'>]
  const priorityInfo = getPriorityInfo(task.priority)
  const next = NEXT_STATUS[task.workStatus]
  const overdue = isOverdue(task.deadlineAt, task.workStatus)
  const canStartSession = task.workStatus === 'todo' || task.workStatus === 'running'
  const canCancelSession = sessionState === 'active' && elapsed < CANCEL_TIMEOUT_SECONDS

  useEffect(() => {
    if (sessionState !== 'active' || !activeSessionStartedAt) {
      setElapsed(0)
      return
    }

    const tick = () => {
      setElapsed(Math.floor((Date.now() - new Date(activeSessionStartedAt).getTime()) / 1000))
    }

    tick()
    const id = setInterval(tick, 1000)
    return () => clearInterval(id)
  }, [activeSessionStartedAt, sessionState])

  return (
    <motion.div
      layout
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0, scale: 0.98 }}
      className={cn(
        'rounded-xl border bg-card p-4 transition-shadow hover:shadow-sm',
        sessionState === 'active' ? 'border-amber-300 dark:border-amber-700' : 'border-border/50'
      )}
    >
      <div className="mb-2 flex flex-wrap items-center gap-2">
        {statusInfo && (
          <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', statusInfo.cls)}>
            {statusInfo.label}
          </span>
        )}
        <span className={cn('rounded-full px-2 py-0.5 text-xs font-medium', priorityInfo.cls)}>
          {priorityInfo.label}
        </span>
        <span className="text-xs text-muted-foreground">{getComplexityLabel(task.complexityLevel)}</span>
      </div>

      <h3 className="text-sm font-medium leading-snug text-foreground">{task.title}</h3>
      <p className="mt-1 line-clamp-2 text-xs text-muted-foreground">{task.description}</p>

      <div className="mt-3 flex items-center gap-3">
        <span className="flex items-center gap-1 text-xs text-muted-foreground">
          <Clock className="h-3 w-3" />
          {task.estimatedMinutes} мин
        </span>
        <span
          className={cn(
            'flex items-center gap-1 text-xs',
            overdue ? 'font-medium text-destructive' : 'text-muted-foreground'
          )}
        >
          <Calendar className="h-3 w-3" />
          {formatDeadline(task.deadlineAt)}
          {overdue && ' · просрочено'}
        </span>
      </div>

      {canStartSession && (
        <div className="mt-3 flex items-center gap-2 border-t border-border/40 pt-3">
          {sessionState === 'active' && (
            <>
              <button
                onClick={onStopSession}
                disabled={sessionSyncing}
                className="flex items-center gap-1 text-xs font-medium text-amber-700 transition-colors hover:text-amber-800 disabled:opacity-50"
              >
                <Square className="h-3 w-3 fill-current" />
                Стоп
              </button>
              {canCancelSession && (
                <button
                  onClick={onCancelSession}
                  disabled={sessionSyncing}
                  className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground disabled:opacity-50"
                >
                  <X className="h-3 w-3" />
                  Отмена
                </button>
              )}
            </>
          )}

          {sessionState === 'none' && (
            <button
              onClick={onStartSession}
              disabled={sessionSyncing}
              className="flex items-center gap-1 text-xs font-medium text-indigo-600 transition-colors hover:text-indigo-700 disabled:opacity-50"
            >
              <Play className="h-3 w-3 fill-current" />
              Начать работу
            </button>
          )}

          {sessionState === 'other' && (
            <span className="flex items-center gap-1 text-xs text-muted-foreground/50">
              <Play className="h-3 w-3" />
              Начать работу
            </span>
          )}
        </div>
      )}

      <div
        className={cn(
          'flex items-center gap-3 border-t border-border/40 pt-3',
          canStartSession ? 'mt-2' : 'mt-3'
        )}
      >
        {next && sessionState !== 'active' && (
          <button
            onClick={() => onStatusChange(task.id, next.status)}
            className="flex items-center gap-0.5 text-xs font-medium text-indigo-600 transition-colors hover:text-indigo-700"
          >
            {next.label}
            <ChevronRight className="h-3 w-3" />
          </button>
        )}

        {task.workStatus === 'completed' && !feedbackDone && (
          <button
            onClick={() => onFeedback(task)}
            className="text-xs font-medium text-green-600 transition-colors hover:text-green-700"
          >
            + Фидбек
          </button>
        )}

        {task.workStatus !== 'completed' && (
          <button
            onClick={() => onEdit(task)}
            className="ml-auto rounded p-1 text-muted-foreground transition-colors hover:text-foreground"
          >
            <Pencil className="h-3.5 w-3.5" />
          </button>
        )}

        <button
          onClick={() => onDelete(task.id)}
          className={cn(
            'rounded p-1 text-muted-foreground transition-colors hover:text-destructive',
            task.workStatus === 'completed' && 'ml-auto'
          )}
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </div>
    </motion.div>
  )
}
