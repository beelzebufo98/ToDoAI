import { RefreshCw } from 'lucide-react'
import type { DaySchedule } from '@/api/schedule'

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

function blockDuration(startAt: string, endAt: string) {
  const mins = Math.round((new Date(endAt).getTime() - new Date(startAt).getTime()) / 60000)
  return `${mins} мин`
}

interface Props {
  schedule: DaySchedule
  onRegenerate: () => void
}

export function DayScheduleTimeline({ schedule, onRegenerate }: Props) {
  const blocks = schedule.blocks

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-sm font-semibold text-foreground">Расписание на сегодня</h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            {blocks.length} {blocks.length === 1 ? 'задача' : blocks.length < 5 ? 'задачи' : 'задач'} · версия {schedule.version}
          </p>
        </div>
        <button
          onClick={onRegenerate}
          className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground transition-colors"
        >
          <RefreshCw className="h-3.5 w-3.5" />
          Перегенерировать
        </button>
      </div>

      <div className="relative space-y-0">
        {blocks.map((block, idx) => (
          <div key={block.scheduleId} className="flex gap-3">
            {/* Timeline spine */}
            <div className="flex flex-col items-center">
              <div className="h-2.5 w-2.5 rounded-full bg-indigo-500 mt-3 shrink-0" />
              {idx < blocks.length - 1 && (
                <div className="w-px flex-1 bg-border my-1" />
              )}
            </div>

            {/* Block card */}
            <div className="flex-1 pb-3">
              <div className="bg-card border border-border/50 rounded-xl p-3">
                <div className="flex items-baseline justify-between gap-2 mb-1">
                  <span className="text-xs font-mono text-indigo-600 dark:text-indigo-400 shrink-0">
                    {formatTime(block.startAt)} – {formatTime(block.endAt)}
                  </span>
                  <span className="text-xs text-muted-foreground shrink-0">{blockDuration(block.startAt, block.endAt)}</span>
                </div>
                <p className="text-sm font-medium text-foreground leading-snug">{block.taskTitle}</p>
                {block.taskDescription && (
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">{block.taskDescription}</p>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
