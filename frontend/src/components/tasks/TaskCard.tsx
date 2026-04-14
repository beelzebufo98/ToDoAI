import { motion } from 'framer-motion'
import { Clock, Calendar, ChevronRight, Trash2, Pencil } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { Task, WorkStatus, Priority, ComplexityLevel } from '@/api/tasks'

const STATUS_CONFIG: Record<Exclude<WorkStatus, 'deleted'>, { label: string; cls: string }> = {
  new:       { label: 'Новая',         cls: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400' },
  todo:      { label: 'Запланирована', cls: 'bg-blue-50 text-blue-600 dark:bg-blue-950 dark:text-blue-400' },
  running:   { label: 'В работе',      cls: 'bg-amber-50 text-amber-600 dark:bg-amber-950 dark:text-amber-400' },
  completed: { label: 'Выполнена',     cls: 'bg-green-50 text-green-600 dark:bg-green-950 dark:text-green-400' },
}

const NEXT_STATUS: Partial<Record<WorkStatus, { status: Exclude<WorkStatus, 'deleted'>; label: string }>> = {
  new:     { status: 'todo',      label: 'Запланировать' },
  todo:    { status: 'running',   label: 'Начать' },
  running: { status: 'completed', label: 'Завершить' },
}

function getPriorityInfo(p: Priority) {
  if (p <= 3) return { label: 'Низкий',      cls: 'bg-green-50  text-green-700' }
  if (p <= 6) return { label: 'Средний',     cls: 'bg-yellow-50 text-yellow-700' }
  if (p <= 8) return { label: 'Высокий',     cls: 'bg-orange-50 text-orange-700' }
  return           { label: 'Критический',  cls: 'bg-red-50    text-red-700' }
}

function getComplexityLabel(c: ComplexityLevel) {
  if (c <= 3) return 'Простая'
  if (c <= 6) return 'Средняя'
  if (c <= 8) return 'Сложная'
  return 'Очень сложная'
}

function formatDeadline(iso: string) {
  return new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

function isOverdue(iso: string, status: WorkStatus) {
  return status !== 'completed' && status !== 'deleted' && new Date(iso) < new Date()
}

interface Props {
  task: Task
  onStatusChange: (taskId: string, status: Exclude<WorkStatus, 'deleted'>) => void
  onDelete: (taskId: string) => void
  onFeedback: (task: Task) => void
  onEdit: (task: Task) => void
  feedbackDone?: boolean
}

export function TaskCard({ task, onStatusChange, onDelete, onFeedback, onEdit, feedbackDone }: Props) {
  const statusInfo = STATUS_CONFIG[task.workStatus as Exclude<WorkStatus, 'deleted'>]
  const priorityInfo = getPriorityInfo(task.priority)
  const next = NEXT_STATUS[task.workStatus]
  const overdue = isOverdue(task.deadlineAt, task.workStatus)

  return (
    <motion.div
      layout
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.98 }}
      className="bg-card border border-border/50 rounded-xl p-4 hover:shadow-sm transition-shadow"
    >
      {/* Top row: badges */}
      <div className="flex items-center gap-2 mb-2 flex-wrap">
        {statusInfo && (
          <span className={cn('text-xs font-medium px-2 py-0.5 rounded-full', statusInfo.cls)}>
            {statusInfo.label}
          </span>
        )}
        <span className={cn('text-xs font-medium px-2 py-0.5 rounded-full', priorityInfo.cls)}>
          {priorityInfo.label}
        </span>
        <span className="text-xs text-muted-foreground">
          {getComplexityLabel(task.complexityLevel)}
        </span>
      </div>

      {/* Title & description */}
      <h3 className="font-medium text-sm text-foreground leading-snug">{task.title}</h3>
      <p className="text-xs text-muted-foreground mt-1 line-clamp-2">{task.description}</p>

      {/* Meta: time + deadline */}
      <div className="flex items-center gap-3 mt-3">
        <span className="flex items-center gap-1 text-xs text-muted-foreground">
          <Clock className="h-3 w-3" />
          {task.estimatedMinutes} мин
        </span>
        <span className={cn(
          'flex items-center gap-1 text-xs',
          overdue ? 'text-destructive font-medium' : 'text-muted-foreground'
        )}>
          <Calendar className="h-3 w-3" />
          {formatDeadline(task.deadlineAt)}
          {overdue && ' · просрочено'}
        </span>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-3 mt-3 pt-3 border-t border-border/40">
        {next && (
          <button
            onClick={() => onStatusChange(task.id, next.status)}
            className="flex items-center gap-0.5 text-xs text-indigo-600 hover:text-indigo-700 font-medium transition-colors"
          >
            {next.label}
            <ChevronRight className="h-3 w-3" />
          </button>
        )}
        {task.workStatus === 'completed' && !feedbackDone && (
          <button
            onClick={() => onFeedback(task)}
            className="text-xs text-green-600 hover:text-green-700 font-medium transition-colors"
          >
            + Фидбек
          </button>
        )}
        <button
          onClick={() => onEdit(task)}
          className="ml-auto text-muted-foreground hover:text-foreground transition-colors p-1 rounded"
        >
          <Pencil className="h-3.5 w-3.5" />
        </button>
        <button
          onClick={() => onDelete(task.id)}
          className="text-muted-foreground hover:text-destructive transition-colors p-1 rounded"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </button>
      </div>
    </motion.div>
  )
}
