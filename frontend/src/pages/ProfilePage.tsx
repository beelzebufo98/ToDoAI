import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { userStateApi } from '@/api/userState'
import type { UserState } from '@/api/userState'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

const LEVELS = [
  { label: '1–2', value: 2  },
  { label: '3–4', value: 4  },
  { label: '5–6', value: 6  },
  { label: '7–8', value: 8  },
  { label: '9–10', value: 10 },
]

const schema = z.object({
  sleepHours:          z.number().min(0, 'Минимум 0').max(24, 'Максимум 24'),
  energyLevel:         z.number().min(1).max(10),
  stressLevel:         z.number().min(1).max(10),
  motivationLevel:     z.number().min(1).max(10),
  concentrationLevel:  z.number().min(1).max(10),
})

type FormData = z.infer<typeof schema>

function LevelSelector({
  label,
  value,
  onChange,
}: {
  label: string
  value: number
  onChange: (v: number) => void
}) {
  return (
    <div className="space-y-1.5">
      <Label className="text-sm font-medium">
        {label}
        <span className="ml-1.5 font-normal text-muted-foreground">({value}/10)</span>
      </Label>
      <div className="flex gap-1.5">
        {LEVELS.map(l => (
          <button
            key={l.value}
            type="button"
            onClick={() => onChange(l.value)}
            className={cn(
              'flex-1 rounded-lg py-1.5 text-xs font-medium transition-colors border',
              value === l.value
                ? 'bg-foreground text-background border-foreground'
                : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
            )}
          >
            {l.label}
          </button>
        ))}
      </div>
    </div>
  )
}

