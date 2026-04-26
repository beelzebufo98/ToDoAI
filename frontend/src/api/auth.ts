import { apiClient, refreshSession } from './client'

export interface LoginRequest {
  userName: string
  password: string
}

export interface RegisterRequest {
  userName: string
  firstName: string
  lastName?: string
  email: string
  password: string
}

export interface ConfirmEmailRequest {
  email: string
  code: string
}

export interface ResetPasswordRequest {
  email: string
  code: string
  newPassword: string
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post('/auth/login', data),

  register: (data: RegisterRequest) =>
    apiClient.post('/auth/register', data),

  confirmEmail: (data: ConfirmEmailRequest) =>
    apiClient.post('/auth/confirm-email', data),

  resendConfirmationCode: (email: string) =>
    apiClient.post('/auth/resend-confirmation-code', { email }),

  forgotPassword: (email: string) =>
    apiClient.post('/auth/forgot-password', { email }),

  resetPassword: (data: ResetPasswordRequest) =>
    apiClient.post('/auth/reset-password', data),

  logout: () =>
    apiClient.post('/auth/logout'),

  refresh: () =>
    refreshSession(),
}
