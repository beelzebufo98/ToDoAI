import { useNavigate } from 'react-router-dom'
import { ClipboardList } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function NoStateTodayCard() {
  const navigate = useNavigate()

  return (
    <div className="flex flex-col items-center text-center py-16 px-6">
      <div className="h-12 w-12 rounded-2xl bg-amber-100 dark:bg-amber-950 flex items-center justify-center mb-4">
        <ClipboardList className="h-6 w-6 text-amber-600 dark:text-amber-400" />
      </div>
      <h2 className="text-base font-semibold text-foreground mb-1">Сначала заполните состояние</h2>
      <p className="text-sm text-muted-foreground mb-6 max-w-xs">
        Перед генерацией расписания нужно указать своё состояние на сегодня — энергию, стресс, мотивацию
      </p>
      <Button
        onClick={() => navigate('/profile')}
        className="bg-indigo-600 text-white hover:bg-indigo-500"
      >
        Заполнить состояние
      </Button>
    </div>
  )
}
