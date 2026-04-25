import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { LayoutDashboard, CheckSquare, User, LogOut, Sparkles } from 'lucide-react'
import { motion } from 'framer-motion'
import { cn } from '@/lib/utils'
import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { ActiveSessionBanner } from '@/components/session/ActiveSessionBanner'

const navItems = [
  { to: '/', icon: LayoutDashboard, label: 'Главная' },
  { to: '/tasks', icon: CheckSquare, label: 'Задачи' },
  { to: '/profile', icon: User, label: 'Профиль' },
]

export function Layout() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen bg-background">
      {/* Sidebar */}
      <aside className="w-56 flex flex-col border-r border-border/60 bg-sidebar shrink-0">
        {/* Logo */}
        <div className="flex items-center gap-2.5 px-4 py-5">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary">
            <Sparkles className="h-4 w-4 text-primary-foreground" />
          </div>
          <span className="font-semibold text-sidebar-foreground tracking-tight">ToDoAI</span>
        </div>

        <Separator className="bg-sidebar-border" />

        {/* Nav */}
        <nav className="flex flex-col gap-1 p-3 flex-1">
          {navItems.map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              end={to === '/'}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                    : 'text-sidebar-foreground/70 hover:bg-sidebar-accent/60 hover:text-sidebar-foreground'
                )
              }
            >
              {({ isActive }) => (
                <>
                  <Icon className={cn('h-4 w-4 shrink-0', isActive ? 'opacity-100' : 'opacity-60')} />
                  {label}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <Separator className="bg-sidebar-border" />

        {/* Active session */}
        <div className="pt-2">
          <ActiveSessionBanner />
        </div>

        {/* Logout */}
        <div className="p-3">
          <Button
            variant="ghost"
            className="w-full justify-start gap-2.5 text-sidebar-foreground/70 hover:text-destructive hover:bg-destructive/10"
            onClick={handleLogout}
          >
            <LogOut className="h-4 w-4" />
            Выйти
          </Button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-y-auto">
        <motion.div
          key={location.pathname}
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.2 }}
          className="h-full"
        >
          <Outlet />
        </motion.div>
      </main>
    </div>
  )
}
