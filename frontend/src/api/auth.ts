import { apiClient } from './client'

export interface LoginRequest {
  userName: string
  password: string
}

export interface RegisterRequest {
  userName: string
  firstName: string
  lastName?: string
  password: string
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post('/auth/login', data),

  register: (data: RegisterRequest) =>
    apiClient.post('/auth/register', data),

  logout: () =>
    apiClient.post('/auth/logout'),

  refresh: () =>
    apiClient.post('/auth/refresh'),
}
