import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { BarChart3, CalendarRange, Rows3, TrendingUp } from 'lucide-react'
import { userStateApi, type DailyUserStateStatistics } from '@/api/userState'
import { StateCard } from '@/components/user-state/StateCard'
import { formatSleepLabel, isSameLocalDate } from '@/components/user-state/state-utils'

function SummaryMetricCard({
  label,
  value,
  hint,
  accent = false,
}: {
  label: string
  value: string
  hint?: string
  accent?: boolean
}) {
  return (
    <div
      className={[
        'rounded-2xl border p-4 transition-colors',
        accent ? 'border-emerald-200/70 bg-emerald-50/60' : 'border-border/50 bg-card',
      ].join(' ')}
    >
      <p className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted-foreground">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-foreground">{value}</p>
      {hint ? <p className="mt-1.5 text-xs text-muted-foreground">{hint}</p> : null}
    </div>
  )
}

function toDailySummary(entry: DailyUserStateStatistics) {
  return {
    day: entry.createdDate,
    date: entry.createdDate,
    sleepMinutes: entry.sleepMinutes,
    energyLevel: entry.energyLevel,
    stressLevel: entry.stressLevel,
    motivationLevel: entry.motivationLevel,
    concentrationLevel: entry.concentrationLevel,
    count: entry.entriesCount,
  }
}

export function StatisticsPage() {
  const { data: latestData, isLoading: latestLoading } = useQuery({
    queryKey: ['userState', 'latest'],
    queryFn: () => userStateApi.getLatest(),
    retry: false,
  })

  const { data: statisticsData, isLoading: statisticsLoading } = useQuery({
    queryKey: ['userState', 'statistics', 30],
    queryFn: () => userStateApi.getStatistics(30),
    retry: false,
  })

  const latest = latestData?.data.payload
  const statistics = statisticsData?.data.payload

  const dailyHistory = useMemo(() => {
    const latestIsToday = latest ? isSameLocalDate(latest.createdAt) : false

    return (statistics?.dateStatistics ?? [])
      .filter((entry) => !(latestIsToday && latest && entry.createdDate === latest.createdAt.slice(0, 10)))
      .slice()
      .sort((left, right) => right.createdDate.localeCompare(left.createdDate))
      .map(toDailySummary)
  }, [latest, statistics])

  return (
    <div className="p-6 max-w-5xl mx-auto w-full">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-foreground">Статистика</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Динамика состояния и средние показатели за последние {statistics?.periodDays ?? 30} дней
        </p>
      </div>

      {(latestLoading || statisticsLoading) && (
        <div className="flex justify-center py-16">
          <div className="h-6 w-6 rounded-full border-2 border-indigo-600 border-t-transparent animate-spin" />
        </div>
      )}

      {!latestLoading && !statisticsLoading && (
        <div className="space-y-7">
          {statistics && statistics.entriesCount > 0 ? (
            <>
              <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
                <SummaryMetricCard
                  label="Средний сон"
                  value={formatSleepLabel(statistics.averages.sleepMinutes)}
                  hint="Среднее значение по всем записям периода"
                  accent
                />
                <SummaryMetricCard label="Средняя энергия" value={`${statistics.averages.energyLevel}/10`} />
                <SummaryMetricCard label="Средний стресс" value={`${statistics.averages.stressLevel}/10`} />
                <SummaryMetricCard label="Средняя мотивация" value={`${statistics.averages.motivationLevel}/10`} />
                <SummaryMetricCard label="Средняя концентрация" value={`${statistics.averages.concentrationLevel}/10`} />
                <SummaryMetricCard
                  label="Дней с записями"
                  value={String(statistics.daysWithEntries)}
                  hint="Дней, в которые вы отмечали состояние"
                />
                <SummaryMetricCard
                  label="Всего записей"
                  value={String(statistics.entriesCount)}
                  hint="Все сохраненные состояния за период"
                />
                <SummaryMetricCard
                  label="Период"
                  value={`${statistics.periodDays} дн.`}
                  hint="Текущее окно расчета статистики"
                />
              </section>

              {latest ? (
                <section>
                  <div className="mb-2 flex items-center gap-2">
                    <TrendingUp className="h-4 w-4 text-muted-foreground" />
                    <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                      Последняя запись
                    </p>
                  </div>
                  <StateCard state={latest} compact />
                </section>
              ) : null}

              {dailyHistory.length > 0 ? (
                <section>
                  <div className="mb-2 flex items-center gap-2">
                    <BarChart3 className="h-4 w-4 text-muted-foreground" />
                    <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                      История по дням
                    </p>
                  </div>

                  <div className="space-y-3">
                    {dailyHistory.map((entry) => (
                      <div key={entry.day} className="space-y-1.5">
                        <StateCard state={entry} compact />
                        {entry.count > 1 ? (
                          <p className="pl-1 text-xs text-muted-foreground">
                            Средние значения за {entry.count} записи этого дня
                          </p>
                        ) : null}
                      </div>
                    ))}
                  </div>
                </section>
              ) : null}
            </>
          ) : (
            <div className="rounded-2xl border border-dashed border-border/60 bg-card/50 px-6 py-10 text-center">
              <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-muted">
                <CalendarRange className="h-5 w-5 text-muted-foreground" />
              </div>
              <p className="text-sm font-medium text-foreground">Пока недостаточно данных для статистики</p>
              <p className="mt-1 text-sm text-muted-foreground">
                Добавьте несколько записей о состоянии, и здесь появится сводка по дням.
              </p>
            </div>
          )}

          {statistics && statistics.entriesCount > 0 && dailyHistory.length === 0 ? (
            <div className="rounded-2xl border border-border/40 bg-muted/20 px-4 py-3">
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Rows3 className="h-4 w-4" />
                История за предыдущие дни появится, когда накопится больше одного дня наблюдений.
              </div>
            </div>
          ) : null}
        </div>
      )}
    </div>
  )
}
