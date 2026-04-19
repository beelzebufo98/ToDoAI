import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import axios from 'axios'
import { userStateApi } from '@/api/userState'
import { scheduleApi, type DaySchedule } from '@/api/schedule'
import { NoStateTodayCard } from '@/components/schedule/NoStateTodayCard'
import { GenerateDayCard } from '@/components/schedule/GenerateDayCard'
import { DayScheduleTimeline } from '@/components/schedule/DayScheduleTimeline'

function todayString() {
  const d = new Date()
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
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

export function HomePage() {
  const today = todayString()

  // optimistic schedule: when user generates, show immediately without waiting for refetch
  const [localSchedule, setLocalSchedule] = useState<DaySchedule | null>(null)
  const [showGenerate, setShowGenerate]   = useState(false)

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

  const hasStateToday = stateData?.data.payload
    ? isToday(stateData.data.payload.createdAt)
    : false

  const scheduleNotFound =
    !scheduleData && axios.isAxiosError(scheduleError) && scheduleError.response?.status === 404

  const scheduleLoadError =
    !scheduleData && !!scheduleError && !scheduleNotFound

  const schedule: DaySchedule | null =
    localSchedule ?? scheduleData?.data.payload ?? null

  const isLoading = stateLoading || scheduleLoading

  return (
    <div className="p-6 max-w-2xl mx-auto w-full">
      {/* Header */}
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
        <>
          {/* State 1: no user state today */}
          {!hasStateToday && <NoStateTodayCard />}

          {/* Schedule load error (non-404) */}
          {hasStateToday && scheduleLoadError && (
            <p className="text-center text-sm text-muted-foreground py-16">
              Не удалось загрузить расписание — попробуйте обновить страницу
            </p>
          )}

          {/* State 2: has state, show schedule or generate form */}
          {hasStateToday && !scheduleLoadError && (
            <div className="bg-card border border-border/50 rounded-xl p-5">
              {schedule && !showGenerate ? (
                <DayScheduleTimeline
                  schedule={schedule}
                  onRegenerate={() => {
                    setLocalSchedule(null)
                    setShowGenerate(true)
                  }}
                />
              ) : (
                <GenerateDayCard
                  scheduleDate={today}
                  onGenerated={s => {
                    setLocalSchedule(s)
                    setShowGenerate(false)
                  }}
                />
              )}
            </div>
          )}
        </>
      )}
    </div>
  )
}
