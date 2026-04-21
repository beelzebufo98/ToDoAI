import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { X, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import { tasksApi } from '@/api/tasks'
import type { Task, Priority, ComplexityLevel } from '@/api/tasks'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

const PRIORITY_OPTIONS: { label: string; value: Priority }[] = [
  { label: 'Низкий',      value: 2  },
  { label: 'Средний',     value: 5  },
  { label: 'Высокий',     value: 8  },
  { label: 'Критический', value: 10 },
]

const COMPLEXITY_OPTIONS: { label: string; value: ComplexityLevel }[] = [
  { label: 'Простая',       value: 2  },
  { label: 'Средняя',       value: 5  },
  { label: 'Сложная',       value: 8  },
  { label: 'Очень сложная', value: 10 },
]

const schema = z.object({
  title:            z.string().min(1, 'Введите название'),
  description:      z.string().min(1, 'Введите описание'),
  estimatedMinutes: z.number().int().min(1, 'Минимум 1 минута').max(1440),
  priority:         z.number().min(1).max(10),
  complexityLevel:  z.number().min(1).max(10),
  deadlineAt:       z.string().min(1, 'Выберите дедлайн'),
})

type FormData = z.infer<typeof schema>

// datetime-local input expects "YYYY-MM-DDTHH:mm"
function toDatetimeLocal(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

interface Props {
  open: boolean
  onClose: () => void
  task: Task
}

export function EditTaskDialog({ open, onClose, task }: Props) {
  const queryClient = useQueryClient()

  const { register, handleSubmit, formState: { errors }, setValue, watch, reset } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title:            task.title,
      description:      task.description,
      estimatedMinutes: task.estimatedMinutes,
      priority:         task.priority,
      complexityLevel:  task.complexityLevel,
      deadlineAt:       toDatetimeLocal(task.deadlineAt),
    },
  })

  // sync if task prop changes while dialog is open
  useEffect(() => {
    reset({
      title:            task.title,
      description:      task.description,
      estimatedMinutes: task.estimatedMinutes,
      priority:         task.priority,
      complexityLevel:  task.complexityLevel,
      deadlineAt:       toDatetimeLocal(task.deadlineAt),
    })
  }, [task.id]) // eslint-disable-line react-hooks/exhaustive-deps

  const priority        = watch('priority')
  const complexityLevel = watch('complexityLevel')

  const mutation = useMutation({
    mutationFn: (data: FormData) =>
      tasksApi.update(task.id, {
        title:            data.title,
        description:      data.description,
        estimatedMinutes: data.estimatedMinutes,
        priority:         data.priority as Priority,
        complexityLevel:  data.complexityLevel as ComplexityLevel,
        deadlineAt:       new Date(data.deadlineAt).toISOString(),
      }),
    onSuccess: () => {
      toast.success('Задача обновлена')
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      onClose()
    },
    onError: () => toast.error('Не удалось обновить задачу'),
  })

  const handleClose = () => { reset(); onClose() }

  return (
    <AnimatePresence>
      {open && (
        <>
          <motion.div
            className="fixed inset-0 bg-black/40 z-40"
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          />
          <motion.div
            className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-lg bg-card border border-border/50 rounded-2xl shadow-xl p-6 max-h-[90vh] overflow-y-auto"
            initial={{ opacity: 0, scale: 0.96 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.96 }}
            transition={{ duration: 0.18 }}
          >
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-base font-semibold">Редактировать задачу</h2>
              <button onClick={handleClose} className="text-muted-foreground hover:text-foreground transition-colors">
                <X className="h-4 w-4" />
              </button>
            </div>

            <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-4">
              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Название</Label>
                <Input className="h-10 bg-background/80" {...register('title')} />
                {errors.title && <p className="text-xs text-destructive">{errors.title.message}</p>}
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Описание</Label>
                <textarea
                  className="w-full min-h-[72px] resize-none rounded-lg border border-input bg-background/80 px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-ring"
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
                  {errors.estimatedMinutes && <p className="text-xs text-destructive">{errors.estimatedMinutes.message}</p>}
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
                  {PRIORITY_OPTIONS.map(opt => (
                    <button
                      key={opt.value}
                      type="button"
                      onClick={() => setValue('priority', opt.value)}
                      className={cn(
                        'rounded-lg py-1.5 text-xs font-medium transition-colors border',
                        priority === opt.value
                          ? 'bg-foreground text-background border-foreground'
                          : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
                      )}
                    >
                      {opt.label}
                    </button>
                  ))}
                </div>
              </div>

              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Сложность</Label>
                <div className="grid grid-cols-4 gap-1.5">
                  {COMPLEXITY_OPTIONS.map(opt => (
                    <button
                      key={opt.value}
                      type="button"
                      onClick={() => setValue('complexityLevel', opt.value)}
                      className={cn(
                        'rounded-lg py-1.5 text-xs font-medium transition-colors border',
                        complexityLevel === opt.value
                          ? 'bg-foreground text-background border-foreground'
                          : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
                      )}
                    >
                      {opt.label}
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
                  disabled={mutation.isPending}
                  className="flex-1 bg-indigo-600 text-white hover:bg-indigo-500"
                >
                  {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
                  Сохранить
                </Button>
              </div>
            </form>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  )
}
