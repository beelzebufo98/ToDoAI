import type { UserState } from '@/api/userState'
import { cn } from '@/lib/utils'
import { formatSleepLabel, formatStateDate, type DailyStateSummary } from './state-utils'

function Metric({
  label,
  value,
  colorInvert,
}: {
  label: string
  value: number
  colorInvert?: boolean
}) {
  const isLow = value <= 3
  const isHigh = value >= 8
  const good = colorInvert ? isLow : isHigh
  const bad = colorInvert ? isHigh : isLow

  return (
    <div className="flex items-center justify-between gap-4">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span
        className={cn(
          'text-xs font-semibold tabular-nums',
          good ? 'text-green-600' : bad ? 'text-red-500' : 'text-foreground'
        )}
      >
        {value}/10
      </span>
    </div>
  )
}

export function StateCard({
  state,
  compact = false,
}: {
  state: UserState | DailyStateSummary
  compact?: boolean
}) {
  const stateDate = 'createdAt' in state ? state.createdAt : state.date

  return (
    <div className={cn('bg-card border border-border/50 rounded-xl', compact ? 'p-4' : 'p-5')}>
      <div className={cn('flex items-center justify-between gap-4', compact ? 'mb-3' : 'mb-4')}>
        <span className="text-sm text-muted-foreground">{formatStateDate(stateDate)}</span>
        <span className="text-sm font-semibold text-foreground">Сон: {formatSleepLabel(state.sleepMinutes)}</span>
      </div>

      <div className="grid gap-x-8 gap-y-2 sm:grid-cols-2">
        <Metric label="Энергия" value={state.energyLevel} />
        <Metric label="Стресс" value={state.stressLevel} colorInvert />
        <Metric label="Мотивация" value={state.motivationLevel} />
        <Metric label="Концентрация" value={state.concentrationLevel} />
      </div>
    </div>
  )
}
