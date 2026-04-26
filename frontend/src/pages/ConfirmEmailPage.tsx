import { useEffect, useRef, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { motion } from 'framer-motion'
import { CheckCircle2, Loader2, MailOpen, Sparkles } from 'lucide-react'
import axios from 'axios'

import { authApi } from '@/api/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const INCORRECT_VALUE = 'incorrect_value'
const RESEND_COOLDOWN = 60

const schema = z.object({
  email: z.string().min(1, 'Введите email').email('Некорректный email'),
  code: z.string().length(6, 'Код состоит из 6 цифр').regex(/^\d+$/, 'Только цифры'),
})

type FormData = z.infer<typeof schema>

export function ConfirmEmailPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const routeState = (location.state as { email?: string; justSent?: boolean } | null) ?? null
  const prefillEmail = routeState?.email ?? ''

  const [confirmed, setConfirmed] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [resendCooldown, setResendCooldown] = useState(0)
  const [resendMessage, setResendMessage] = useState<string | null>(null)
  const cooldownRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const { register, handleSubmit, getValues, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: prefillEmail, code: '' },
  })

  useEffect(() => {
    return () => {
      if (cooldownRef.current) {
        clearInterval(cooldownRef.current)
      }
    }
  }, [])

  useEffect(() => {
    if (routeState?.justSent) {
      setResendMessage('Код уже отправлен на вашу почту')
      startCooldown()
    }
  }, [routeState?.justSent])

  const startCooldown = () => {
    setResendCooldown(RESEND_COOLDOWN)
    cooldownRef.current = setInterval(() => {
      setResendCooldown((prev) => {
        if (prev <= 1) {
          if (cooldownRef.current) {
            clearInterval(cooldownRef.current)
          }
          return 0
        }

        return prev - 1
      })
    }, 1000)
  }

  const onSubmit = async (data: FormData) => {
    try {
      setError(null)
      await authApi.confirmEmail({ email: data.email, code: data.code })
      setConfirmed(true)
    } catch (err) {
      const code = axios.isAxiosError(err) ? err.response?.data?.error?.code : null
      if (code === INCORRECT_VALUE) {
        setError('Код неверный или истёк. Запросите новый.')
      } else {
        setError('Не удалось подтвердить email. Попробуйте ещё раз.')
      }
    }
  }

  const onResend = async () => {
    if (resendCooldown > 0) {
      return
    }

    const email = getValues('email')
    if (!email) {
      return
    }

    try {
      setResendMessage(null)
      await authApi.resendConfirmationCode(email)
      setResendMessage('Код отправлен повторно')
      startCooldown()
    } catch {
      setResendMessage('Не удалось отправить код. Попробуйте позже.')
    }
  }

  if (confirmed) {
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
          </div>

          <div className="bg-card rounded-2xl shadow-sm border border-border/50 p-6 text-center">
            <CheckCircle2 className="mx-auto mb-3 h-10 w-10 text-green-500" />
            <h2 className="text-base font-semibold text-foreground">Email подтверждён</h2>
            <p className="mt-1 text-sm text-muted-foreground">Теперь вы можете войти в аккаунт</p>
            <Button
              onClick={() => navigate('/login')}
              className="mt-5 w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500"
            >
              Войти
            </Button>
          </div>
        </motion.div>
      </div>
    )
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
          <div className="mb-5 flex items-start gap-3">
            <MailOpen className="mt-0.5 h-5 w-5 shrink-0 text-indigo-500" />
            <div>
              <h2 className="text-base font-semibold text-foreground">Подтвердите email</h2>
              <p className="text-sm text-muted-foreground mt-0.5">
                Мы отправили код подтверждения на ваш email
              </p>
            </div>
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
              <Label htmlFor="code" className="text-sm font-medium">Код подтверждения</Label>
              <Input
                id="code"
                inputMode="numeric"
                autoComplete="one-time-code"
                maxLength={6}
                className="bg-background/80 h-10 rounded-lg border-border/60 tracking-widest text-center font-mono"
                {...register('code')}
              />
              {errors.code && (
                <p className="text-xs text-destructive">{errors.code.message}</p>
              )}
            </div>

            <Button
              type="submit"
              disabled={isSubmitting}
              className="w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500 active:scale-[0.98]"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
              Подтвердить
            </Button>
          </form>

          <div className="mt-4 border-t border-border/40 pt-4">
            {resendMessage && (
              <p className="mb-2 text-xs text-muted-foreground">{resendMessage}</p>
            )}
            <button
              type="button"
              onClick={onResend}
              disabled={resendCooldown > 0}
              className="text-sm text-muted-foreground underline-offset-4 hover:underline disabled:cursor-not-allowed disabled:opacity-50"
            >
              {resendCooldown > 0
                ? `Отправить повторно через ${resendCooldown} с`
                : 'Отправить код повторно'}
            </button>
          </div>
        </div>

        <p className="text-sm text-muted-foreground text-center mt-5">
          <Link
            to="/login"
            className="text-foreground font-medium underline-offset-4 hover:underline"
          >
            Вернуться ко входу
          </Link>
        </p>
      </motion.div>
    </div>
  )
}
