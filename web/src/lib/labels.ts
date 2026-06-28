const TYPE_FR: Record<string, string> = {
  normal: 'Normal',
  fire: 'Feu',
  water: 'Eau',
  electric: 'Électrik',
  grass: 'Plante',
  ice: 'Glace',
  fighting: 'Combat',
  poison: 'Poison',
  ground: 'Sol',
  flying: 'Vol',
  psychic: 'Psy',
  bug: 'Insecte',
  rock: 'Roche',
  ghost: 'Spectre',
  dragon: 'Dragon',
  dark: 'Ténèbres',
  steel: 'Acier',
  fairy: 'Fée',
}

const STAT_FR: Record<string, string> = {
  hp: 'PV',
  attack: 'Attaque',
  defense: 'Défense',
  'special-attack': 'Att. Spé.',
  'special-defense': 'Déf. Spé.',
  speed: 'Vitesse',
}

export function typeLabel(type: string): string {
  return TYPE_FR[type.toLowerCase()] ?? type
}

export function statLabel(stat: string): string {
  return STAT_FR[stat.toLowerCase()] ?? stat
}

export function formatName(slug: string): string {
  return slug
    .split('-')
    .map((p) => p.charAt(0).toUpperCase() + p.slice(1))
    .join(' ')
}

export function formatTrigger(detail: {
  min_level: number | null
  trigger: { name: string } | null
  item: { name: string } | null
}): string {
  if (detail.min_level) return `Niveau ${detail.min_level}`
  if (detail.item?.name) return `Objet : ${formatName(detail.item.name)}`
  const t = detail.trigger?.name ?? ''
  const map: Record<string, string> = {
    'level-up': 'Montée de niveau',
    trade: 'Échange',
    'use-item': 'Objet',
  }
  return map[t] ?? (t ? formatName(t) : '')
}

export const TYPE_COLORS: Record<string, string> = {
  Normal: '#a8a878',
  Feu: '#f08030',
  Eau: '#6890f0',
  Électrik: '#f8d030',
  Plante: '#78c850',
  Glace: '#98d8d8',
  Combat: '#c03028',
  Poison: '#a040a0',
  Sol: '#e0c068',
  Vol: '#a890f0',
  Psy: '#f85888',
  Insecte: '#a8b820',
  Roche: '#b8a038',
  Spectre: '#705898',
  Dragon: '#7038f8',
  Ténèbres: '#705848',
  Acier: '#b8b8d0',
  Fée: '#ee99ac',
}
