import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react'
import { authApi, type LoginRequest, type RegisterRequest } from '@/api/auth'

interface AuthContextValue {
  isAuthenticated: boolean
  isLoading: boolean
  login: (data: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  // true пока не проверили сессию на бэкенде
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Если localStorage говорит "авторизован" — проверяем реальность сессии через refresh
    const wasAuthenticated = localStorage.getItem('isAuthenticated') === 'true'
    if (!wasAuthenticated) {
      setIsLoading(false)
      return
    }

    authApi.refresh()
      .then(() => setIsAuthenticated(true))
      .catch(() => localStorage.removeItem('isAuthenticated'))
      .finally(() => setIsLoading(false))
  }, [])

  const login = useCallback(async (data: LoginRequest) => {
    await authApi.login(data)
    setIsAuthenticated(true)
    localStorage.setItem('isAuthenticated', 'true')
  }, [])

  const register = useCallback(async (data: RegisterRequest) => {
    await authApi.register(data)
    // register не выдаёт токены — логинимся сразу после
    await authApi.login({ userName: data.userName, password: data.password })
    setIsAuthenticated(true)
    localStorage.setItem('isAuthenticated', 'true')
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      // Чистим стейт даже если бэкенд вернул ошибку
      setIsAuthenticated(false)
      localStorage.removeItem('isAuthenticated')
    }
  }, [])

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
