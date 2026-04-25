import { apiClient } from './client'

// Числа 1-10 как на бэкенде
export type Priority = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10
export type ComplexityLevel = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10

// Строковые значения enum как на бэкенде (lowercase)
export type WorkStatus = 'new' | 'todo' | 'running' | 'completed' | 'deleted'
export type SortBy = 'createdAt' | 'priority' | 'complexityLevel'
export type SortType = 'asc' | 'desc'

export interface Task {
  id: string
  title: string
  description: string
  estimatedMinutes: number
  actualSpentMinutes: number
  remainingMinutes: number
  complexityLevel: ComplexityLevel
  priority: Priority
  workStatus: WorkStatus
  deadlineAt: string
  createdAt: string
  updatedAt: string
  actualStartDate?: string
  actualEndDate?: string
}

export interface CreateTaskRequest {
  title: string
  description: string        // обязательный на бэкенде
  estimatedMinutes: number
  complexityLevel: ComplexityLevel
  priority: Priority
  deadlineAt: string
}

export interface UpdateTaskRequest {
  title?: string
  description?: string
  estimatedMinutes?: number
  complexityLevel?: ComplexityLevel
  priority?: Priority
  deadlineAt?: string
}

export interface TaskFilters {
  workStatus?: Exclude<WorkStatus, 'deleted'>
  sortBy?: SortBy
  sortType?: SortType
  page?: number
  pageSize?: number
}

export interface GetTasksResponse {
  tasks: Task[]   // поле "tasks", не "items"
}

export const tasksApi = {
  create: (data: CreateTaskRequest) =>
    apiClient.post<{ payload: { taskId: string } }>('/task/create', data),

  getAll: (filters?: TaskFilters) =>
    apiClient.get<{ payload: GetTasksResponse }>('/task/get', { params: filters }),

  getById: (taskId: string) =>
    apiClient.get<{ payload: Task }>(`/task/${taskId}/get`),

  update: (taskId: string, data: UpdateTaskRequest) =>
    apiClient.patch<{ payload: Task }>(`/task/${taskId}/update`, data),

  updateStatus: (taskId: string, workStatus: Exclude<WorkStatus, 'deleted'>) =>
    apiClient.patch<{ payload: Task }>(`/task/${taskId}/status`, { workStatus }),

  delete: (taskId: string) =>
    apiClient.delete(`/task/${taskId}/decline`),   // soft-delete маршрут
}
