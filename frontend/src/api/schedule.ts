import { apiClient } from './client'

export interface ScheduleBlock {
  scheduleId: string
  taskId: string
  startAt: string
  endAt: string
  taskTitle?: string
  taskDescription?: string
}

export interface DaySchedule {
  dayScheduleId: string
  scheduleDate: string
  version: number
  isActiveVersion: boolean
  createDate: string
  blocks: ScheduleBlock[]
}

export interface GenerateScheduleRequest {
  scheduleDate: string       // "YYYY-MM-DD"
  startAt: string            // ISO DateTimeOffset, UTC date must match scheduleDate
  taskIds: string[]
}

export const scheduleApi = {
  generate: (data: GenerateScheduleRequest) =>
    apiClient.post<{ payload: DaySchedule }>('/schedule/generate', data),

  getDay: (scheduleDate: string) =>
    apiClient.get<{ payload: DaySchedule }>(`/schedule/${scheduleDate}/day`),
}
