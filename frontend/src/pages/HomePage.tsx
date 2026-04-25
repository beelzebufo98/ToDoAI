import { useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { userStateApi } from '@/api/userState'
import { scheduleApi } from '@/api/schedule'
import { taskWorkSessionsApi, type TaskWorkSession } from '@/api/taskWorkSessions'
import { NoStateTodayCard } from '@/components/schedule/NoStateTodayCard'
import { GenerateDayCard } from '@/components/schedule/GenerateDayCard'
import { DayScheduleTimeline } from '@/components/schedule/DayScheduleTimeline'
import { cn } from '@/lib/utils'

// Local date for schedule (schedule startAt is built in local timezone)
function todayLocalString() {
  const d = new Date()
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

// UTC date for session history — backend filters task-work-sessions by UTC day range
function todayUtcString() {
  return new Date().toISOString().slice(0, 10)
}

function isToday(iso: string) {
  const d = new Date(iso)
  const now = new Date()
  return (
    d.getFullYear() === now.getFullYear() &&
    d.getMonth()    === now.getMonth()    &&
    d.getDate()     === now.getDate()
  )
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

function SessionHistoryItem({ session }: { session: TaskWorkSession }) {
  const isCompleted = session.status === 'completed'
  const isCancelled = session.status === 'cancelled'

  return (
    <div className="flex items-center gap-3 py-2">
      <div className={cn(
        'h-2 w-2 rounded-full shrink-0',
        isCompleted ? 'bg-green-500' : isCancelled ? 'bg-gray-300' : 'bg-amber-500'
      )} />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-foreground truncate">{session.title ?? '—'}</p>
        <p className="text-xs text-muted-foreground">
          {formatTime(session.startedAt)}
          {session.endedAt && ` – ${formatTime(session.endedAt)}`}
        </p>
      </div>
      <div className="text-right shrink-0">
        {isCompleted && session.spentMinutes != null && (
          <span className="text-xs font-medium text-green-600">{session.spentMinutes} мин</span>
        )}
        {isCancelled && (
          <span className="text-xs text-muted-foreground">отменено</span>
        )}
      </div>
    </div>
  )
}

export function HomePage() {
  const today = todayLocalString()
  const todayUtc = todayUtcString()
  const queryClient = useQueryClient()
  const [showGenerate, setShowGenerate] = useState(false)

  const { data: stateData, isLoading: stateLoading } = useQuery({
    queryKey: ['userState', 'latest'],
    queryFn: () => userStateApi.getLatest(),
    retry: false,
  })

  const { data: scheduleData, isLoading: scheduleLoading, error: scheduleError } = useQuery({
    queryKey: ['schedule', today],
    queryFn: () => scheduleApi.getDay(today),
    retry: false,
  })

  const { data: sessionsData } = useQuery({
    queryKey: ['sessions', 'day', todayUtc],
    queryFn: () => taskWorkSessionsApi.getByDate(todayUtc).then(r => r.data.payload.taskWorkSessionResponses),
    staleTime: 0,
    retry: false,
  })

  const hasStateToday = stateData?.data.payload
    ? isToday(stateData.data.payload.createdAt)
    : false

  const scheduleNotFound =
    !scheduleData && axios.isAxiosError(scheduleError) && scheduleError.response?.status === 404

  const scheduleLoadError =
    !scheduleData && !!scheduleError && !scheduleNotFound

  const schedule = scheduleData?.data.payload ?? null

  const isLoading = stateLoading || scheduleLoading

  const finishedSessions = (sessionsData ?? []).filter(s => s.status !== 'open')
  const totalMinutes = finishedSessions
    .filter(s => s.status === 'completed' && s.spentMinutes != null)
    .reduce((sum, s) => sum + (s.spentMinutes ?? 0), 0)

  return (
    <div className="p-6 max-w-2xl mx-auto w-full">
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-foreground">Главная</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          {new Date().toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' })}
        </p>
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <div className="h-6 w-6 rounded-full border-2 border-indigo-600 border-t-transparent animate-spin" />
        </div>
      )}

      {!isLoading && (
        <div className="space-y-5">
          {!hasStateToday && <NoStateTodayCard />}

          {hasStateToday && scheduleLoadError && (
            <p className="text-center text-sm text-muted-foreground py-16">
              Не удалось загрузить расписание — попробуйте обновить страницу
            </p>
          )}

          {hasStateToday && !scheduleLoadError && (
            <div className="bg-card border border-border/50 rounded-xl p-5">
              {schedule && !showGenerate ? (
                <DayScheduleTimeline
                  schedule={schedule}
                  onRegenerate={() => setShowGenerate(true)}
                />
              ) : (
                <GenerateDayCard
                  scheduleDate={today}
                  preselectedTaskIds={schedule?.blocks.map(block => block.taskId) ?? []}
                  onGenerated={() => {
                    queryClient.invalidateQueries({ queryKey: ['schedule', today] })
                    setShowGenerate(false)
                  }}
                  onCancel={schedule ? () => setShowGenerate(false) : undefined}
                />
              )}
            </div>
          )}

          {/* Today's session history */}
          {finishedSessions.length > 0 && (
            <div className="bg-card border border-border/50 rounded-xl p-5">
              <div className="flex items-center justify-between mb-3">
                <h2 className="text-sm font-semibold text-foreground">Сессии за сегодня</h2>
                {totalMinutes > 0 && (
                  <span className="text-xs font-medium text-green-600">{totalMinutes} мин суммарно</span>
                )}
              </div>
              <div className="divide-y divide-border/40">
                {finishedSessions.map(s => (
                  <SessionHistoryItem key={s.sessionId} session={s} />
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
