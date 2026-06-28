export function normalizeQuery(query: string): string {
  let q = query.trim()
  if (q.startsWith('#')) q = q.slice(1).trim()
  return q
}

export function tryParseId(query: string): number | null {
  const q = normalizeQuery(query)
  const id = parseInt(q, 10)
  return Number.isFinite(id) && id > 0 ? id : null
}
