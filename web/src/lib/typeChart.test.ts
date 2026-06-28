import { describe, expect, it } from 'vitest'
import { getResistances, getWeaknesses } from './typeChart'
import { tryParseId } from './search'

describe('typeChart', () => {
  it('returns ground weakness for electric', () => {
    expect(getWeaknesses(['Électrik'])).toContain('Sol')
  })

  it('returns water weakness for fire', () => {
    expect(getWeaknesses(['Feu'])).toContain('Eau')
  })

  it('combines dual types', () => {
    expect(getWeaknesses(['Feu', 'Glace'])).toContain('Combat')
  })

  it('lists steel resistances', () => {
    expect(getResistances(['Acier']).length).toBeGreaterThan(5)
  })
})

describe('search', () => {
  it('parses hash id', () => {
    expect(tryParseId('#25')).toBe(25)
  })
})
