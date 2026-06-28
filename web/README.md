# Pokédex Web

Application Pokédex moderne connectée à [PokéAPI](https://pokeapi.co/).

## Fonctionnalités

- Liste paginée du Pokédex national (~1025 espèces)
- Noms et descriptions en français
- Fiche détaillée : types, talents, stats, BST, capture, XP
- Arbre d'évolutions avec branches
- Mode chromatique
- Recherche par nom
- Cache mémoire côté client

## Prérequis

- Node.js 18+

## Lancement

```bash
npm install
npm run dev
```

Ou double-clic sur `RunPokedex.bat` (ouvre le navigateur sur http://localhost:5173).

## Stack

- React 19 + TypeScript
- Vite
- Tailwind CSS 4
- PokéAPI (REST)

## Ancien projet C#

Le projet WinForms `PokedexCSharp` reste dans le dossier parent. Cette version web le remplace pour l'interface graphique.
