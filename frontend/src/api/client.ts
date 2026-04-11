import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5114/api/v1.0'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  withCredentials: true, // JWT + CSRF cookies
  headers: {
    'Content-Type': 'application/json',
  },
})

// Читаем XSRF-TOKEN cookie (HttpOnly=false — доступен JS)
function getCsrfToken(): string | null {
  const match = document.cookie.match(/(^|;\s*)XSRF-TOKEN=([^;]*)/)
  return match ? decodeURIComponent(match[2]) : null
}

// Прикладываем CSRF-токен ко всем мутирующим запросам.
// В dev-режиме бэк отключает проверку, поэтому отсутствие cookie не ломает локалку.
apiClient.interceptors.request.use((config) => {
  const csrfToken = getCsrfToken()
  if (csrfToken) {
    config.headers['X-CSRF-TOKEN'] = csrfToken
  }
  return config
})

// Интерсептор для обработки 401 — попытка обновить токен.
// /auth/refresh тоже ставит XSRF-TOKEN cookie, поэтому повтор запроса
// уже уйдёт с актуальным CSRF-токеном.
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        await apiClient.post('/auth/refresh')
        return apiClient(originalRequest)
      } catch {
        // refresh провалился — полная перезагрузка чистит все React-состояние
        window.location.replace('/login')
        return Promise.reject(error)
      }
    }

    return Promise.reject(error)
  }
)
