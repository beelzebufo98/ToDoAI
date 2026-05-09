import { useMemo } from 'react'
import { Info, X } from 'lucide-react'
import { cn } from '@/lib/utils'

interface InfoHintProps {
  title: string
  description: string
  onDismiss?: () => void
  className?: string
}

export function InfoHint({ title, description, onDismiss, className }: InfoHintProps) {
  const wrapperClassName = useMemo(
    () =>
      cn(
        'rounded-xl border border-sky-200/70 bg-sky-50/70 px-4 py-3 text-sm text-slate-700',
        className,
      ),
    [className],
  )

  return (
    <div className={wrapperClassName}>
      <div className="flex items-start gap-3">
        <Info className="mt-0.5 h-4 w-4 shrink-0 text-sky-600" />
        <div className="min-w-0 flex-1">
          <p className="font-medium text-slate-900">{title}</p>
          <p className="mt-1 leading-6 text-slate-700">{description}</p>
        </div>
        {onDismiss ? (
          <button
            type="button"
            onClick={onDismiss}
            className="rounded-md p-1 text-slate-500 transition-colors hover:bg-sky-100 hover:text-slate-900"
            aria-label="Скрыть подсказку"
          >
            <X className="h-4 w-4" />
          </button>
        ) : null}
      </div>
    </div>
  )
}
