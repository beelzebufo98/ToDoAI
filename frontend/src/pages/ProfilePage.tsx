import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Mail, UserRound } from 'lucide-react'
import { z } from 'zod'
import { toast } from 'sonner'
import { userApi } from '@/api/user'
import { userStateApi } from '@/api/userState'
import { UserAvatar } from '@/components/profile/UserAvatar'
import { StateCard } from '@/components/user-state/StateCard'
import { shouldUpdateLatestState } from '@/components/user-state/state-utils'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  avatarVariantLabels,
  avatarVariants,
  getAvatarVariant,
  setAvatarVariant,
  type AvatarVariant,
} from '@/lib/avatar-preferences'
import { cn } from '@/lib/utils'

const LEVELS = [
  { label: '1–2', value: 2 },
  { label: '3–4', value: 4 },
  { label: '5–6', value: 6 },
  { label: '7–8', value: 8 },
  { label: '9–10', value: 10 },
]

const defaultFormValues = {
  sleepHours: 8,
  energyLevel: 6,
  stressLevel: 4,
  motivationLevel: 6,
  concentrationLevel: 6,
}

const schema = z.object({
  sleepHours: z.number().min(0, 'Минимум 0').max(24, 'Максимум 24'),
  energyLevel: z.number().min(1).max(10),
  stressLevel: z.number().min(1).max(10),
  motivationLevel: z.number().min(1).max(10),
  concentrationLevel: z.number().min(1).max(10),
})

type FormData = z.infer<typeof schema>

function LevelSelector({
  label,
  value,
  onChange,
}: {
  label: string
  value: number
  onChange: (value: number) => void
}) {
  return (
    <div className="space-y-1.5">
      <Label className="text-sm font-medium">
        {label}
        <span className="ml-1.5 font-normal text-muted-foreground">({value}/10)</span>
      </Label>
      <div className="flex gap-1.5">
        {LEVELS.map((level) => (
          <button
            key={level.value}
            type="button"
            onClick={() => onChange(level.value)}
            className={cn(
              'flex-1 rounded-lg py-1.5 text-xs font-medium transition-colors border',
              value === level.value
                ? 'bg-foreground text-background border-foreground'
                : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
            )}
          >
            {level.label}
          </button>
        ))}
      </div>
    </div>
  )
}

