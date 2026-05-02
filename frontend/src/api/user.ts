import { apiClient } from './client'

export interface UserProfile {
  userId: string
  userName: string
  firstName: string
  lastName: string | null
  email: string | null
}

export const userApi = {
  getMe: () => apiClient.get<{ payload: UserProfile }>('/user/me'),
}
