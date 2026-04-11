import { apiClient } from './client'

export interface CreateUserStateRequest {
  sleepMinutes: number
  energyLevel: number      // 1-10
  stressLevel: number      // 1-10
  motivationLevel: number  // 1-10
  concentrationLevel: number // 1-10
}

export const userStateApi = {
  create: (data: CreateUserStateRequest) =>
    apiClient.post('/user-state/create', data),
}
