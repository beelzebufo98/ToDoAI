import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { motion } from 'framer-motion'
import { Loader2, Sparkles } from 'lucide-react'

import { authApi } from '@/api/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

const schema = z.object({
  email: z.string().min(1, 'Введите email').email('Некорректный email'),
})

type FormData = z.infer<typeof schema>

export function ForgotPasswordPage() {
  const navigate = useNavigate()
  const [sent, setSent] = useState(false)
  const [sentEmail, setSentEmail] = useState('')
  const [error, setError] = useState<string | null>(null)
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (data: FormData) => {
    // Always shows neutral message — backend never reveals whether email exists
    try {
      setError(null)
      await authApi.forgotPassword(data.email)
      setSentEmail(data.email)
      setSent(true)
    } catch {
      setError('Не удалось отправить запрос. Попробуйте ещё раз.')
    }
  }

  if (sent) {
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
          <div className="bg-card rounded-2xl shadow-sm border border-border/50 p-6">
            <h2 className="text-base font-semibold text-foreground">Проверьте почту</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Если аккаунт с адресом <span className="font-medium text-foreground">{sentEmail}</span> существует
              и email подтверждён, мы отправили код для сброса пароля.
            </p>
            <Button
              onClick={() => navigate('/reset-password', { state: { email: sentEmail } })}
              className="mt-5 w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500"
            >
              Ввести код
            </Button>
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
            <h2 className="text-base font-semibold text-foreground">Забыли пароль?</h2>
            <p className="text-sm text-muted-foreground mt-0.5">
              Введите email — мы пришлём код для сброса
            </p>
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

            <Button
              type="submit"
              disabled={isSubmitting}
              className="w-full h-10 bg-indigo-600 text-white hover:bg-indigo-500 active:scale-[0.98]"
            >
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />}
              Отправить код
            </Button>
          </form>
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
