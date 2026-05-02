export const avatarVariants = ['bot-1', 'bot-2', 'bot-3', 'bot-4', 'bot-5', 'bot-6'] as const

export type AvatarVariant = (typeof avatarVariants)[number]

export const avatarVariantLabels: Record<AvatarVariant, string> = {
  'bot-1': 'Скаут',
  'bot-2': 'Пульс',
  'bot-3': 'Вектор',
  'bot-4': 'Нова',
  'bot-5': 'Титан',
  'bot-6': 'Спарк',
}

const DEFAULT_AVATAR_VARIANT: AvatarVariant = 'bot-1'

function avatarPreferenceKey(userId: string) {
  return `todoai:avatar-variant:${userId}`
}

export function getAvatarVariant(userId: string | null | undefined): AvatarVariant {
  if (!userId) {
    return DEFAULT_AVATAR_VARIANT
  }

  const stored = window.localStorage.getItem(avatarPreferenceKey(userId))
  if (stored && avatarVariants.includes(stored as AvatarVariant)) {
    return stored as AvatarVariant
  }

  return DEFAULT_AVATAR_VARIANT
}

export function setAvatarVariant(userId: string, variant: AvatarVariant) {
  window.localStorage.setItem(avatarPreferenceKey(userId), variant)
}
