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
    queryFn: () => userStateApi.getHistory(7),
    retry: false,
  })

  const latest  = latestData?.data.payload
  const history = historyData?.data.payload.history ?? []

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
      toast.success('Состояние сохранено')
      queryClient.invalidateQueries({ queryKey: ['userState'] })
      reset()
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
          <h2 className="text-sm font-semibold mb-4">Добавить состояние</h2>

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
              Сохранить
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
              {history.map(s => (
                <StateCard key={s.id} state={s} />
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
