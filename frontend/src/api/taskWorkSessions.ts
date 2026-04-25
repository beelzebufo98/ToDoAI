import { apiClient } from './client'

export type TaskWorkSessionStatus = 'open' | 'completed' | 'cancelled'
// backend serializes via JsonNamingPolicy.SnakeCaseLower
export type TaskWorkSessionSource = 'timer' | 'manual_interval' | 'manual_duration' | 'schedule_block'

export interface TaskWorkSession {
  sessionId: string
  userId: string
  taskId: string
  scheduleBlockId: string | null
  startedAt: string
  endedAt: string | null
  spentMinutes: number | null
  status: TaskWorkSessionStatus
  source: TaskWorkSessionSource
  createdAt: string
  title: string | null
  taskEstimatedMinutes: number | null
  taskActualSpentMinutes: number | null
}

export const taskWorkSessionsApi = {
  start: (taskId: string, scheduleId?: string) =>
    apiClient.post<{ payload: TaskWorkSession }>('/task-work-sessions/start', { taskId, scheduleId }),

  stop: (sessionId: string) =>
    apiClient.post<{ payload: TaskWorkSession }>(`/task-work-sessions/${sessionId}/stop`),

  cancel: (sessionId: string) =>
    apiClient.post<{ payload: TaskWorkSession }>(`/task-work-sessions/${sessionId}/cancel`),

  getCurrent: () =>
    apiClient.get<{ payload: TaskWorkSession | null }>('/task-work-sessions/current'),

  getByDate: (date: string) =>
    apiClient.get<{ payload: { taskWorkSessionResponses: TaskWorkSession[] } }>(`/task-work-sessions?date=${date}`),
}
