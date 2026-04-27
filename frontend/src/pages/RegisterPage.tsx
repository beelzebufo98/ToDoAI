import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { motion } from 'framer-motion'
import { Loader2, Sparkles } from 'lucide-react'

import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { PasswordInput } from '@/components/ui/password-input'

const schema = z.object({
  userName: z.string()
    .min(6, 'Минимум 6 символов')
    .max(100, 'Максимум 100 символов')
    .regex(/^[a-zA-Z0-9_]*$/, 'Только буквы, цифры и символ _'),
  firstName: z.string()
    .min(1, 'Введите имя')
    .max(100, 'Максимум 100 символов')
    .regex(/^[a-zA-Zа-яА-ЯёЁ]+$/, 'Только буквы'),
  lastName: z.string()
    .max(100, 'Максимум 100 символов')
    .regex(/^[a-zA-Zа-яА-ЯёЁ]*$/, 'Только буквы')
    .optional(),
  email: z.string()
    .min(1, 'Введите email')
    .email('Некорректный email'),
  password: z.string()
    .min(8, 'Минимум 8 символов')
    .max(64, 'Максимум 64 символа')
    .regex(/[a-z]/, 'Нужна хотя бы одна строчная буква')
    .regex(/[0-9]/, 'Нужна хотя бы одна цифра')
    .regex(/[^a-zA-Z0-9]/, 'Нужен хотя бы один спецсимвол'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Пароли не совпадают',
  path: ['confirmPassword'],
})

type FormData = z.infer<typeof schema>

export function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: FormData) => {
    try {
      setError(null)
      await registerUser({
        userName: data.userName,
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
      })
      navigate('/confirm-email', { state: { email: data.email, justSent: true } })
    } catch {
      setError('Ошибка регистрации. Возможно, логин или email уже заняты.')
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
        <div className="flex flex-col items-center mb-8 gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-foreground shadow-sm">
            <Sparkles className="h-5 w-5 text-background" />
          </div>
          <div className="text-center">
            <h1 className="text-xl font-semibold tracking-tight text-foreground">ToDoAI</h1>
            <p className="text-muted-foreground text-sm mt-0.5">Умный планировщик задач</p>
          </div>
        </div>

        <div className="bg-card rounded-2xl shadow-sm border border-border/50 p-6">
          <div className="mb-5">
            <h2 className="text-base font-semibold text-foreground">Создать аккаунт</h2>
            <p className="text-sm text-muted-foreground mt-0.5">Займёт меньше минуты</p>
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

            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <Label htmlFor="firstName" className="text-sm font-medium">Имя</Label>
                <Input
                  id="firstName"
                  className="bg-background/80 h-10 rounded-lg border-border/60"
                  {...register('firstName')}
                />
                {errors.firstName && (
                  <p className="text-xs text-destructive">{errors.firstName.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="lastName" className="text-sm font-medium">Фамилия</Label>
                <Input
                  id="lastName"
                  className="bg-background/80 h-10 rounded-lg border-border/60"
                  {...register('lastName')}
                />
                {errors.lastName && (
                  <p className="text-xs text-destructive">{errors.lastName.message}</p>
                )}
              </div>
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="userName" className="text-sm font-medium">Логин</Label>
              <Input
                id="userName"
                autoComplete="username"
                className="bg-background/80 h-10 rounded-lg border-border/60"
                {...register('userName')}
              />
              {errors.userName && (
                <p className="text-xs text-destructive">{errors.userName.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="email" className="text-sm font-medium">Email</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                className="bg-background/80 h-10 rounded-lg border-border/60"
                {...register('email')}
              />
              {errors.email && (
                <p className="text-xs text-destructive">{errors.email.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="password" className="text-sm font-medium">Пароль</Label>
              <PasswordInput
                id="password"
                autoComplete="new-password"
                className="bg-background/80 h-10 rounded-lg border-border/60"
                {...register('password')}
              />
              {errors.password && (
                <p className="text-xs text-destructive">{errors.password.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="confirmPassword" className="text-sm font-medium">Подтверждение пароля</Label>
              <PasswordInput
                id="confirmPassword"
                autoComplete="new-password"
                className="bg-background/80 h-10 rounded-lg border-border/60"
                {...register('confirmPassword')}
              />
              {errors.confirmPassword && (
                <p className="text-xs text-destructive">{errors.confirmPassword.message}</p>
              )}
            </div>

            <Button
              type="submit"
              disabled={isSubmitting}
              className="w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500 active:scale-[0.98] mt-2"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
              Зарегистрироваться
            </Button>
          </form>
        </div>

        <p className="text-sm text-muted-foreground text-center mt-5">
          Уже есть аккаунт?{' '}
          <Link
            to="/login"
            className="text-foreground font-medium underline-offset-4 hover:underline"
          >
            Войти
          </Link>
        </p>
      </motion.div>
    </div>
  )
}
