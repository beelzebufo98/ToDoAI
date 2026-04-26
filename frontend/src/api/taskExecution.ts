import { apiClient } from './client'

export interface TaskExecution {
  taskExecutionId: string
  taskId: string
  scheduleId?: string
  actualMinutes: number
  stressAfter: number
  energyAfter: number
  createdAt: string
  motivationMessage: string
}

export interface CreateTaskExecutionRequest {
  scheduleId?: string
  actualMinutes: number  // > 0
  stressAfter: number    // 1-10
  energyAfter: number    // 1-10
}

export const taskExecutionApi = {
  // Задача ДОЛЖНА быть в статусе completed
  create: (taskId: string, data: CreateTaskExecutionRequest) =>
    apiClient.post<{ payload: TaskExecution }>(`/task-execution/${taskId}/create`, data),
}
