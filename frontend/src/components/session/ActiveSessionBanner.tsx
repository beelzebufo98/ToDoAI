import { useEffect, useState } from 'react'
import { Loader2, Square, X } from 'lucide-react'
import { useTaskSession } from '@/contexts/TaskSessionContext'

const CANCEL_TIMEOUT_SECONDS = 120

function formatElapsed(seconds: number) {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60

  if (h > 0) {
    return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  }

  return `${m}:${String(s).padStart(2, '0')}`
}

export function ActiveSessionBanner() {
  const { current, syncing, stopSession, cancelSession } = useTaskSession()
  const [elapsed, setElapsed] = useState(0)
  const canCancel = elapsed < CANCEL_TIMEOUT_SECONDS

  useEffect(() => {
    if (!current) {
      setElapsed(0)
      return
    }

    const tick = () => {
      setElapsed(Math.floor((Date.now() - new Date(current.startedAt).getTime()) / 1000))
    }

    tick()
    const id = setInterval(tick, 1000)
    return () => clearInterval(id)
  }, [current?.sessionId])

  if (!current) {
    return null
  }

  return (
    <div className="mx-3 mb-2 space-y-2 rounded-lg border border-amber-200 bg-amber-50 p-3 dark:border-amber-800 dark:bg-amber-950/40">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1.5">
          <span className="h-2 w-2 shrink-0 animate-pulse rounded-full bg-amber-500" />
          <span className="text-xs font-medium text-amber-800 dark:text-amber-300">В работе</span>
        </div>
        <span className="tabular-nums text-sm font-semibold font-mono text-amber-800 dark:text-amber-300">
          {formatElapsed(elapsed)}
        </span>
      </div>

      <p className="line-clamp-1 text-xs font-medium text-amber-700 dark:text-amber-400">
        {current.title ?? 'Задача без названия'}
      </p>

      <div className="flex gap-1.5">
        <button
          onClick={stopSession}
          disabled={syncing}
          className="flex flex-1 items-center justify-center gap-1 rounded-md bg-amber-600 py-1 text-xs font-medium text-white transition-colors hover:bg-amber-700 disabled:opacity-50"
        >
          {syncing ? <Loader2 className="h-3 w-3 animate-spin" /> : <Square className="h-3 w-3 fill-current" />}
          Стоп
        </button>

        {canCancel && (
          <button
            onClick={cancelSession}
            disabled={syncing}
            className="flex items-center justify-center gap-1 rounded-md border border-amber-300 px-2 py-1 text-xs font-medium text-amber-700 transition-colors hover:bg-amber-100 disabled:opacity-50"
          >
            <X className="h-3 w-3" />
            Отмена
          </button>
        )}
      </div>
    </div>
  )
}
