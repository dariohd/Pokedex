const store = new Map<string, { value: unknown; expires: number }>()
const TTL = 1000 * 60 * 60 * 24

export function getCached<T>(key: string): T | undefined {
  const entry = store.get(key)
  if (!entry) return undefined
  if (Date.now() > entry.expires) {
    store.delete(key)
    return undefined
  }
  return entry.value as T
}

export function setCache<T>(key: string, value: T): void {
  store.set(key, { value, expires: Date.now() + TTL })
}
