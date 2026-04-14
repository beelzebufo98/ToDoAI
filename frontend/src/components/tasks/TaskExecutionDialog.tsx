import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useMutation } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { X, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import axios from 'axios'
import { taskExecutionApi } from '@/api/taskExecution'
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
  actualMinutes: z.number().int().min(1, 'Минимум 1 минута'),
  stressAfter:   z.number().min(1).max(10),
  energyAfter:   z.number().min(1).max(10),
})

type FormData = z.infer<typeof schema>

interface Props {
  open: boolean
  onClose: () => void
  onDone?: () => void   // вызывается когда фидбек сохранён или уже существует
  taskId: string
  taskTitle: string
}

export function TaskExecutionDialog({ open, onClose, onDone, taskId, taskTitle }: Props) {
  const { register, handleSubmit, formState: { errors }, setValue, watch, reset } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { stressAfter: 6, energyAfter: 6 },
  })

  const stressAfter = watch('stressAfter')
  const energyAfter = watch('energyAfter')

  const mutation = useMutation({
    mutationFn: (data: FormData) => taskExecutionApi.create(taskId, data),
    onSuccess: () => {
      toast.success('Фидбек сохранён')
      reset()
      onDone?.()
      onClose()
    },
    onError: (err) => {
      if (axios.isAxiosError(err) && err.response?.data?.error?.code === 7) {
        toast.error('Фидбек по этой задаче уже добавлен')
        onDone?.()
      } else {
        toast.error('Не удалось сохранить фидбек')
      }
      onClose()
    },
  })

  const handleClose = () => { reset(); onClose() }

  return (
    <AnimatePresence>
      {open && (
        <>
          <motion.div
            className="fixed inset-0 bg-black/40 z-40"
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={handleClose}
          />
          <motion.div
            className="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-full max-w-md bg-card border border-border/50 rounded-2xl shadow-xl p-6"
            initial={{ opacity: 0, scale: 0.96 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.96 }}
            transition={{ duration: 0.18 }}
          >
            <div className="flex items-center justify-between mb-1">
              <h2 className="text-base font-semibold">Фидбек по задаче</h2>
              <button onClick={handleClose} className="text-muted-foreground hover:text-foreground transition-colors">
                <X className="h-4 w-4" />
              </button>
            </div>
            <p className="text-sm text-muted-foreground mb-5 truncate">{taskTitle}</p>

            <form onSubmit={handleSubmit(d => mutation.mutate(d))} className="space-y-4">
              {/* Actual time */}
              <div className="space-y-1.5">
                <Label className="text-sm font-medium">Фактическое время (мин.)</Label>
                <Input type="number" min={1} className="h-10 bg-background/80" {...register('actualMinutes', { valueAsNumber: true })} />
                {errors.actualMinutes && <p className="text-xs text-destructive">{errors.actualMinutes.message}</p>}
              </div>

              {/* Stress level */}
              <div className="space-y-1.5">
                <Label className="text-sm font-medium">
                  Стресс после задачи
                  <span className="ml-1.5 font-normal text-muted-foreground">({stressAfter}/10)</span>
                </Label>
                <div className="flex gap-1.5">
                  {LEVELS.map(l => (
                    <button
                      key={l.value}
                      type="button"
                      onClick={() => setValue('stressAfter', l.value)}
                      className={cn(
                        'flex-1 rounded-lg py-1.5 text-xs font-medium transition-colors border',
                        stressAfter === l.value
                          ? 'bg-foreground text-background border-foreground'
                          : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
                      )}
                    >
                      {l.label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Energy level */}
              <div className="space-y-1.5">
                <Label className="text-sm font-medium">
                  Энергия после задачи
                  <span className="ml-1.5 font-normal text-muted-foreground">({energyAfter}/10)</span>
                </Label>
                <div className="flex gap-1.5">
                  {LEVELS.map(l => (
                    <button
                      key={l.value}
                      type="button"
                      onClick={() => setValue('energyAfter', l.value)}
                      className={cn(
                        'flex-1 rounded-lg py-1.5 text-xs font-medium transition-colors border',
                        energyAfter === l.value
                          ? 'bg-foreground text-background border-foreground'
                          : 'bg-background border-border text-muted-foreground hover:border-foreground/40'
                      )}
                    >
                      {l.label}
                    </button>
                  ))}
                </div>
              </div>

              <div className="flex gap-2 pt-1">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose}>
                  Пропустить
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
