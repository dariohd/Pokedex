import { useCallback, useEffect, useState } from 'react'
import { getPokemonDetail, searchPokemon } from './api/pokeapi'
import { PokemonList } from './components/PokemonList'
import { PokemonDetailView } from './components/PokemonDetailView'
import {
  addSearchHistory,
  getFavorites,
  getSearchHistory,
  isFavorite,
  toggleFavorite,
} from './lib/userData'
import type { PokemonDetail, PokemonSummary } from './types/pokemon'

export default function App() {
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [pokemon, setPokemon] = useState<PokemonDetail | null>(null)
  const [comparePokemon, setComparePokemon] = useState<PokemonDetail | null>(null)
  const [compareMode, setCompareMode] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [shiny, setShiny] = useState(false)
  const [history, setHistory] = useState<string[]>(() => getSearchHistory())
  const [favorites, setFavorites] = useState<number[]>(() => getFavorites())
  const [offline, setOffline] = useState(false)

  const loadPokemon = useCallback(async (slug: string, id?: number) => {
    setLoading(true)
    setError(null)
    setOffline(false)
    try {
      const detail = await getPokemonDetail(slug)
      setPokemon(detail)
      setSelectedId(id ?? detail.id)
    } catch (e) {
      setOffline(e instanceof TypeError)
      setError(e instanceof Error ? e.message : 'Erreur inconnue')
      setPokemon(null)
    } finally {
      setLoading(false)
    }
  }, [])

  const handleSelect = async (item: PokemonSummary) => {
    if (compareMode && pokemon && !comparePokemon) {
      try {
        setComparePokemon(await getPokemonDetail(String(item.id)))
      } catch {
        setError('Comparaison impossible')
      }
      return
    }
    setComparePokemon(null)
    void loadPokemon(String(item.id), item.id)
  }

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    const q = search.trim()
    if (!q) return

    setLoading(true)
    setError(null)
    setOffline(false)
    try {
      const result = await searchPokemon(q)
      if (result) {
        setHistory(addSearchHistory(q))
        if (compareMode && pokemon && !comparePokemon) {
          setComparePokemon(result)
        } else {
          setPokemon(result)
          setSelectedId(result.id)
          setComparePokemon(null)
        }
      } else {
        setError(`Aucun Pokémon trouvé pour « ${q} »`)
        setPokemon(null)
      }
    } catch {
      setOffline(true)
      setError('API inaccessible')
    } finally {
      setLoading(false)
    }
  }

  const handleToggleFavorite = () => {
    if (!pokemon) return
    setFavorites(toggleFavorite(pokemon.id))
  }

  const handlePlayCry = () => {
    if (!pokemon?.cryUrl) return
    const audio = new Audio(pokemon.cryUrl)
    audio.play().catch(() => setError('Impossible de lire le cri'))
  }

  useEffect(() => {
    setFavorites(getFavorites())
  }, [pokemon?.id])

  const compareBst = comparePokemon
    ? comparePokemon.stats.reduce((s, x) => s + x.value, 0)
    : 0
  const bst = pokemon?.stats.reduce((s, x) => s + x.value, 0) ?? 0

  return (
    <div className="flex h-screen flex-col">
      <header className="flex shrink-0 flex-col gap-2 border-b border-gray-200 bg-white px-4 py-3 shadow-sm">
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-pokedex-red text-white">
              <div className="h-3 w-3 rounded-full bg-white" />
            </div>
            <span className="text-xl font-bold text-gray-900">Pokédex</span>
          </div>

          <form onSubmit={handleSearch} className="flex flex-1 flex-wrap gap-2 min-w-[200px] max-w-2xl">
            <input
              type="search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Rechercher (pikachu, #25, dracaufeu...)"
              aria-label="Recherche Pokémon"
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm focus:border-pokedex-red focus:outline-none focus:ring-1 focus:ring-pokedex-red"
            />
            <button
              type="submit"
              className="rounded-lg bg-pokedex-red px-4 py-2 text-sm font-bold text-white hover:bg-pokedex-red-dark"
            >
              Chercher
            </button>
            <button
              type="button"
              onClick={handleToggleFavorite}
              disabled={!pokemon}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm font-bold disabled:opacity-40"
              title="Favori"
            >
              {pokemon && isFavorite(pokemon.id) ? '★' : '☆'}
            </button>
            <button
              type="button"
              onClick={() => {
                setCompareMode((v) => !v)
                setComparePokemon(null)
              }}
              className={`rounded-lg border px-3 py-2 text-sm font-bold ${
                compareMode ? 'border-pokedex-red bg-red-50 text-pokedex-red' : 'border-gray-300'
              }`}
              title="Comparer"
            >
              ⇄
            </button>
            <button
              type="button"
              onClick={handlePlayCry}
              disabled={!pokemon?.cryUrl}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm font-bold disabled:opacity-40"
              title="Cri"
            >
              ♫
            </button>
          </form>

          <label className="flex cursor-pointer items-center gap-2 text-sm font-medium text-gray-700">
            <input
              type="checkbox"
              checked={shiny}
              onChange={(e) => setShiny(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-pokedex-red focus:ring-pokedex-red"
            />
            Chromatique
          </label>
        </div>

        {offline && (
          <p className="rounded-lg bg-amber-50 px-3 py-2 text-center text-sm text-amber-800">
            Mode hors ligne : impossible de joindre PokéAPI.
          </p>
        )}

        {history.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {history.map((item) => (
              <button
                key={item}
                type="button"
                onClick={() => {
                  setSearch(item)
                  void searchPokemon(item).then((r) => {
                    if (r) {
                      setPokemon(r)
                      setSelectedId(r.id)
                    }
                  })
                }}
                className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-700 hover:bg-gray-200"
              >
                {item}
              </button>
            ))}
          </div>
        )}
      </header>

      <div className="grid min-h-0 flex-1 grid-cols-1 lg:grid-cols-[340px_1fr]">
        <aside className="hidden border-r border-gray-200 lg:block">
          <PokemonList selectedId={selectedId} onSelect={handleSelect} favoriteIds={favorites} />
        </aside>

        <main className="min-h-0 overflow-hidden bg-pokedex-bg">
          <PokemonDetailView pokemon={pokemon} shiny={shiny} loading={loading} error={error} />
          {comparePokemon && pokemon && (
            <div className="border-t border-gray-200 bg-gray-50 px-6 py-3 text-sm">
              <span className="font-bold">Comparaison BST : </span>
              {pokemon.displayName} ({bst}) vs {comparePokemon.displayName} ({compareBst})
            </div>
          )}
        </main>
      </div>

      <details className="border-t border-gray-200 bg-white lg:hidden">
        <summary className="cursor-pointer px-4 py-3 text-sm font-bold text-pokedex-red">
          Ouvrir la liste des Pokémon
        </summary>
        <div className="h-80">
          <PokemonList selectedId={selectedId} onSelect={handleSelect} favoriteIds={favorites} />
        </div>
      </details>
    </div>
  )
}
