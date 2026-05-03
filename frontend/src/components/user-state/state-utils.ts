import type { UserState } from '@/api/userState'

export function isWithin3Hours(iso: string) {
  return Date.now() - new Date(iso).getTime() < 3 * 60 * 60 * 1000
}

export function isSameLocalDate(iso: string) {
  const createdAt = new Date(iso)
  const now = new Date()

  return (
    createdAt.getFullYear() === now.getFullYear() &&
    createdAt.getMonth() === now.getMonth() &&
    createdAt.getDate() === now.getDate()
  )
}

export function shouldUpdateLatestState(iso: string) {
  return isSameLocalDate(iso) && isWithin3Hours(iso)
}

export function formatStateDate(iso: string) {
  const date = new Date(iso)
  const hasTime = iso.includes('T')

  return date.toLocaleDateString('ru-RU', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    ...(hasTime
      ? {
          hour: '2-digit' as const,
          minute: '2-digit' as const,
        }
      : {}),
  })
}

export function formatSleepLabel(sleepMinutes: number) {
  const hours = Math.floor(sleepMinutes / 60)
  const minutes = sleepMinutes % 60

  return minutes > 0 ? `${hours} ч ${minutes} мин` : `${hours} ч`
}

export interface DailyStateSummary {
  day: string
  date: string
  sleepMinutes: number
  energyLevel: number
  stressLevel: number
  motivationLevel: number
  concentrationLevel: number
  count: number
}

export function groupStatesByDay(states: UserState[], options?: { excludeToday?: boolean }) {
  const today = new Date().toDateString()
  const excludeToday = options?.excludeToday ?? false

  return Object.entries(
    states.reduce<Record<string, UserState[]>>((acc, state) => {
      const day = new Date(state.createdAt).toDateString()
      if (!excludeToday || day !== today) {
        ;(acc[day] ??= []).push(state)
      }
      return acc
    }, {})
  )
    .map<DailyStateSummary>(([day, entries]) => {
      const avg = (selector: (state: UserState) => number) =>
        Math.round(entries.reduce((sum, state) => sum + selector(state), 0) / entries.length)

      return {
        day,
        date: entries[0].createdAt,
        sleepMinutes: avg((state) => state.sleepMinutes),
        energyLevel: avg((state) => state.energyLevel),
        stressLevel: avg((state) => state.stressLevel),
        motivationLevel: avg((state) => state.motivationLevel),
        concentrationLevel: avg((state) => state.concentrationLevel),
        count: entries.length,
      }
    })
    .sort((left, right) => right.date.localeCompare(left.date))
}
