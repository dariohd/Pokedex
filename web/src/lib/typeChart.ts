const weakTo: Record<string, string[]> = {
  Normal: ['Combat'],
  Feu: ['Eau', 'Sol', 'Roche'],
  Eau: ['Électrik', 'Plante'],
  Électrik: ['Sol'],
  Plante: ['Feu', 'Glace', 'Vol', 'Poison', 'Insecte'],
  Glace: ['Feu', 'Combat', 'Roche', 'Acier'],
  Combat: ['Vol', 'Psy', 'Fée'],
  Poison: ['Sol', 'Psy'],
  Sol: ['Eau', 'Glace', 'Plante'],
  Vol: ['Électrik', 'Glace', 'Roche'],
  Psy: ['Insecte', 'Spectre', 'Ténèbres'],
  Insecte: ['Feu', 'Vol', 'Roche'],
  Roche: ['Eau', 'Plante', 'Combat', 'Sol', 'Acier'],
  Spectre: ['Spectre', 'Ténèbres'],
  Dragon: ['Glace', 'Dragon', 'Fée'],
  Ténèbres: ['Combat', 'Insecte', 'Fée'],
  Acier: ['Feu', 'Combat', 'Sol'],
  Fée: ['Poison', 'Acier'],
}

const resistMap: Record<string, string[]> = {
  Feu: ['Feu', 'Plante', 'Glace', 'Insecte', 'Acier', 'Fée'],
  Eau: ['Feu', 'Eau', 'Glace', 'Acier'],
  Plante: ['Eau', 'Électrik', 'Plante', 'Sol'],
  Électrik: ['Électrik', 'Vol', 'Acier'],
  Glace: ['Glace'],
  Combat: ['Roche', 'Ténèbres', 'Insecte'],
  Poison: ['Combat', 'Poison', 'Plante', 'Fée', 'Insecte'],
  Sol: ['Poison', 'Roche'],
  Vol: ['Combat', 'Insecte', 'Plante', 'Sol'],
  Psy: ['Combat', 'Psy'],
  Insecte: ['Combat', 'Plante', 'Sol', 'Insecte'],
  Roche: ['Normal', 'Feu', 'Poison', 'Vol'],
  Spectre: ['Poison', 'Insecte'],
  Dragon: ['Feu', 'Eau', 'Électrik', 'Plante'],
  Ténèbres: ['Spectre', 'Ténèbres'],
  Acier: ['Normal', 'Plante', 'Glace', 'Vol', 'Psy', 'Insecte', 'Roche', 'Dragon', 'Acier', 'Fée'],
  Fée: ['Combat', 'Insecte', 'Ténèbres'],
}

function isImmune(attacking: string, defending: string): boolean {
  return (
    (attacking === 'Normal' && defending === 'Spectre') ||
    (attacking === 'Combat' && defending === 'Spectre') ||
    (attacking === 'Électrik' && defending === 'Sol') ||
    (attacking === 'Psy' && defending === 'Ténèbres') ||
    (attacking === 'Dragon' && defending === 'Fée')
  )
}

function isResistant(defending: string, attacking: string): boolean {
  return resistMap[defending]?.includes(attacking) ?? false
}

function getMultiplier(attacking: string, defendingTypes: string[]): number {
  let multiplier = 1
  for (const def of defendingTypes) {
    if (weakTo[def]?.includes(attacking)) multiplier *= 2
    else if (isImmune(attacking, def)) return 0
    else if (isResistant(def, attacking)) multiplier *= 0.5
  }
  return multiplier
}

export function getWeaknesses(types: string[]): string[] {
  const defending = [...new Set(types)]
  if (defending.length === 0) return []

  return Object.keys(weakTo)
    .filter((attacking) => getMultiplier(attacking, defending) >= 2)
    .sort()
}

export function getResistances(types: string[]): string[] {
  const defending = [...new Set(types)]
  if (defending.length === 0) return []

  return Object.keys(weakTo)
    .filter((attacking) => {
      const m = getMultiplier(attacking, defending)
      return m > 0 && m <= 0.5
    })
    .sort()
}
