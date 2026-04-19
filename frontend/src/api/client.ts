import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5114/api/v1.0'

export const apiClient = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

let refreshRequestPromise: Promise<void> | null = null

export function refreshSession(): Promise<void> {
  if (refreshRequestPromise) {
    return refreshRequestPromise
  }

  refreshRequestPromise = apiClient
    .post('/auth/refresh')
    .then(() => undefined)
    .finally(() => {
      refreshRequestPromise = null
    })

  return refreshRequestPromise
}

function getCsrfToken(): string | null {
  const match = document.cookie.match(/(^|;\s*)XSRF-TOKEN=([^;]*)/)
  return match ? decodeURIComponent(match[2]) : null
}

apiClient.interceptors.request.use((config) => {
  const csrfToken = getCsrfToken()
  if (csrfToken) {
    config.headers['X-CSRF-TOKEN'] = csrfToken
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const requestUrl = originalRequest?.url ?? ''

    if (requestUrl.includes('/auth/refresh')) {
      return Promise.reject(error)
    }

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        await refreshSession()
        return apiClient(originalRequest)
      } catch {
        window.location.replace('/login')
        return Promise.reject(error)
      }
    }

    return Promise.reject(error)
  }
)
