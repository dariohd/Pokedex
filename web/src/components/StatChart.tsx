function barColor(value: number): string {
  if (value >= 130) return '#28a745'
  if (value >= 90) return '#8bc34a'
  if (value >= 60) return '#ffc107'
  return '#ff7043'
}

export function StatChart({ stats }: { stats: { name: string; value: number }[] }) {
  const total = stats.reduce((s, x) => s + x.value, 0)
  const max = 255

  return (
    <div className="space-y-6 p-6">
      <p className="text-center text-lg font-bold text-pokedex-red-dark">
        Total des stats : {total}
      </p>
      <div className="grid grid-cols-3 gap-4 sm:grid-cols-6">
        {stats.map((stat) => (
          <div key={stat.name} className="flex flex-col items-center gap-2">
            <div className="relative flex h-40 w-full max-w-[52px] items-end rounded-lg bg-gray-200">
              <div
                className="w-full rounded-lg transition-all duration-500"
                style={{
                  height: `${(stat.value / max) * 100}%`,
                  backgroundColor: barColor(stat.value),
                  minHeight: stat.value > 0 ? '4px' : 0,
                }}
              />
            </div>
            <span className="text-lg font-bold">{stat.value}</span>
            <span className="text-center text-xs text-gray-600">{stat.name}</span>
          </div>
        ))}
      </div>
    </div>
  )
}

export function StatBar({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex items-center gap-3 text-sm">
      <span className="w-20 shrink-0 font-medium text-gray-600">{label}</span>
      <div className="h-2 flex-1 overflow-hidden rounded-full bg-gray-200">
        <div
          className="h-full rounded-full"
          style={{ width: `${(value / 255) * 100}%`, backgroundColor: barColor(value) }}
        />
      </div>
      <span className="w-8 text-right font-bold">{value}</span>
    </div>
  )
}
