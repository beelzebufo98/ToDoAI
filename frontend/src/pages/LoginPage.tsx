import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { motion } from 'framer-motion'
import { Loader2, Sparkles } from 'lucide-react'
import axios from 'axios'

import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const EMAIL_NOT_CONFIRMED = 'email_not_confirmed'

const schema = z.object({
  userName: z.string().min(1, 'Введите логин'),
  password: z.string().min(1, 'Введите пароль'),
})

type FormData = z.infer<typeof schema>

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [emailNotConfirmed, setEmailNotConfirmed] = useState(false)
  const [confirmationEmail, setConfirmationEmail] = useState('')
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const isConfirmationEmailValid = z.string().email().safeParse(confirmationEmail).success

  const onSubmit = async (data: FormData) => {
    try {
      setError(null)
      setEmailNotConfirmed(false)
      setConfirmationEmail('')
      await login(data)
      navigate('/')
    } catch (err) {
      const code = axios.isAxiosError(err) ? err.response?.data?.error?.code : null
      if (code === EMAIL_NOT_CONFIRMED) {
        setEmailNotConfirmed(true)
        setConfirmationEmail(data.userName.includes('@') ? data.userName : '')
      } else {
        setError('Неверный логин или пароль')
      }
    }
  }

  return (
    <div
      className="min-h-screen flex items-center justify-center p-4"
      style={{ background: 'radial-gradient(ellipse 100% 55% at 50% 0%, oklch(0.80 0.14 265 / 0.55) 0%, oklch(0.97 0.006 80) 65%)' }}
    >
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35, ease: 'easeOut' }}
        className="w-full max-w-[400px]"
      >
        {/* Logo */}
        <div className="flex flex-col items-center mb-8 gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-foreground shadow-sm">
            <Sparkles className="h-5 w-5 text-background" />
          </div>
          <div className="text-center">
            <h1 className="text-xl font-semibold tracking-tight text-foreground">ToDoAI</h1>
            <p className="text-muted-foreground text-sm mt-0.5">Умный планировщик задач</p>
          </div>
        </div>

        {/* Card */}
        <div className="bg-card rounded-2xl shadow-sm border border-border/50 p-6">
          <div className="mb-5">
            <h2 className="text-base font-semibold text-foreground">Вход в аккаунт</h2>
            <p className="text-sm text-muted-foreground mt-0.5">Рады снова видеть вас</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {error && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="text-sm text-destructive bg-destructive/8 border border-destructive/20 px-3 py-2 rounded-lg"
              >
                {error}
              </motion.div>
            )}

            {emailNotConfirmed && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2.5 dark:border-amber-800 dark:bg-amber-950"
              >
                <p className="text-sm text-amber-800 dark:text-amber-300">
                  Сначала подтвердите email
                </p>
                <p className="mt-1 text-xs text-amber-700 dark:text-amber-400">
                  Укажите email, который использовали при регистрации.
                </p>
                <Input
                  type="email"
                  value={confirmationEmail}
                  onChange={(e) => setConfirmationEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="mt-3 h-10 border-amber-200 bg-white/80 dark:border-amber-800 dark:bg-amber-950/60"
                />
                <Button
                  type="button"
                  onClick={() => navigate('/confirm-email', { state: { email: confirmationEmail } })}
                  disabled={!isConfirmationEmailValid}
                  className="mt-3 w-full bg-amber-600 text-white hover:bg-amber-500"
                >
                  Перейти к подтверждению
                </Button>
              </motion.div>
            )}

            <div className="space-y-1.5">
              <Label htmlFor="userName" className="text-sm font-medium">Логин</Label>
              <Input
                id="userName"
                autoComplete="username"
                className="bg-background/80 h-10 rounded-lg border-border/60 focus-visible:ring-foreground/20"
                {...register('userName')}
              />
              {errors.userName && (
                <p className="text-xs text-destructive">{errors.userName.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <div className="flex items-center justify-between">
                <Label htmlFor="password" className="text-sm font-medium">Пароль</Label>
                <Link
                  to="/forgot-password"
                  className="text-xs text-muted-foreground underline-offset-4 hover:underline"
                >
                  Забыли пароль?
                </Link>
              </div>
              <Input
                id="password"
                type="password"
                autoComplete="current-password"
                className="bg-background/80 h-10 rounded-lg border-border/60 focus-visible:ring-foreground/20"
                {...register('password')}
              />
              {errors.password && (
                <p className="text-xs text-destructive">{errors.password.message}</p>
              )}
            </div>

            <Button
              type="submit"
              disabled={isSubmitting}
              className="w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500 active:scale-[0.98] mt-2"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
              Войти
            </Button>
          </form>
        </div>

        <p className="text-sm text-muted-foreground text-center mt-5">
          Нет аккаунта?{' '}
          <Link
            to="/register"
            className="text-foreground font-medium underline-offset-4 hover:underline"
          >
            Зарегистрироваться
          </Link>
        </p>
      </motion.div>
    </div>
  )
}
