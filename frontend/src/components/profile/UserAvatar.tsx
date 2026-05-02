import { useMemo } from 'react'
import { createAvatar } from '@dicebear/core'
import { bottts } from '@dicebear/collection'
import type { AvatarVariant } from '@/lib/avatar-preferences'
import { cn } from '@/lib/utils'

interface UserAvatarProps {
  seed: string
  variant: AvatarVariant
  size?: number
  className?: string
}

export function UserAvatar({ seed, variant, size = 80, className }: UserAvatarProps) {
  const src = useMemo(() => {
    return createAvatar(bottts, {
      seed: `${seed}:${variant}`,
      size,
      backgroundType: ['solid'],
      radius: 50,
      scale: 90,
    }).toDataUri()
  }, [seed, size, variant])

  return (
    <div className={cn('overflow-hidden rounded-full border border-border/60 bg-card', className)}>
      <img src={src} alt="" width={size} height={size} className="block rounded-full" />
    </div>
  )
}
