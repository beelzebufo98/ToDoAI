import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { AnimatePresence } from 'framer-motion'
import { Plus, AlertCircle } from 'lucide-react'
import { toast } from 'sonner'
import { useNavigate } from 'react-router-dom'
import { tasksApi } from '@/api/tasks'
import type { Task, WorkStatus } from '@/api/tasks'
import { userStateApi } from '@/api/userState'
import { Button } from '@/components/ui/button'
import { TaskCard } from '@/components/tasks/TaskCard'
import { CreateTaskDialog } from '@/components/tasks/CreateTaskDialog'
import { EditTaskDialog } from '@/components/tasks/EditTaskDialog'
import { TaskExecutionDialog } from '@/components/tasks/TaskExecutionDialog'
import { cn } from '@/lib/utils'

function isToday(iso: string) {
  const d = new Date(iso)
  const now = new Date()
  return (
    d.getFullYear() === now.getFullYear() &&
    d.getMonth()    === now.getMonth()    &&
    d.getDate()     === now.getDate()
  )
}

type FilterTab = 'all' | Exclude<WorkStatus, 'deleted'>

const TABS: { value: FilterTab; label: string }[] = [
  { value: 'all',       label: 'Активные' },
  { value: 'new',       label: 'Новые' },
  { value: 'todo',      label: 'Запланированные' },
  { value: 'running',   label: 'В работе' },
  { value: 'completed', label: 'Завершённые' },
]

export function TasksPage() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [filter, setFilter]             = useState<FilterTab>('all')
  const [createOpen, setCreateOpen]     = useState(false)
  const [editTask, setEditTask]         = useState<Task | null>(null)
  const [feedbackTask, setFeedbackTask]       = useState<Task | null>(null)
  const [doneFeedbacks, setDoneFeedbacks] = useState<Set<string>>(() => {
    try {
      const raw = localStorage.getItem('doneFeedbacks')
      return raw ? new Set<string>(JSON.parse(raw)) : new Set<string>()
    } catch {
      return new Set<string>()
    }
  })

  const markFeedbackDone = (taskId: string) => {
    setDoneFeedbacks(prev => {
      const next = new Set(prev).add(taskId)
      try { localStorage.setItem('doneFeedbacks', JSON.stringify([...next])) } catch { /* ignore */ }
      return next
    })
  }

  const { data: stateData } = useQuery({
    queryKey: ['userState', 'latest'],
    queryFn: () => userStateApi.getLatest(),
    retry: false,
  })

  const hasStateToday = stateData?.data.payload
    ? isToday(stateData.data.payload.createdAt)
    : false

  const { data, isLoading, isError } = useQuery({
    queryKey: ['tasks', filter],
    queryFn: () =>
      tasksApi.getAll(filter === 'all' ? { pageSize: 100 } : { workStatus: filter, pageSize: 100 }),
  })

  const tasks = data?.data.payload.tasks ?? []

  const statusMutation = useMutation({
    mutationFn: ({ taskId, status }: { taskId: string; status: Exclude<WorkStatus, 'deleted'> }) =>
      tasksApi.updateStatus(taskId, status),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      if (vars.status === 'completed') {
        const task = tasks.find(t => t.id === vars.taskId)
        if (task) setFeedbackTask({ ...task, workStatus: 'completed' })
      }
    },
    onError: () => toast.error('Не удалось обновить статус'),
  })

  const deleteMutation = useMutation({
    mutationFn: (taskId: string) => tasksApi.delete(taskId),
    onSuccess: () => {
      toast.success('Задача удалена')
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
    },
    onError: () => toast.error('Не удалось удалить задачу'),
  })

  const taskCount = tasks.length
  const countLabel =
    taskCount === 1 ? '1 задача' :
    taskCount < 5   ? `${taskCount} задачи` :
                      `${taskCount} задач`

  return (
    <div className="p-6 max-w-2xl mx-auto w-full">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Задачи</h1>
          {!isLoading && (
            <p className="text-sm text-muted-foreground mt-0.5">{countLabel}</p>
          )}
        </div>
        <Button
          onClick={() => setCreateOpen(true)}
          disabled={!hasStateToday}
          className="bg-indigo-600 text-white hover:bg-indigo-500 gap-1.5 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Plus className="h-4 w-4" />
          Создать
        </Button>
      </div>

      {/* No state today banner */}
      {!hasStateToday && (
        <div className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950/40 px-4 py-3 mb-6">
          <AlertCircle className="h-4 w-4 text-amber-600 dark:text-amber-400 mt-0.5 shrink-0" />
          <p className="text-sm text-amber-800 dark:text-amber-300">
            Чтобы создавать задачи, сначала{' '}
            <button
              onClick={() => navigate('/profile')}
              className="font-medium underline underline-offset-2 hover:no-underline"
            >
              добавьте состояние на сегодня
            </button>
          </p>
        </div>
      )}

      {/* Filter tabs */}
      <div className="flex gap-1 bg-muted/40 rounded-xl p-1 mb-6">
        {TABS.map(tab => (
          <button
            key={tab.value}
            onClick={() => setFilter(tab.value)}
            className={cn(
              'flex-1 text-xs font-medium rounded-lg py-1.5 transition-colors',
              filter === tab.value
                ? 'bg-card text-foreground shadow-sm'
                : 'text-muted-foreground hover:text-foreground'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Loading */}
      {isLoading && (
        <div className="flex justify-center py-16">
          <div className="h-6 w-6 rounded-full border-2 border-indigo-600 border-t-transparent animate-spin" />
        </div>
      )}

      {/* Error */}
      {isError && (
        <p className="text-center text-sm text-muted-foreground py-16">
          Не удалось загрузить задачи
        </p>
      )}

      {/* Empty state */}
      {!isLoading && !isError && tasks.length === 0 && (
        <div className="text-center py-16">
          <p className="text-muted-foreground text-sm">Задач пока нет</p>
          <button
            onClick={() => setCreateOpen(true)}
            className="mt-2 text-sm text-indigo-600 hover:text-indigo-700 font-medium"
          >
            Создать первую →
          </button>
        </div>
      )}

      {/* Task list */}
      <div className="space-y-3">
        <AnimatePresence mode="popLayout">
          {tasks.map(task => (
            <TaskCard
              key={task.id}
              task={task}
              onStatusChange={(id, status) => statusMutation.mutate({ taskId: id, status })}
              onDelete={id => deleteMutation.mutate(id)}
              onEdit={t => setEditTask(t)}
              onFeedback={t => setFeedbackTask(t)}
              feedbackDone={doneFeedbacks.has(task.id)}
            />
          ))}
        </AnimatePresence>
      </div>

      {/* Dialogs */}
      <CreateTaskDialog open={createOpen} onClose={() => setCreateOpen(false)} />
      {editTask && (
        <EditTaskDialog
          open
          onClose={() => setEditTask(null)}
          task={editTask}
        />
      )}
      {feedbackTask && (
        <TaskExecutionDialog
          open
          onClose={() => setFeedbackTask(null)}
          onDone={() => markFeedbackDone(feedbackTask.id)}
          taskId={feedbackTask.id}
          taskTitle={feedbackTask.title}
        />
      )}
    </div>
  )
}
