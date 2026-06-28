import {
  formatName,
  formatTrigger,
  statLabel,
  typeLabel,
} from '../lib/labels'
import { getCached, setCache } from '../lib/cache'
import { getWeaknesses, getResistances } from '../lib/typeChart'
import { tryParseId } from '../lib/search'
import type {
  AbilityApi,
  EvolutionChainLink,
  EvolutionChainResponse,
  EvolutionNode,
  PokemonApi,
  PokemonDetail,
  PokemonListResponse,
  PokemonSpecies,
  PokemonSummary,
} from '../types/pokemon'

const BASE = 'https://pokeapi.co/api/v2'
export const MAX_DEX_ID = 1025
const PAGE_SIZE = 60

const FORM_SUFFIXES = [
  '-mega', '-gmax', '-alola', '-galar', '-hisui', '-paldea',
  '-white', '-black', '-origin', '-altered', '-primal', '-form',
]

async function fetchJson<T>(url: string): Promise<T> {
  const cached = getCached<T>(url)
  if (cached) return cached

  const res = await fetch(url)
  if (!res.ok) throw new Error(`API ${res.status}: ${url}`)
  const data = (await res.json()) as T
  setCache(url, data)
  return data
}

function localized(
  names: { name: string; language: { name: string } }[],
  lang: string,
): string | undefined {
  return names.find((n) => n.language.name === lang)?.name
}

