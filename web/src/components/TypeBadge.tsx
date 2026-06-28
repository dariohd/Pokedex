import { TYPE_COLORS } from '../lib/labels'

export function TypeBadge({ type }: { type: string }) {
  const bg = TYPE_COLORS[type] ?? '#6c757d'
  return (
    <span
      className="inline-block rounded-full px-3 py-1 text-xs font-bold text-white shadow-sm"
      style={{ backgroundColor: bg }}
    >
      {type}
    </span>
  )
}