export function ProfilePage() {
  const queryClient = useQueryClient()
  const [avatarVariant, setSelectedAvatarVariant] = useState<AvatarVariant>('bot-1')
  const [isAvatarPickerOpen, setIsAvatarPickerOpen] = useState(true)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: defaultFormValues,
  })

  const energyLevel = watch('energyLevel')
  const stressLevel = watch('stressLevel')
  const motivationLevel = watch('motivationLevel')
  const concentrationLevel = watch('concentrationLevel')

  const { data: userData, isLoading: userLoading } = useQuery({
    queryKey: ['user', 'me'],
    queryFn: () => userApi.getMe(),
    retry: false,
  })

  const { data: latestData, isLoading: latestLoading } = useQuery({
    queryKey: ['userState', 'latest'],
    queryFn: () => userStateApi.getLatest(),
    retry: false,
  })

  const user = userData?.data.payload
  const latest = latestData?.data.payload
  const isUpdate = !!latest && shouldUpdateLatestState(latest.createdAt)

  const avatarSeed = useMemo(() => {
    if (!user) {
      return 'todoai-user'
    }

    return [user.userId, user.userName, user.firstName, user.lastName].filter(Boolean).join(':')
  }, [user])

  useEffect(() => {
    if (!user) {
      return
    }

    const savedVariant = getAvatarVariant(user.userId)
    setSelectedAvatarVariant(savedVariant)
    setIsAvatarPickerOpen(savedVariant === 'bot-1')
  }, [user])

  useEffect(() => {
    if (latest && shouldUpdateLatestState(latest.createdAt)) {
      reset({
        sleepHours: latest.sleepMinutes / 60,
        energyLevel: latest.energyLevel,
        stressLevel: latest.stressLevel,
        motivationLevel: latest.motivationLevel,
        concentrationLevel: latest.concentrationLevel,
      })
      return
    }

    reset(defaultFormValues)
  }, [latest?.id, latest, reset])

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      userStateApi.create({
        sleepMinutes: Math.round(data.sleepHours * 60),
        energyLevel: data.energyLevel,
        stressLevel: data.stressLevel,
        motivationLevel: data.motivationLevel,
        concentrationLevel: data.concentrationLevel,
      }),
    onSuccess: () => {
      toast.success(isUpdate ? 'Состояние обновлено' : 'Состояние сохранено')
      queryClient.invalidateQueries({ queryKey: ['userState'] })
    },
    onError: () => {
      toast.error('Не удалось сохранить состояние')
    },
  })

  const handleAvatarChange = (variant: AvatarVariant) => {
    if (!user) {
      return
    }

    setSelectedAvatarVariant(variant)
    setAvatarVariant(user.userId, variant)
    setIsAvatarPickerOpen(false)
    toast.success(`Выбран робот ${avatarVariantLabels[variant].toLowerCase()}`)
  }

  return (
    <div className="p-6 max-w-4xl mx-auto w-full">
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-foreground">Профиль</h1>
        <p className="text-sm text-muted-foreground mt-0.5">
          Общая информация, аватар и состояние на сегодня
        </p>
      </div>

      <div className="space-y-6">
        <section className="bg-card border border-border/50 rounded-xl p-5">
          <div className="flex flex-col gap-6">
            <div className="flex items-center gap-4">
              <UserAvatar seed={avatarSeed} variant={avatarVariant} size={84} className="shrink-0" />
              <div>
                <p className="text-lg font-semibold text-foreground">
                  {userLoading ? 'Загрузка…' : [user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.userName}
                </p>
                <div className="mt-2 space-y-1.5 text-sm text-muted-foreground">
                  <div className="flex items-center gap-2">
                    <UserRound className="h-4 w-4" />
                    <span>@{user?.userName ?? '—'}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Mail className="h-4 w-4" />
                    <span>{user?.email ?? 'Email не указан'}</span>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <div className="mb-3">
                <p className="text-sm font-medium text-foreground">Робот-напарник</p>
                <p className="text-xs text-muted-foreground mt-1">
                  Выберите визуальный образ помощника ToDoAI. Выбор сохранится для этого аккаунта на текущем устройстве.
                </p>
              </div>
              <div className="flex items-center justify-between rounded-xl border border-border/50 bg-muted/20 px-3 py-2">
                <div className="flex items-center gap-3">
                  <UserAvatar seed={avatarSeed} variant={avatarVariant} size={40} />
                  <div>
                    <p className="text-sm font-medium text-foreground">{avatarVariantLabels[avatarVariant]}</p>
                    <p className="text-xs text-muted-foreground">Активный напарник профиля</p>
                  </div>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  className="h-8"
                  onClick={() => setIsAvatarPickerOpen((value) => !value)}
                >
                  {isAvatarPickerOpen ? 'Скрыть' : 'Сменить'}
                </Button>
              </div>

              {isAvatarPickerOpen && (
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-6 mt-3">
                  {avatarVariants.map((variant) => (
                    <button
                      key={variant}
                      type="button"
                      onClick={() => handleAvatarChange(variant)}
                      className={cn(
                        'rounded-xl border p-3 transition-colors text-left',
                        avatarVariant === variant
                          ? 'border-primary bg-primary/5'
                          : 'border-border/60 hover:border-foreground/30'
                      )}
                    >
                      <UserAvatar seed={avatarSeed} variant={variant} size={56} className="mx-auto" />
                      <p className="mt-2 text-xs font-medium text-foreground text-center">
                        {avatarVariantLabels[variant]}
                      </p>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>
        </section>

        {!latestLoading && latest && (
          <section>
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide mb-2">
              Последнее состояние
            </p>
            <StateCard state={latest} compact />
          </section>
        )}

        <section className="bg-card border border-border/50 rounded-xl p-5">
          <h2 className="text-sm font-semibold mb-4">{isUpdate ? 'Обновить состояние' : 'Добавить состояние'}</h2>

          <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className="space-y-4">
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
              {errors.sleepHours && <p className="text-xs text-destructive">{errors.sleepHours.message}</p>}
            </div>

            <LevelSelector label="Энергия" value={energyLevel} onChange={(value) => setValue('energyLevel', value)} />
            <LevelSelector label="Стресс" value={stressLevel} onChange={(value) => setValue('stressLevel', value)} />
            <LevelSelector
              label="Мотивация"
              value={motivationLevel}
              onChange={(value) => setValue('motivationLevel', value)}
            />
            <LevelSelector
              label="Концентрация"
              value={concentrationLevel}
              onChange={(value) => setValue('concentrationLevel', value)}
            />

            <Button
              type="submit"
              disabled={mutation.isPending}
              className="w-full bg-indigo-600 text-white hover:bg-indigo-500 mt-2"
            >
              {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
              {isUpdate ? 'Обновить' : 'Сохранить'}
            </Button>
          </form>
        </section>
      </div>
    </div>
  )
}
