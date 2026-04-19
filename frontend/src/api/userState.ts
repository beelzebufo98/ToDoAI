import { apiClient } from './client'

export interface UserState {
  id: string
  createdAt: string
  sleepMinutes: number
  energyLevel: number
  stressLevel: number
  motivationLevel: number
  concentrationLevel: number
}

export interface CreateUserStateRequest {
  sleepMinutes: number       // 0-1440
  energyLevel: number        // 1-10
  stressLevel: number        // 1-10
  motivationLevel: number    // 1-10
  concentrationLevel: number // 1-10
}

export const userStateApi = {
  create: (data: CreateUserStateRequest) =>
    apiClient.post<{ payload: UserState }>('/user-state/create', data),

  getLatest: () =>
    apiClient.get<{ payload: UserState }>('/user-state/latest'),

  getHistory: (limit = 7) =>
    apiClient.get<{ payload: { history: UserState[] } }>('/user-state/history', {
      params: { limit },
    }),
}
