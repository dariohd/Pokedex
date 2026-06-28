const FAVORITES_KEY = 'pokedex:favorites'
const HISTORY_KEY = 'pokedex:search-history'
const MAX_HISTORY = 8

function readJson<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key)
    return raw ? (JSON.parse(raw) as T) : fallback
  } catch {
    return fallback
  }
}

function writeJson(key: string, value: unknown): void {
  localStorage.setItem(key, JSON.stringify(value))
}

export function getFavorites(): number[] {
  return readJson<number[]>(FAVORITES_KEY, [])
}

export function toggleFavorite(id: number): number[] {
  const current = getFavorites()
  const next = current.includes(id)
    ? current.filter((x) => x !== id)
    : [...current, id]
  writeJson(FAVORITES_KEY, next)
  return next
}

export function isFavorite(id: number): boolean {
  return getFavorites().includes(id)
}

export function getSearchHistory(): string[] {
  return readJson<string[]>(HISTORY_KEY, [])
}

export function addSearchHistory(query: string): string[] {
  const trimmed = query.trim()
  if (!trimmed) return getSearchHistory()
  const next = [trimmed, ...getSearchHistory().filter((q) => q !== trimmed)].slice(0, MAX_HISTORY)
  writeJson(HISTORY_KEY, next)
  return next
}
