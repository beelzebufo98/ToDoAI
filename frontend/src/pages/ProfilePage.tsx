import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ChevronUp, Loader2, Mail, PencilLine, UserRound } from 'lucide-react'
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
              'flex-1 rounded-lg border py-1.5 text-xs font-medium transition-colors',
              value === level.value
                ? 'border-foreground bg-foreground text-background'
                : 'border-border bg-background text-muted-foreground hover:border-foreground/40'
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
  const [isStateFormOpen, setIsStateFormOpen] = useState(true)

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
      setIsStateFormOpen(false)
      return
    }

    reset(defaultFormValues)
    setIsStateFormOpen(true)
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
      if (isUpdate) {
        setIsStateFormOpen(false)
      }
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

  const profileTitle =
    [user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.userName || 'Профиль'

  return (
    <div className="mx-auto w-full max-w-4xl p-6">
      <div className="mb-6">
        <h1 className="text-xl font-semibold text-foreground">Профиль</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">
          Общая информация, робот-напарник и состояние на сегодня
        </p>
      </div>

      <div className="space-y-6">
        <section className="rounded-xl border border-border/50 bg-card p-5">
          <div className="flex flex-col gap-6">
            <div className="flex items-center gap-4">
              <UserAvatar seed={avatarSeed} variant={avatarVariant} size={84} className="shrink-0" />
              <div>
                <p className="text-lg font-semibold text-foreground">{userLoading ? 'Загрузка…' : profileTitle}</p>
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
                <p className="mt-1 text-xs text-muted-foreground">
                  Выберите визуальный образ помощника ToDoAI. Выбор сохранится для этого аккаунта на текущем
                  устройстве.
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
                <div className="mt-3 grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-6">
                  {avatarVariants.map((variant) => (
                    <button
                      key={variant}
                      type="button"
                      onClick={() => handleAvatarChange(variant)}
                      className={cn(
                        'rounded-xl border p-3 text-left transition-colors',
                        avatarVariant === variant
                          ? 'border-primary bg-primary/5'
                          : 'border-border/60 hover:border-foreground/30'
                      )}
                    >
                      <UserAvatar seed={avatarSeed} variant={variant} size={56} className="mx-auto" />
                      <p className="mt-2 text-center text-xs font-medium text-foreground">
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
            <p className="mb-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">Последнее состояние</p>
            <StateCard state={latest} compact />
          </section>
        )}

        <section className="rounded-xl border border-border/50 bg-card p-5">
          <div className="mb-4 flex items-start justify-between gap-4">
            <div>
              <h2 className="text-sm font-semibold text-foreground">
                {isUpdate ? 'Обновление состояния' : 'Состояние на сегодня'}
              </h2>
              <p className="mt-1 text-xs text-muted-foreground">
                {isUpdate
                  ? 'Текущее состояние уже сохранено. Раскройте форму, если хотите скорректировать данные.'
                  : 'Заполните форму, чтобы сохранить текущее состояние на сегодня.'}
              </p>
            </div>

            {isUpdate ? (
              <Button
                type="button"
                variant="outline"
                className="h-8 shrink-0"
                onClick={() => setIsStateFormOpen((value) => !value)}
              >
                {isStateFormOpen ? (
                  <>
                    <ChevronUp className="h-4 w-4" />
                    Скрыть
                  </>
                ) : (
                  <>
                    <PencilLine className="h-4 w-4" />
                    Изменить
                  </>
                )}
              </Button>
            ) : null}
          </div>

          {!isStateFormOpen && isUpdate ? (
            <div className="rounded-xl border border-border/50 bg-muted/20 px-4 py-3">
              <p className="text-sm text-muted-foreground">
                Данные уже заполнены. Форма скрыта, чтобы не отвлекать. При необходимости вы можете раскрыть ее и
                обновить запись.
              </p>
            </div>
          ) : (
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
                {errors.sleepHours ? <p className="text-xs text-destructive">{errors.sleepHours.message}</p> : null}
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
                className="mt-2 w-full bg-indigo-600 text-white hover:bg-indigo-500"
              >
                {mutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
                {isUpdate ? 'Обновить' : 'Сохранить'}
              </Button>
            </form>
          )}
        </section>
      </div>
    </div>
  )
}
