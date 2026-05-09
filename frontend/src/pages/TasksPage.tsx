import { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { AnimatePresence } from 'framer-motion'
import { Plus, AlertCircle } from 'lucide-react'
import { toast } from 'sonner'
import { useNavigate } from 'react-router-dom'
import { tasksApi } from '@/api/tasks'
import type { Task, WorkStatus } from '@/api/tasks'
import { userStateApi } from '@/api/userState'
import { useTaskSession } from '@/contexts/TaskSessionContext'
import { Button } from '@/components/ui/button'
import { InfoHint } from '@/components/ui/info-hint'
import { TaskCard, type SessionState } from '@/components/tasks/TaskCard'
import { CreateTaskDialog } from '@/components/tasks/CreateTaskDialog'
import { EditTaskDialog } from '@/components/tasks/EditTaskDialog'
import { TaskExecutionDialog } from '@/components/tasks/TaskExecutionDialog'
import { cn } from '@/lib/utils'

function isToday(iso: string) {
  const d = new Date(iso)
  const now = new Date()
  return (
    d.getFullYear() === now.getFullYear() &&
    d.getMonth() === now.getMonth() &&
    d.getDate() === now.getDate()
  )
}

type FilterTab = 'all' | Exclude<WorkStatus, 'deleted'>

const TABS: { value: FilterTab; label: string }[] = [
  { value: 'all', label: 'Активные' },
  { value: 'new', label: 'Новые' },
  { value: 'todo', label: 'Запланированные' },
  { value: 'running', label: 'В работе' },
  { value: 'completed', label: 'Завершённые' },
]

export function TasksPage() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const { current: activeSession, syncing: sessionSyncing, startSession, stopSession, cancelSession } = useTaskSession()
  const [showGuideHint, setShowGuideHint] = useState(false)
  const [filter, setFilter] = useState<FilterTab>('all')
  const [createOpen, setCreateOpen] = useState(false)
  const [editTask, setEditTask] = useState<Task | null>(null)
  const [feedbackTask, setFeedbackTask] = useState<Task | null>(null)
  const [doneFeedbacks, setDoneFeedbacks] = useState<Set<string>>(() => {
    try {
      const raw = localStorage.getItem('doneFeedbacks')
      return raw ? new Set<string>(JSON.parse(raw)) : new Set<string>()
    } catch {
      return new Set<string>()
    }
  })

  const markFeedbackDone = (taskId: string) => {
    setDoneFeedbacks((prev) => {
      const next = new Set(prev).add(taskId)
      try {
        localStorage.setItem('doneFeedbacks', JSON.stringify([...next]))
      } catch {
        // ignore
      }
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

  useEffect(() => {
    try {
      setShowGuideHint(localStorage.getItem('tasksGuideHintDismissed') !== 'true')
    } catch {
      setShowGuideHint(true)
    }
  }, [])

  const dismissGuideHint = () => {
    setShowGuideHint(false)
    try {
      localStorage.setItem('tasksGuideHintDismissed', 'true')
    } catch {
      // ignore storage errors
    }
  }

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
      queryClient.invalidateQueries({ queryKey: ['task', vars.taskId] })
      queryClient.invalidateQueries({ queryKey: ['schedule'] })
      if (vars.status === 'completed') {
        const task = tasks.find((t) => t.id === vars.taskId)
        if (task) {
          setFeedbackTask({ ...task, workStatus: 'completed' })
        }
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
    taskCount >= 2 && taskCount <= 4 ? `${taskCount} задачи` :
    `${taskCount} задач`

  return (
    <div className="mx-auto w-full max-w-2xl p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Задачи</h1>
          {!isLoading && <p className="mt-0.5 text-sm text-muted-foreground">{countLabel}</p>}
        </div>
        <Button
          onClick={() => setCreateOpen(true)}
          disabled={!hasStateToday}
          className="gap-1.5 bg-indigo-600 text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <Plus className="h-4 w-4" />
          Создать
        </Button>
      </div>

      {!hasStateToday && (
        <div className="mb-6 flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 dark:border-amber-800 dark:bg-amber-950/40">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
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

      {hasStateToday && showGuideHint && (
        <InfoHint
          className="mb-6"
          title="Как пользоваться задачами"
          description="Создавайте задачу вручную или через AI-подсказку, затем переводите ее в работу и отмечайте результат после завершения. Так планирование и статистика будут точнее."
          onDismiss={dismissGuideHint}
        />
      )}

      <div className="mb-6 flex gap-1 rounded-xl bg-muted/40 p-1">
        {TABS.map((tab) => (
          <button
            key={tab.value}
            onClick={() => setFilter(tab.value)}
            className={cn(
              'flex-1 rounded-lg py-1.5 text-xs font-medium transition-colors',
              filter === tab.value
                ? 'bg-card text-foreground shadow-sm'
                : 'text-muted-foreground hover:text-foreground',
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <div className="h-6 w-6 animate-spin rounded-full border-2 border-indigo-600 border-t-transparent" />
        </div>
      )}

      {isError && (
        <p className="py-16 text-center text-sm text-muted-foreground">
          Не удалось загрузить задачи
        </p>
      )}

      {!isLoading && !isError && tasks.length === 0 && (
        <div className="py-16 text-center">
          <p className="text-sm text-muted-foreground">Задач пока нет</p>
          {hasStateToday && (
            <button
              onClick={() => setCreateOpen(true)}
              className="mt-2 text-sm font-medium text-indigo-600 hover:text-indigo-700"
            >
              Создать первую →
            </button>
          )}
        </div>
      )}

      <div className="space-y-3">
        <AnimatePresence mode="popLayout">
          {tasks.map((task) => {
            const sessionState: SessionState =
              activeSession?.taskId === task.id ? 'active' :
              activeSession ? 'other' : 'none'

            return (
              <TaskCard
                key={task.id}
                task={task}
                onStatusChange={(id, status) => statusMutation.mutate({ taskId: id, status })}
                onDelete={(id) => deleteMutation.mutate(id)}
                onEdit={(t) => setEditTask(t)}
                onFeedback={(t) => setFeedbackTask(t)}
                feedbackDone={doneFeedbacks.has(task.id)}
                sessionState={sessionState}
                sessionSyncing={sessionSyncing}
                activeSessionStartedAt={activeSession?.taskId === task.id ? activeSession.startedAt : undefined}
                onStartSession={() => startSession(task.id)}
                onStopSession={stopSession}
                onCancelSession={cancelSession}
              />
            )
          })}
        </AnimatePresence>
      </div>

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
