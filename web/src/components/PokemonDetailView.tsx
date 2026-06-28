import { useState } from 'react'
import type { PokemonDetail } from '../types/pokemon'
import { TypeBadge } from './TypeBadge'
import { StatChart } from './StatChart'
import { EvolutionTree } from './EvolutionTree'

type Tab = 'details' | 'stats' | 'combat' | 'forms' | 'evolutions'

export function PokemonDetailView({
  pokemon,
  shiny,
  loading,
  error,
}: {
  pokemon: PokemonDetail | null
  shiny: boolean
  loading: boolean
  error: string | null
}) {
  const [tab, setTab] = useState<Tab>('details')

  if (loading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <div className="mx-auto mb-4 h-12 w-12 animate-spin rounded-full border-4 border-pokedex-red border-t-transparent" />
          <p className="text-gray-600">Chargement du Pokémon...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex h-full items-center justify-center p-8">
        <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-center text-red-700">
          <p className="font-bold">Erreur</p>
          <p className="mt-2 text-sm">{error}</p>
        </div>
      </div>
    )
  }

  if (!pokemon) {
    return (
      <div className="flex h-full flex-col items-center justify-center gap-4 p-8 text-gray-500">
        <div className="text-6xl opacity-30">?</div>
        <p className="text-lg">Sélectionnez un Pokémon dans la liste</p>
        <p className="text-sm">ou utilisez la barre de recherche</p>
      </div>
    )
  }

  const sprite = shiny ? pokemon.shinySpriteUrl : pokemon.spriteUrl
  const bst = pokemon.stats.reduce((s, x) => s + x.value, 0)
  const tags = [
    pokemon.isLegendary && 'Légendaire',
    pokemon.isMythical && 'Fabuleux',
  ].filter(Boolean)

  const tabs: { id: Tab; label: string }[] = [
    { id: 'details', label: 'Détails' },
    { id: 'stats', label: 'Stats' },
    { id: 'combat', label: 'Combat' },
    { id: 'forms', label: 'Formes' },
    { id: 'evolutions', label: 'Évolutions' },
  ]

  return (
    <div className="flex h-full flex-col overflow-hidden">
      <header className="border-b border-gray-200 bg-white px-6 py-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-gray-500">N° {String(pokemon.id).padStart(3, '0')}</p>
            <h1 className="text-3xl font-bold text-pokedex-red">{pokemon.displayName}</h1>
            {pokemon.genus && (
              <p className="text-sm italic text-gray-500">{pokemon.genus}</p>
            )}
            {tags.length > 0 && (
              <div className="mt-2 flex gap-2">
                {tags.map((t) => (
                  <span
                    key={String(t)}
                    className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-bold text-amber-800"
                  >
                    {t}
                  </span>
                ))}
              </div>
            )}
          </div>
          <div className="flex flex-wrap gap-2">
            {pokemon.types.map((t) => (
              <TypeBadge key={t} type={t} />
            ))}
          </div>
        </div>
        <p className="mt-2 text-sm text-gray-500">BST {bst}</p>
      </header>

      <nav className="flex border-b border-gray-200 bg-white px-4">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`border-b-2 px-5 py-3 text-sm font-semibold transition-colors ${
              tab === t.id
                ? 'border-pokedex-red text-pokedex-red'
                : 'border-transparent text-gray-500 hover:text-gray-800'
            }`}
          >
            {t.label}
          </button>
        ))}
      </nav>

      <div className="flex-1 overflow-y-auto bg-white">
        {tab === 'details' && (
          <div className="grid gap-6 p-6 lg:grid-cols-2">
            <div className="flex items-center justify-center rounded-2xl bg-gradient-to-b from-gray-50 to-white p-6">
              <img
                src={sprite}
                alt={pokemon.displayName}
                className="max-h-72 w-auto object-contain drop-shadow-lg"
                onError={(e) => {
                  e.currentTarget.src = `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/${pokemon.id}.png`
                }}
              />
            </div>
            <div className="space-y-5">
              <div className="grid grid-cols-2 gap-4">
                <InfoCard label="Taille" value={`${(pokemon.height / 10).toFixed(1)} m`} />
                <InfoCard label="Poids" value={`${(pokemon.weight / 10).toFixed(1)} kg`} />
                <InfoCard label="Capture" value={`${pokemon.captureRate}`} />
                <InfoCard label="XP base" value={`${pokemon.baseExperience}`} />
                {pokemon.habitat && <InfoCard label="Habitat" value={pokemon.habitat} />}
                {pokemon.generation && <InfoCard label="Génération" value={pokemon.generation} />}
              </div>
              <div>
                <h3 className="mb-2 font-bold text-gray-700">Talents</h3>
                <ul className="space-y-1 text-sm">
                  {pokemon.abilities.map((a) => (
                    <li key={a.name}>
                      • {a.name}
                      {a.isHidden && (
                        <span className="ml-1 text-xs text-purple-600">(caché)</span>
                      )}
                    </li>
                  ))}
                </ul>
              </div>
              <div>
                <h3 className="mb-2 font-bold text-gray-700">Description</h3>
                <p className="rounded-xl bg-gray-50 p-4 text-sm leading-relaxed text-gray-700">
                  {pokemon.description}
                </p>
              </div>
            </div>
          </div>
        )}
        {tab === 'stats' && <StatChart stats={pokemon.stats} />}
        {tab === 'combat' && (
          <div className="space-y-6 p-6">
            <div>
              <h3 className="mb-2 font-bold text-gray-700">Faiblesses</h3>
              <div className="flex flex-wrap gap-2">
                {pokemon.weaknesses.map((t) => (
                  <TypeBadge key={t} type={t} />
                ))}
              </div>
            </div>
            <div>
              <h3 className="mb-2 font-bold text-gray-700">Résistances</h3>
              <div className="flex flex-wrap gap-2">
                {pokemon.resistances.map((t) => (
                  <TypeBadge key={t} type={t} />
                ))}
              </div>
            </div>
          </div>
        )}
        {tab === 'forms' && (
          <div className="grid gap-4 p-6 sm:grid-cols-2 lg:grid-cols-3">
            {pokemon.forms.map((form) => (
              <div key={form.name} className="rounded-xl border border-gray-100 bg-gray-50 p-4 text-center">
                <p className="mb-2 text-sm font-bold">{form.name}</p>
                <img src={form.imageUrl} alt={form.name} className="mx-auto max-h-32 object-contain" />
              </div>
            ))}
          </div>
        )}
        {tab === 'evolutions' && (
          <EvolutionTree tree={pokemon.evolutionTree} currentId={pokemon.id} shiny={shiny} />
        )}
      </div>
    </div>
  )
}

function InfoCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-gray-100 bg-gray-50 p-4">
      <p className="text-xs font-medium uppercase tracking-wide text-gray-500">{label}</p>
      <p className="mt-1 text-lg font-bold">{value}</p>
    </div>
  )
}
