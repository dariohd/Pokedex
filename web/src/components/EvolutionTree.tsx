import type { EvolutionNode } from '../types/pokemon'

function EvoCard({
  node,
  currentId,
  shiny,
}: {
  node: EvolutionNode
  currentId: number
  shiny: boolean
}) {
  const isCurrent = node.id === currentId
  const sprite = shiny
    ? `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/shiny/${node.id}.png`
    : `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/${node.id}.png`

  return (
    <div
      className={`flex w-36 shrink-0 flex-col items-center rounded-xl border p-3 ${
        isCurrent ? 'border-pokedex-red bg-red-50 shadow-md' : 'border-gray-200 bg-white'
      }`}
    >
      <img
        src={sprite}
        alt={node.displayName}
        className="h-24 w-24 object-contain"
        loading="lazy"
        onError={(e) => {
          e.currentTarget.src = `https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/${node.id}.png`
        }}
      />
      <p className="mt-1 text-center text-sm font-bold">{node.displayName}</p>
      {node.trigger && (
        <p className="mt-1 text-center text-xs text-gray-500">{node.trigger}</p>
      )}
    </div>
  )
}

function EvoArrow() {
  return <span className="shrink-0 text-2xl font-bold text-pokedex-red">→</span>
}

function EvoNode({
  node,
  currentId,
  shiny,
  isRoot = false,
}: {
  node: EvolutionNode
  currentId: number
  shiny: boolean
  isRoot?: boolean
}) {
  if (node.children.length === 0) {
    return (
      <div className="flex items-center gap-2">
        {!isRoot && <EvoArrow />}
        <EvoCard node={node} currentId={currentId} shiny={shiny} />
      </div>
    )
  }

  if (node.children.length === 1) {
    return (
      <div className="flex items-center gap-2">
        {!isRoot && <EvoArrow />}
        <EvoCard node={node} currentId={currentId} shiny={shiny} />
        <EvoArrow />
        <EvoNode node={node.children[0]} currentId={currentId} shiny={shiny} />
      </div>
    )
  }

  return (
    <div className="flex items-start gap-2">
      {!isRoot && <EvoArrow />}
      <EvoCard node={node} currentId={currentId} shiny={shiny} />
      <EvoArrow />
      <div className="flex flex-col gap-4">
        {node.children.map((child) => (
          <EvoNode key={child.id} node={child} currentId={currentId} shiny={shiny} />
        ))}
      </div>
    </div>
  )
}

export function EvolutionTree({
  tree,
  currentId,
  shiny,
}: {
  tree: EvolutionNode | null
  currentId: number
  shiny: boolean
}) {
  if (!tree) {
    return (
      <p className="p-8 text-center text-gray-500">
        Aucune donnée d&apos;évolution disponible.
      </p>
    )
  }

  if (tree.children.length === 0) {
    return (
      <p className="p-8 text-center text-gray-500">
        Ce Pokémon est au sommet de sa lignée ou n&apos;a pas d&apos;évolution.
      </p>
    )
  }

  return (
    <div className="overflow-x-auto p-6">
      <EvoNode node={tree} currentId={currentId} shiny={shiny} isRoot />
    </div>
  )
}
