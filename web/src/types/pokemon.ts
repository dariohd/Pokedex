export interface NamedResource {
  name: string
  url: string
}

export interface LocalizedName {
  name: string
  language: NamedResource
}

export interface PokemonListItem {
  name: string
  url: string
}

export interface PokemonListResponse {
  count: number
  next: string | null
  results: PokemonListItem[]
}

export interface PokemonApi {
  id: number
  name: string
  height: number
  weight: number
  base_experience: number
  types: { type: NamedResource }[]
  stats: { base_stat: number; stat: NamedResource }[]
  abilities: {
    ability: NamedResource
    is_hidden: boolean
    slot: number
  }[]
  sprites: {
    front_default: string | null
    front_shiny: string | null
    other: {
      'official-artwork': {
        front_default: string | null
        front_shiny: string | null
      }
    }
  }
  cries?: { latest?: string }
}

export interface PokemonSpecies {
  names: LocalizedName[]
  genera: { genus: string; language: NamedResource }[]
  flavor_text_entries: {
    flavor_text: string
    language: NamedResource
  }[]
  evolution_chain: { url: string }
  capture_rate: number
  is_legendary: boolean
  is_mythical: boolean
  habitat?: { name: string } | null
  generation?: { name: string } | null
}

export interface EvolutionDetail {
  min_level: number | null
  trigger: NamedResource | null
  item: NamedResource | null
}

export interface EvolutionChainLink {
  species: NamedResource
  evolution_details: EvolutionDetail[]
  evolves_to: EvolutionChainLink[]
}

export interface EvolutionChainResponse {
  chain: EvolutionChainLink
}

export interface AbilityApi {
  names: LocalizedName[]
}

export interface PokemonSummary {
  id: number
  name: string
  displayName: string
}

export interface PokemonDetail {
  id: number
  name: string
  displayName: string
  genus: string
  description: string
  types: string[]
  height: number
  weight: number
  captureRate: number
  baseExperience: number
  isLegendary: boolean
  isMythical: boolean
  habitat: string
  generation: string
  cryUrl: string
  weaknesses: string[]
  resistances: string[]
  stats: { name: string; value: number }[]
  abilities: { name: string; isHidden: boolean }[]
  evolutionTree: EvolutionNode | null
  forms: PokemonForm[]
  spriteUrl: string
  shinySpriteUrl: string
}

export interface PokemonForm {
  name: string
  imageUrl: string
}

export interface EvolutionNode {
  name: string
  displayName: string
  id: number
  trigger: string
  children: EvolutionNode[]
}