function cleanFlavor(text: string): string {
  return text
    .replace(/\f/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
}

function isMainSpecies(slug: string, id: number): boolean {
  if (id > MAX_DEX_ID) return false
  return !FORM_SUFFIXES.some((s) => slug.includes(s))
}

function extractId(url: string): number {
  const parts = url.split('/').filter(Boolean)
  return parseInt(parts[parts.length - 1], 10)
}

export async function getTotalCount(): Promise<number> {
  const data = await fetchJson<PokemonListResponse>(`${BASE}/pokemon?limit=1`)
  return Math.min(data.count, MAX_DEX_ID)
}

export async function getPokemonPage(
  page: number,
): Promise<{ items: PokemonSummary[]; totalPages: number; total: number }> {
  const total = await getTotalCount()
  const totalPages = Math.ceil(total / PAGE_SIZE)
  const offset = page * PAGE_SIZE

  const data = await fetchJson<PokemonListResponse>(
    `${BASE}/pokemon?offset=${offset}&limit=${PAGE_SIZE}`,
  )

  const items: PokemonSummary[] = []
  for (const item of data.results) {
    const id = extractId(item.url)
    if (!isMainSpecies(item.name, id)) continue
    items.push({
      id,
      name: formatName(item.name),
      displayName: formatName(item.name),
    })
  }

  return { items, totalPages, total }
}

async function getFrenchAbilityName(slug: string): Promise<string> {
  try {
    const data = await fetchJson<AbilityApi>(`${BASE}/ability/${slug}`)
    return localized(data.names, 'fr') ?? formatName(slug)
  } catch {
    return formatName(slug)
  }
}

async function getFrenchSpeciesName(slug: string): Promise<string> {
  const cacheKey = `fr:${slug}`
  const cached = getCached<string>(cacheKey)
  if (cached) return cached
  try {
    const pokemon = await fetchJson<PokemonApi>(`${BASE}/pokemon/${slug}`)
    const species = await fetchJson<PokemonSpecies>(`${BASE}/pokemon-species/${pokemon.id}`)
    const fr = localized(species.names, 'fr') ?? formatName(slug)
    setCache(cacheKey, fr)
    return fr
  } catch {
    return formatName(slug)
  }
}

async function buildEvolutionNode(
  link: EvolutionChainLink,
  idMap: Map<string, number>,
): Promise<EvolutionNode> {
  const slug = link.species.name
  const id = idMap.get(slug) ?? (await fetchJson<PokemonApi>(`${BASE}/pokemon/${slug}`)).id
  idMap.set(slug, id)
  const displayName = await getFrenchSpeciesName(slug)

  const children = await Promise.all(
    link.evolves_to.map(async (child) => {
      const detail = child.evolution_details[0]
      const node = await buildEvolutionNode(child, idMap)
      node.trigger = detail ? formatTrigger(detail) : ''
      return node
    }),
  )

  return { name: formatName(slug), displayName, id, trigger: '', children }
}

export async function getPokemonDetail(slug: string): Promise<PokemonDetail> {
  const normalized = slug.toLowerCase().trim().replace(/\s+/g, '-')
  const cacheKey = `detail:${normalized}`
  const cached = getCached<PokemonDetail>(cacheKey)
  if (cached) return cached

  const pokemon = await fetchJson<PokemonApi>(`${BASE}/pokemon/${normalized}`)
  const species = await fetchJson<PokemonSpecies>(
    `${BASE}/pokemon-species/${pokemon.id}`,
  )

  const frName = localized(species.names, 'fr') ?? formatName(pokemon.name)
  const genus =
    species.genera.find((g) => g.language.name === 'fr')?.genus ??
    species.genera.find((g) => g.language.name === 'en')?.genus ??
    ''
  const description =
    cleanFlavor(
      species.flavor_text_entries.filter((e) => e.language.name === 'fr').at(-1)
        ?.flavor_text ?? '',
    ) ||
    cleanFlavor(
      species.flavor_text_entries.filter((e) => e.language.name === 'en').at(-1)
        ?.flavor_text ?? '',
    ) ||
    'Aucune description disponible.'

  const abilities = await Promise.all(
    [...pokemon.abilities]
      .sort((a, b) => a.slot - b.slot || Number(a.is_hidden) - Number(b.is_hidden))
      .map(async (a) => ({
        name: await getFrenchAbilityName(a.ability.name),
        isHidden: a.is_hidden,
      })),
  )

  let evolutionTree: EvolutionNode | null = null
  if (species.evolution_chain?.url) {
    const chain = await fetchJson<EvolutionChainResponse>(species.evolution_chain.url)
    evolutionTree = await buildEvolutionNode(chain.chain, new Map())
  }

  const types = pokemon.types.map((t) => typeLabel(t.type.name))
  const art = pokemon.sprites.other['official-artwork']
  const detail: PokemonDetail = {
    id: pokemon.id,
    name: formatName(pokemon.name),
    displayName: frName,
    genus,
    description,
    types,
    height: pokemon.height,
    weight: pokemon.weight,
    captureRate: species.capture_rate,
    baseExperience: pokemon.base_experience,
    isLegendary: species.is_legendary,
    isMythical: species.is_mythical,
    habitat: species.habitat?.name ? formatName(species.habitat.name) : '',
    generation: species.generation?.name ? formatName(species.generation.name) : '',
    cryUrl: pokemon.cries?.latest ?? '',
    weaknesses: getWeaknesses(types),
    resistances: getResistances(types),
    stats: pokemon.stats.map((s) => ({
      name: statLabel(s.stat.name),
      value: s.base_stat,
    })),
    abilities,
    evolutionTree,
    forms: await getPokemonForms(pokemon.id, pokemon.name),
    spriteUrl:
      art.front_default ??
      pokemon.sprites.front_default ??
      `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/${pokemon.id}.png`,
    shinySpriteUrl:
      art.front_shiny ??
      pokemon.sprites.front_shiny ??
      `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/shiny/${pokemon.id}.png`,
  }

  setCache(cacheKey, detail)
  return detail
}

export async function searchPokemon(query: string): Promise<PokemonDetail | null> {
  const normalized = tryParseId(query)
  if (normalized) {
    try {
      return await getPokemonDetail(String(normalized))
    } catch {
      return null
    }
  }

  const slug = query.trim().toLowerCase().replace(/\s+/g, '-')
  try {
    return await getPokemonDetail(slug)
  } catch {
    // fuzzy fallback via species list would be heavy; try common FR aliases via direct slug
    return null
  }
}

async function getPokemonForms(
  pokemonId: number,
  baseName: string,
): Promise<{ name: string; imageUrl: string }[]> {
  const forms: { name: string; imageUrl: string }[] = []
  const baseArt = `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/${pokemonId}.png`

  forms.push({ name: 'Forme standard', imageUrl: baseArt })

  const variants = [
    `${baseName}-mega-x`,
    `${baseName}-mega-y`,
    `${baseName}-gmax`,
    `${baseName}-alola`,
    `${baseName}-galar`,
  ]

  for (const variant of variants) {
    try {
      const v = await fetchJson<PokemonApi>(`${BASE}/pokemon/${variant}`)
      forms.push({
        name: formatName(variant),
        imageUrl:
          v.sprites.other['official-artwork'].front_default ??
          `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/${v.id}.png`,
      })
    } catch {
      // variant absent
    }
  }

  return forms
}

export async function enrichSummaries(
  items: PokemonSummary[],
): Promise<PokemonSummary[]> {
  const enriched = await Promise.all(
    items.map(async (item) => {
      try {
        const species = await fetchJson<PokemonSpecies>(
          `${BASE}/pokemon-species/${item.id}`,
        )
        const fr = localized(species.names, 'fr')
        return fr ? { ...item, displayName: fr } : item
      } catch {
        return item
      }
    }),
  )
  return enriched
}