function isWithin3Hours(iso: string) {
  return Date.now() - new Date(iso).getTime() < 3 * 60 * 60 * 1000
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

function StateCard({ state }: { state: UserState }) {
  const h = Math.floor(state.sleepMinutes / 60)
  const m = state.sleepMinutes % 60
  const sleepLabel = m > 0 ? `${h} ч ${m} мин` : `${h} ч`

  return (
    <div className="bg-card border border-border/50 rounded-xl p-4">
      <div className="flex items-center justify-between mb-3">
        <span className="text-xs text-muted-foreground">{formatDate(state.createdAt)}</span>
        <span className="text-xs font-medium text-foreground">Сон: {sleepLabel}</span>
      </div>
      <div className="grid grid-cols-2 gap-x-6 gap-y-1.5">
        <Metric label="Энергия"       value={state.energyLevel} />
        <Metric label="Стресс"        value={state.stressLevel} colorInvert />
        <Metric label="Мотивация"     value={state.motivationLevel} />
        <Metric label="Концентрация"  value={state.concentrationLevel} />
      </div>
    </div>
  )
}

function Metric({ label, value, colorInvert }: { label: string; value: number; colorInvert?: boolean }) {
  const isLow  = value <= 3
  const isHigh = value >= 8
  const good   = colorInvert ? isLow : isHigh
  const bad    = colorInvert ? isHigh : isLow

  return (
    <div className="flex items-center justify-between">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className={cn(
        'text-xs font-semibold tabular-nums',
        good ? 'text-green-600' : bad ? 'text-red-500' : 'text-foreground'
      )}>
        {value}/10
      </span>
    </div>
  )
}

export function ProfilePage() {
  const queryClient = useQueryClient()

  const { register, handleSubmit, formState: { errors }, setValue, watch, reset } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      sleepHours: 8,
      energyLevel: 6,
      stressLevel: 4,
      motivationLevel: 6,
      concentrationLevel: 6,
    },
  })

  const energyLevel        = watch('energyLevel')
  const stressLevel        = watch('stressLevel')
  const motivationLevel    = watch('motivationLevel')
  const concentrationLevel = watch('concentrationLevel')

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

  const latest  = latestData?.data.payload
  const allHistory = historyData?.data.payload.history ?? []
  const today = new Date().toDateString()
  const history = Object.entries(
    allHistory.reduce<Record<string, typeof allHistory>>((acc, s) => {
      const day = new Date(s.createdAt).toDateString()
      if (day !== today) { (acc[day] ??= []).push(s) }
      return acc
    }, {})
  )
    .map(([day, entries]) => {
      const avg = (fn: (s: typeof entries[0]) => number) =>
        Math.round(entries.reduce((sum, s) => sum + fn(s), 0) / entries.length)
      return {
        day,
        date: entries[0].createdAt,
        sleepMinutes:       avg(s => s.sleepMinutes),
        energyLevel:        avg(s => s.energyLevel),
        stressLevel:        avg(s => s.stressLevel),
        motivationLevel:    avg(s => s.motivationLevel),
        concentrationLevel: avg(s => s.concentrationLevel),
        count: entries.length,
      }
    })
    .sort((a, b) => b.date.localeCompare(a.date))
  const isUpdate = !!latest && isWithin3Hours(latest.createdAt)

  // Pre-fill once when the record first loads (or a new record appears).
  // Intentionally excludes `isUpdate` — it can briefly toggle during refetch
  // and would re-run the effect with stale cache data, overwriting user edits.
  useEffect(() => {
    if (latest && isWithin3Hours(latest.createdAt)) {
      reset({
        sleepHours:         latest.sleepMinutes / 60,
        energyLevel:        latest.energyLevel,
        stressLevel:        latest.stressLevel,
        motivationLevel:    latest.motivationLevel,
        concentrationLevel: latest.concentrationLevel,
      })
    }
  }, [latest?.id]) // eslint-disable-line react-hooks/exhaustive-deps

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      userStateApi.create({
        sleepMinutes:       Math.round(data.sleepHours * 60),
        energyLevel:        data.energyLevel,
        stressLevel:        data.stressLevel,
        motivationLevel:    data.motivationLevel,
        concentrationLevel: data.concentrationLevel,
      }),
    onSuccess: () => {
      toast.success(isUpdate ? 'Состояние обновлено' : 'Состояние сохранено')
      queryClient.invalidateQueries({ queryKey: ['userState'] })
      if (!isUpdate) reset()
    },
    onError: () => toast.error('Не удалось сохранить состояние'),
  })

  return (
    <div className="p-6 max-w-2xl mx-auto w-full">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-foreground">Профиль</h1>
        <p className="text-sm text-muted-foreground mt-0.5">Ваше состояние на сегодня</p>
      </div>

      <div className="space-y-6">
        {/* Latest state */}
        {!latestLoading && latest && (
          <div>
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide mb-2">
              Последнее состояние
            </p>
            <StateCard state={latest} />
          </div>
        )}

        {/* Create form */}
        <div className="bg-card border border-border/50 rounded-xl p-5">
          <h2 className="text-sm font-semibold mb-4">{isUpdate ? 'Обновить состояние' : 'Добавить состояние'}</h2>

          <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-4">
            {/* Sleep */}
            <div className="space-y-1.5">
              <Label className="text-sm font-medium">Сон (часов)</Label>
              <Input
                type="number"
                min={0}
                max={24}
                step={0.5}
                className="h-10 bg-background/80"
                {...register('sleepHours', { valueAsNumber: true })}
              />
              {errors.sleepHours && (
                <p className="text-xs text-destructive">{errors.sleepHours.message}</p>
              )}
            </div>

            <LevelSelector label="Энергия"      value={energyLevel}        onChange={v => setValue('energyLevel', v)} />
            <LevelSelector label="Стресс"        value={stressLevel}        onChange={v => setValue('stressLevel', v)} />
            <LevelSelector label="Мотивация"     value={motivationLevel}    onChange={v => setValue('motivationLevel', v)} />
            <LevelSelector label="Концентрация"  value={concentrationLevel} onChange={v => setValue('concentrationLevel', v)} />

            <Button
              type="submit"
              disabled={mutation.isPending}
              className="w-full bg-indigo-600 text-white hover:bg-indigo-500 mt-2"
            >
              {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
              {isUpdate ? 'Обновить' : 'Сохранить'}
            </Button>
          </form>
        </div>

        {/* History */}
        {!historyLoading && history.length > 0 && (
          <div>
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide mb-2">
              История ({history.length})
            </p>
            <div className="space-y-3">
              {history.map(entry => (
                <div key={entry.day} className="bg-card border border-border/50 rounded-xl p-4">
                  <div className="flex items-center justify-between mb-3">
                    <span className="text-xs text-muted-foreground">
                      {new Date(entry.date).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' })}
                      {entry.count > 1 && <span className="ml-1.5 text-muted-foreground/60">· ср. за {entry.count} записи</span>}
                    </span>
                    <span className="text-xs font-medium text-foreground">
                      Сон: {Math.floor(entry.sleepMinutes / 60)} ч{entry.sleepMinutes % 60 > 0 ? ` ${entry.sleepMinutes % 60} мин` : ''}
                    </span>
                  </div>
                  <div className="grid grid-cols-2 gap-x-6 gap-y-1.5">
                    <Metric label="Энергия"      value={entry.energyLevel} />
                    <Metric label="Стресс"        value={entry.stressLevel} colorInvert />
                    <Metric label="Мотивация"     value={entry.motivationLevel} />
                    <Metric label="Концентрация"  value={entry.concentrationLevel} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {!historyLoading && history.length === 0 && !latestLoading && !latest && (
          <p className="text-center text-sm text-muted-foreground py-8">
            Состояний пока нет — добавьте первое
          </p>
        )}
      </div>
    </div>
  )
}
