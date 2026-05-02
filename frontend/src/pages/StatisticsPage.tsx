import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { TrendingUp } from 'lucide-react'
import { userStateApi } from '@/api/userState'
import { StateCard } from '@/components/user-state/StateCard'
import { formatSleepLabel, groupStatesByDay, isSameLocalDate } from '@/components/user-state/state-utils'

function SummaryMetricCard({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <div className="bg-card border border-border/50 rounded-xl p-4">
      <p className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted-foreground">{label}</p>
      <p className="mt-2 text-xl font-semibold text-foreground">{value}</p>
    </div>
  )
}

export function StatisticsPage() {
  const { data: latestData, isLoading: latestLoading } = useQuery({
    queryKey: ['userState', 'latest'],
    queryFn: () => userStateApi.getLatest(),
    retry: false,
  })

  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['userState', 'history'],
    queryFn: () => userStateApi.getHistory(30),
    retry: false,
  })

  const latest = latestData?.data.payload
  const history = historyData?.data.payload.history ?? []

  const stats = useMemo(() => {
    if (history.length === 0) {
      return null
    }

    const average = (selector: (value: (typeof history)[number]) => number) =>
      Math.round(history.reduce((sum, item) => sum + selector(item), 0) / history.length)

    return {
      averageSleepMinutes: average((item) => item.sleepMinutes),
      averageEnergy: average((item) => item.energyLevel),
      averageStress: average((item) => item.stressLevel),
      averageMotivation: average((item) => item.motivationLevel),
      averageConcentration: average((item) => item.concentrationLevel),
    }
  }, [history])

  const groupedHistory = useMemo(
    () => groupStatesByDay(history, { excludeToday: !!latest && isSameLocalDate(latest.createdAt) }),
    [history, latest]
  )

  return (
    <div className="p-6 max-w-4xl mx-auto w-full">
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-foreground">Статистика</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          Средние показатели состояния за последние 30 записей
        </p>
      </div>

      {(latestLoading || historyLoading) && (
        <div className="flex justify-center py-16">
          <div className="h-6 w-6 rounded-full border-2 border-indigo-600 border-t-transparent animate-spin" />
        </div>
      )}

      {!latestLoading && !historyLoading && (
        <div className="space-y-6">
          {stats && (
            <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              <SummaryMetricCard label="Средний сон" value={formatSleepLabel(stats.averageSleepMinutes)} />
              <SummaryMetricCard label="Энергия" value={`${stats.averageEnergy}/10`} />
              <SummaryMetricCard label="Стресс" value={`${stats.averageStress}/10`} />
              <SummaryMetricCard label="Мотивация" value={`${stats.averageMotivation}/10`} />
              <SummaryMetricCard label="Концентрация" value={`${stats.averageConcentration}/10`} />
            </section>
          )}

          {latest && (
            <section>
              <div className="flex items-center gap-2 mb-2">
                <TrendingUp className="h-4 w-4 text-muted-foreground" />
                <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Последняя запись
                </p>
              </div>
              <StateCard state={latest} compact />
            </section>
          )}

          {groupedHistory.length > 0 ? (
            <section>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide mb-2">
                История по дням
              </p>
              <div className="space-y-2.5">
                {groupedHistory.map((entry) => (
                  <div key={entry.day}>
                    <StateCard state={entry} compact />
                    {entry.count > 1 && (
                      <p className="mt-1.5 text-xs text-muted-foreground">
                        Среднее значение за {entry.count} записи этого дня
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </section>
          ) : (
            <p className="text-center text-sm text-muted-foreground py-8">
              История появится после накопления записей за несколько дней
            </p>
          )}
        </div>
      )}
    </div>
  )
}
