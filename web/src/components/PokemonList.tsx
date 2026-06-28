import { useEffect, useState } from 'react'
import { enrichSummaries, getPokemonPage } from '../api/pokeapi'
import type { PokemonSummary } from '../types/pokemon'

export function PokemonList({
  selectedId,
  onSelect,
  favoriteIds = [],
}: {
  selectedId: number | null
  onSelect: (item: PokemonSummary) => void
  favoriteIds?: number[]
}) {
  const [page, setPage] = useState(0)
  const [items, setItems] = useState<PokemonSummary[]>([])
  const [filter, setFilter] = useState('')
  const [totalPages, setTotalPages] = useState(1)
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    getPokemonPage(page)
      .then(async (data) => {
        if (cancelled) return
        const enriched = await enrichSummaries(data.items)
        if (!cancelled) {
          setItems(enriched)
          setTotalPages(data.totalPages)
          setTotal(data.total)
        }
      })
      .catch((e: Error) => {
        if (!cancelled) setError(e.message)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [page])

  const filtered = items.filter((p) => {
    const q = filter.toLowerCase()
    if (!q) return true
    return (
      p.displayName.toLowerCase().includes(q) ||
      p.name.toLowerCase().includes(q) ||
      String(p.id).includes(q)
    )
  })

  return (
    <div className="flex h-full flex-col bg-white">
      <div className="bg-pokedex-red px-4 py-4 text-white">
        <h2 className="text-lg font-bold">Pokédex National</h2>
        <p className="text-xs text-red-100">{total} espèces</p>
        <input
          type="search"
          placeholder="Filtrer..."
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          className="mt-3 w-full rounded-lg border-0 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:ring-2 focus:ring-white"
        />
      </div>

      <div className="flex-1 overflow-y-auto">
        {loading && (
          <p className="p-4 text-center text-sm text-gray-500">Chargement...</p>
        )}
        {error && (
          <p className="p-4 text-center text-sm text-red-600">{error}</p>
        )}
        {!loading && !error && filtered.length === 0 && (
          <p className="p-4 text-center text-sm text-gray-500">Aucun résultat</p>
        )}
        <ul>
          {filtered.map((p) => (
            <li key={p.id}>
              <button
                type="button"
                onClick={() => onSelect(p)}
                className={`flex w-full items-center gap-3 border-b border-gray-100 px-4 py-3 text-left transition-colors hover:bg-red-50 ${
                  selectedId === p.id ? 'bg-red-50 border-l-4 border-l-pokedex-red' : ''
                }`}
              >
                <span className="w-10 shrink-0 text-sm font-bold text-gray-400">
                  {String(p.id).padStart(3, '0')}
                </span>
                <span className="font-medium text-gray-800">{p.displayName}</span>
                {favoriteIds.includes(p.id) && (
                  <span className="ml-auto text-amber-500" aria-hidden>★</span>
                )}
              </button>
            </li>
          ))}
        </ul>
      </div>

      <div className="flex items-center justify-between border-t border-gray-200 bg-gray-50 px-3 py-2">
        <button
          type="button"
          disabled={page === 0 || loading}
          onClick={() => setPage((p) => p - 1)}
          className="rounded-lg bg-pokedex-red px-3 py-1.5 text-sm font-bold text-white disabled:opacity-40"
        >
          ◀
        </button>
        <span className="text-sm font-medium text-gray-600">
          {page + 1} / {totalPages}
        </span>
        <button
          type="button"
          disabled={page >= totalPages - 1 || loading}
          onClick={() => setPage((p) => p + 1)}
          className="rounded-lg bg-pokedex-red px-3 py-1.5 text-sm font-bold text-white disabled:opacity-40"
        >
          ▶
        </button>
      </div>
    </div>
  )
}
