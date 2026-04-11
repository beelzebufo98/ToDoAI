import { apiClient } from './client'

export type Priority = 'Low' | 'Medium' | 'High' | 'Critical'
export type ComplexityLevel = 'Easy' | 'Medium' | 'Hard' | 'VeryHard'
export type WorkStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled'

export interface CreateTaskRequest {
  title: string
  description?: string
  estimatedMinutes: number
  priority: Priority
  complexityLevel: ComplexityLevel
  deadlineAt?: string
}

export interface UpdateTaskRequest {
  title?: string
  description?: string
  estimatedMinutes?: number
  priority?: Priority
  complexityLevel?: ComplexityLevel
  deadlineAt?: string
}

export interface TaskFilters {
  workStatus?: WorkStatus
  sortType?: 'Asc' | 'Desc'
  sortBy?: string
  page?: number
  pageSize?: number
}

export interface Task {
  id: string
  title: string
  description?: string
  estimatedMinutes: number
  priority: Priority
  complexityLevel: ComplexityLevel
  workStatus: WorkStatus
  deadlineAt?: string
  createdAt: string
}

export interface GetTasksResponse {
  items: Task[]
  totalCount: number
  page: number
  pageSize: number
}

export const tasksApi = {
  create: (data: CreateTaskRequest) =>
    apiClient.post<Task>('/task/create', data),

  getAll: (filters?: TaskFilters) =>
    apiClient.get<GetTasksResponse>('/task/get', { params: filters }),

  getById: (taskId: string) =>
    apiClient.get<Task>(`/task/${taskId}/get`),

  update: (taskId: string, data: UpdateTaskRequest) =>
    apiClient.patch<Task>(`/task/${taskId}`, data),

  updateStatus: (taskId: string, workStatus: WorkStatus) =>
    apiClient.patch(`/task/${taskId}/status`, { workStatus }),

  delete: (taskId: string) =>
    apiClient.delete(`/task/${taskId}`),
}
