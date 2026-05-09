import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { Sparkles, X, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { tasksApi } from '@/api/tasks'
import type { Priority, ComplexityLevel, TaskAssistResponse } from '@/api/tasks'
import { Button } from '@/components/ui/button'
import { InfoHint } from '@/components/ui/info-hint'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

const PRIORITY_OPTIONS: { label: string; value: Priority }[] = [
  { label: 'Низкий', value: 2 },
  { label: 'Средний', value: 5 },
  { label: 'Высокий', value: 8 },
  { label: 'Критический', value: 10 },
]

const COMPLEXITY_OPTIONS: { label: string; value: ComplexityLevel }[] = [
  { label: 'Простая', value: 2 },
  { label: 'Средняя', value: 5 },
  { label: 'Сложная', value: 8 },
  { label: 'Очень сложная', value: 10 },
]

const schema = z.object({
  title: z.string().min(1, 'Введите название'),
  description: z.string().min(1, 'Введите описание'),
  estimatedMinutes: z.number().int().min(1, 'Минимум 1 минута').max(1440),
  priority: z.number().min(1).max(10),
  complexityLevel: z.number().min(1).max(10),
  deadlineAt: z.string().min(1, 'Выберите дедлайн'),
})

type FormData = z.infer<typeof schema>

interface Props {
  open: boolean
  onClose: () => void
}

function getNearestOption(
  value: number,
  options: readonly { value: number }[],
): number {
  return options.reduce((closest, option) => {
    return Math.abs(option.value - value) < Math.abs(closest.value - value) ? option : closest
  }).value
}

export function CreateTaskDialog({ open, onClose }: Props) {
  const queryClient = useQueryClient()
  const [assistSuggestion, setAssistSuggestion] = useState<TaskAssistResponse | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    getValues,
    watch,
    reset,
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      estimatedMinutes: 60,
      priority: 5,
      complexityLevel: 5,
    },
  })

  const priority = watch('priority')
  const complexityLevel = watch('complexityLevel')

  const createMutation = useMutation({
    mutationFn: (data: FormData) =>
      tasksApi.create({
        title: data.title,
        description: data.description,
        estimatedMinutes: data.estimatedMinutes,
        priority: data.priority as Priority,
        complexityLevel: data.complexityLevel as ComplexityLevel,
        deadlineAt: new Date(data.deadlineAt).toISOString(),
      }),
    onSuccess: () => {
      toast.success('Задача создана')
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      setAssistSuggestion(null)
      reset()
      onClose()
    },
    onError: () => toast.error('Не удалось создать задачу'),
  })

  const assistMutation = useMutation({
    mutationFn: async () => {
      const values = getValues()
      const title = values.title.trim()
      const description = values.description.trim()
      const deadlineAt = values.deadlineAt

      if (title.length < 6) {
        throw new Error('Для AI-подсказки название должно быть не короче 6 символов')
      }

      if (description.length < 20) {
        throw new Error('Для AI-подсказки описание должно быть не короче 20 символов')
      }

      if (!deadlineAt) {
        throw new Error('Для AI-подсказки нужно указать дедлайн')
      }

      const response = await tasksApi.assist({
        title,
        description,
        deadlineAt: new Date(deadlineAt).toISOString(),
      })

      return response.data.payload
    },
    onSuccess: (suggestion) => {
      setValue('title', suggestion.suggestedTitle, { shouldDirty: true, shouldValidate: true })
      setValue('description', suggestion.suggestedDescription, { shouldDirty: true, shouldValidate: true })
      setValue('estimatedMinutes', suggestion.suggestedEstimatedMinutes, { shouldDirty: true, shouldValidate: true })
      setValue(
        'priority',
        getNearestOption(suggestion.suggestedPriority, PRIORITY_OPTIONS),
        { shouldDirty: true, shouldValidate: true },
      )
      setValue(
        'complexityLevel',
        getNearestOption(suggestion.suggestedComplexityLevel, COMPLEXITY_OPTIONS),
        { shouldDirty: true, shouldValidate: true },
      )
      setAssistSuggestion(suggestion)
      toast.success('AI уточнил формулировку и оценку задачи')
    },
    onError: (error) => {
      const message = error instanceof Error
        ? error.message
        : 'Не удалось получить AI-подсказку'
      toast.error(message)
    },
  })

  const handleClose = () => {
    setAssistSuggestion(null)
    reset()
    onClose()
  }

  return (
    <AnimatePresence>
      {open && (
        <>
          <motion.div
            className="fixed inset-0 z-40 bg-black/40"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          />
          <motion.div
            className="fixed left-1/2 top-1/2 z-50 max-h-[90vh] w-full max-w-xl -translate-x-1/2 -translate-y-1/2 overflow-y-auto rounded-2xl border border-border/50 bg-card p-6 shadow-xl"
            initial={{ opacity: 0, scale: 0.96 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.96 }}
            transition={{ duration: 0.18 }}
          >
            <div className="mb-5 flex items-start justify-between gap-4">
              <div className="space-y-1">
                <h2 className="text-base font-semibold">Новая задача</h2>
                <p className="text-sm text-muted-foreground">
                  Можно заполнить поля вручную или попросить AI уточнить формулировку, оценку и приоритет.
                </p>
              </div>
              <button
                onClick={handleClose}
                className="text-muted-foreground transition-colors hover:text-foreground"
              >
                <X className="h-4 w-4" />
              </button>
            </div>

            <form onSubmit={handleSubmit((data) => createMutation.mutate(data))} className="space-y-4">
              <div className="rounded-xl border border-border/60 bg-background/50 p-3">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 text-sm font-medium">
                      <Sparkles className="h-4 w-4 text-indigo-500" />
                      AI-подсказка
                    </div>
                    <p className="text-xs leading-5 text-muted-foreground">
                      Работает лучше, если уже заполнены название, описание и дедлайн.
                    </p>
                  </div>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => assistMutation.mutate()}
                    disabled={assistMutation.isPending}
                    className="min-w-40"
                  >
                    {assistMutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
                    Уточнить с AI
                  </Button>
                </div>

                <InfoHint
                  title="Когда AI особенно полезен"
                  description="Лучше всего работает для задач с понятным дедлайном и осмысленным описанием. Используйте подсказку, когда нужно уточнить формулировку, оценку времени или приоритет."
                  className="mt-3 border-sky-200/60 bg-sky-50/50"
                />

                {assistSuggestion && (
                  <div className="mt-3 rounded-lg border border-indigo-200/70 bg-indigo-50/60 px-3 py-2">
                    <p className="text-sm font-medium text-slate-900">Почему AI предложил такие значения</p>
                    <p className="mt-1 text-sm leading-6 text-slate-700">{assistSuggestion.reasoning}</p>
                  </div>
                )}
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Название</Label>
                <Input className="h-10 bg-background/80" {...register('title')} />
                {errors.title && <p className="text-xs text-destructive">{errors.title.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Описание</Label>
                <textarea
                  className="min-h-[88px] w-full resize-none rounded-lg border border-input bg-background/80 px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-ring"
                  {...register('description')}
                />
                {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1.5">
                  <Label className="text-sm font-medium">Оценка (мин.)</Label>
                  <Input
                    type="number"
                    min={1}
                    className="h-10 bg-background/80"
                    {...register('estimatedMinutes', { valueAsNumber: true })}
                  />
                  {errors.estimatedMinutes && (
                    <p className="text-xs text-destructive">{errors.estimatedMinutes.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <Label className="text-sm font-medium">Дедлайн</Label>
                  <Input type="datetime-local" className="h-10 bg-background/80" {...register('deadlineAt')} />
                  {errors.deadlineAt && <p className="text-xs text-destructive">{errors.deadlineAt.message}</p>}
                </div>
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Приоритет</Label>
                <div className="grid grid-cols-4 gap-1.5">
                  {PRIORITY_OPTIONS.map((option) => (
                    <button
                      key={option.value}
                      type="button"
                      onClick={() => setValue('priority', option.value)}
                      className={cn(
                        'rounded-lg border py-1.5 text-xs font-medium transition-colors',
                        priority === option.value
                          ? 'border-foreground bg-foreground text-background'
                          : 'border-border bg-background text-muted-foreground hover:border-foreground/40',
                      )}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Сложность</Label>
                <div className="grid grid-cols-4 gap-1.5">
                  {COMPLEXITY_OPTIONS.map((option) => (
                    <button
                      key={option.value}
                      type="button"
                      onClick={() => setValue('complexityLevel', option.value)}
                      className={cn(
                        'rounded-lg border py-1.5 text-xs font-medium transition-colors',
                        complexityLevel === option.value
                          ? 'border-foreground bg-foreground text-background'
                          : 'border-border bg-background text-muted-foreground hover:border-foreground/40',
                      )}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              </div>

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose}>
                  Отмена
                </Button>
                <Button
                  type="submit"
                  disabled={createMutation.isPending}
                  className="flex-1 bg-indigo-600 text-white hover:bg-indigo-500"
                >
                  {createMutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
                  Создать
                </Button>
              </div>
            </form>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  )
}
