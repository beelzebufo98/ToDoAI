import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Loader2, Clock } from 'lucide-react'
import { toast } from 'sonner'
import { tasksApi } from '@/api/tasks'
import { scheduleApi, type DaySchedule } from '@/api/schedule'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

// Build ISO 8601 with local timezone offset (e.g. "2026-04-19T09:00:00+03:00")
function toLocalISOString(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  const offsetMin = -d.getTimezoneOffset()
  const sign = offsetMin >= 0 ? '+' : '-'
  const absMin = Math.abs(offsetMin)
  const oh = pad(Math.floor(absMin / 60))
  const om = pad(absMin % 60)
  return (
    `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
    `T${pad(d.getHours())}:${pad(d.getMinutes())}:00${sign}${oh}:${om}`
  )
}

function buildLocalDateTime(scheduleDate: string, startTime: string): Date {
  const [year, month, day] = scheduleDate.split('-').map(Number)
  const [hours, minutes]   = startTime.split(':').map(Number)
  return new Date(year, month - 1, day, hours, minutes, 0, 0)
}

function defaultStartTime(): string {
  const d = new Date()
  const h = String(d.getHours()).padStart(2, '0')
  const min = d.getMinutes() < 30 ? '00' : '30'
  return `${h}:${min}`
}

interface Props {
  scheduleDate: string
  onGenerated: (schedule: DaySchedule) => void
}

export function GenerateDayCard({ scheduleDate, onGenerated }: Props) {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [startTime, setStartTime] = useState(defaultStartTime)

  const { data, isLoading } = useQuery({
    queryKey: ['tasks', 'all'],
    queryFn: () => tasksApi.getAll({ pageSize: 100 }),
  })

  const tasks = data?.data.payload.tasks ?? []

  const toggle = (id: string) =>
    setSelectedIds(prev => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })

  const selectAll = () => setSelectedIds(new Set(tasks.map(t => t.id)))
  const clearAll  = () => setSelectedIds(new Set())

  const mutation = useMutation({
    mutationFn: () => {
      const startAt = toLocalISOString(buildLocalDateTime(scheduleDate, startTime))
      return scheduleApi.generate({ scheduleDate, startAt, taskIds: [...selectedIds] })
    },
    onSuccess: res => onGenerated(res.data.payload),
    onError: () => toast.error('Не удалось сгенерировать расписание'),
  })

  return (
    <div className="space-y-5">
      <div>
        <h2 className="text-sm font-semibold text-foreground mb-0.5">Расписание на сегодня</h2>
        <p className="text-xs text-muted-foreground">Выберите задачи и время начала — план составится автоматически</p>
      </div>

      {/* Start time */}
      <div className="space-y-1.5">
        <Label className="text-sm font-medium flex items-center gap-1.5">
          <Clock className="h-3.5 w-3.5" />
          Начало дня
        </Label>
        <Input
          type="time"
          value={startTime}
          onChange={e => setStartTime(e.target.value)}
          className="h-10 bg-background/80 w-36"
        />
      </div>

      {/* Task list */}
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label className="text-sm font-medium">Задачи в план</Label>
          <div className="flex gap-2">
            <button onClick={selectAll} className="text-xs text-indigo-600 hover:text-indigo-700 font-medium">Все</button>
            <span className="text-xs text-muted-foreground">·</span>
            <button onClick={clearAll}  className="text-xs text-muted-foreground hover:text-foreground">Сбросить</button>
          </div>
        </div>

        {isLoading && (
          <div className="flex justify-center py-8">
            <div className="h-5 w-5 rounded-full border-2 border-indigo-600 border-t-transparent animate-spin" />
          </div>
        )}

        {!isLoading && tasks.length === 0 && (
          <p className="text-sm text-muted-foreground py-6 text-center">
            Нет активных задач — сначала создайте задачи
          </p>
        )}

        <div className="space-y-1.5">
          {tasks.map(task => (
            <button
              key={task.id}
              type="button"
              onClick={() => toggle(task.id)}
              className={cn(
                'w-full text-left rounded-xl border px-3 py-2.5 transition-colors',
                selectedIds.has(task.id)
                  ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-950/40'
                  : 'border-border bg-card hover:border-border/80'
              )}
            >
              <div className="flex items-start gap-2.5">
                <span className={cn(
                  'mt-0.5 h-4 w-4 shrink-0 rounded border flex items-center justify-center text-xs',
                  selectedIds.has(task.id)
                    ? 'border-indigo-500 bg-indigo-500 text-white'
                    : 'border-border'
                )}>
                  {selectedIds.has(task.id) && '✓'}
                </span>
                <div className="min-w-0">
                  <p className="text-sm font-medium text-foreground leading-snug">{task.title}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">{task.estimatedMinutes} мин</p>
                </div>
              </div>
            </button>
          ))}
        </div>
      </div>

      <Button
        onClick={() => mutation.mutate()}
        disabled={selectedIds.size === 0 || mutation.isPending || !startTime}
        className="w-full bg-indigo-600 text-white hover:bg-indigo-500"
      >
        {mutation.isPending && <Loader2 className="h-4 w-4 animate-spin" />}
        Сгенерировать план
        {selectedIds.size > 0 && ` (${selectedIds.size})`}
      </Button>
    </div>
  )
}
