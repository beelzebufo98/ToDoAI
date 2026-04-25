import { createContext, useCallback, useContext, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import axios from 'axios'
import { toast } from 'sonner'
import { taskWorkSessionsApi, type TaskWorkSession } from '@/api/taskWorkSessions'
import { useAuth } from '@/contexts/AuthContext'

interface TaskSessionContextValue {
  current: TaskWorkSession | null
  syncing: boolean
  startSession: (taskId: string, scheduleId?: string) => Promise<void>
  stopSession: () => Promise<void>
  cancelSession: () => Promise<void>
  resync: () => Promise<void>
}

const TaskSessionContext = createContext<TaskSessionContextValue | null>(null)

export function useTaskSession() {
  const ctx = useContext(TaskSessionContext)
  if (!ctx) {
    throw new Error('useTaskSession must be used within TaskSessionProvider')
  }

  return ctx
}

export function TaskSessionProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient()
  const { isAuthenticated } = useAuth()
  const [syncing, setSyncing] = useState(false)

  const { data: current = null } = useQuery({
    queryKey: ['session', 'current'],
    queryFn: () => taskWorkSessionsApi.getCurrent().then(response => response.data.payload),
    staleTime: 0,
    retry: false,
    enabled: isAuthenticated,
  })

  const resync = useCallback(async () => {
    await queryClient.refetchQueries({ queryKey: ['session', 'current'] })
  }, [queryClient])

  const invalidateRelatedData = useCallback(
    async (taskId?: string) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['tasks'] }),
        queryClient.invalidateQueries({ queryKey: ['schedule'] }),
        queryClient.invalidateQueries({ queryKey: ['sessions', 'day'] }),
        ...(taskId ? [queryClient.invalidateQueries({ queryKey: ['task', taskId] })] : []),
      ])
    },
    [queryClient]
  )

  const startSession = useCallback(
    async (taskId: string, scheduleId?: string) => {
      setSyncing(true)
      try {
        await taskWorkSessionsApi.start(taskId, scheduleId)
        await resync()
        await invalidateRelatedData(taskId)
      } catch (error) {
        if (axios.isAxiosError(error) && error.response?.status === 409) {
          await resync()
          await invalidateRelatedData(taskId)
          toast.info('Сессия уже активна')
        } else {
          toast.error('Не удалось начать сессию')
        }

        throw error
      } finally {
        setSyncing(false)
      }
    },
    [invalidateRelatedData, resync]
  )

  const stopSession = useCallback(async () => {
    if (!current) {
      return
    }

    setSyncing(true)
    try {
      await taskWorkSessionsApi.stop(current.sessionId)
      queryClient.setQueryData<TaskWorkSession | null>(['session', 'current'], null)
      await invalidateRelatedData(current.taskId)
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        queryClient.setQueryData<TaskWorkSession | null>(['session', 'current'], null)
        await invalidateRelatedData(current.taskId)
      } else {
        toast.error('Не удалось остановить сессию')
        await resync()
      }
    } finally {
      setSyncing(false)
    }
  }, [current, invalidateRelatedData, queryClient, resync])

  const cancelSession = useCallback(async () => {
    if (!current) {
      return
    }

    setSyncing(true)
    try {
      await taskWorkSessionsApi.cancel(current.sessionId)
      queryClient.setQueryData<TaskWorkSession | null>(['session', 'current'], null)
      await invalidateRelatedData(current.taskId)
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        queryClient.setQueryData<TaskWorkSession | null>(['session', 'current'], null)
        await invalidateRelatedData(current.taskId)
      } else {
        toast.error('Не удалось отменить сессию')
        await resync()
      }
    } finally {
      setSyncing(false)
    }
  }, [current, invalidateRelatedData, queryClient, resync])

  return (
    <TaskSessionContext.Provider
      value={{ current, syncing, startSession, stopSession, cancelSession, resync }}
    >
      {children}
    </TaskSessionContext.Provider>
  )
}
