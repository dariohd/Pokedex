# Pokédex

Deux interfaces pour explorer le Pokédex national (1025 espèces) via [PokéAPI](https://pokeapi.co/) :

**Demo web :** [pokedex-dariohprojects.vercel.app](https://pokedex-dariohprojects.vercel.app)  
(alias production : [pokedex-phi-seven-21.vercel.app](https://pokedex-phi-seven-21.vercel.app))

| Dossier | Stack | Lancement |
|---------|-------|-----------|
| `csharp/` | .NET 10, WPF (MVVM), console | `csharp\RunPokedex.bat` |
| `web/` | React 19, Vite, Tailwind 4 | `web\RunPokedex.bat` |

## Fonctionnalités

- Liste paginée, filtre et recherche (nom, ID `#25`)
- Noms et descriptions en français
- Stats, talents, évolutions en arbre
- Faiblesses et résistances (table des types)
- Formes alternatives (Méga, Gmax, régional…)
- Sprites chromatiques, cri audio
- Favoris et historique de recherche (persistés localement)
- Mode comparaison BST

## Prérequis

- **C#** : [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **Web** : Node.js 20+

## Démarrage rapide

```bat
RunPokedex.bat
```

Choisir `1` pour l’application WPF ou `2` pour la version web (http://localhost:5190).

### C# seul

```bash
cd csharp
dotnet build Pokedex.sln
dotnet run --project Pokedex.Wpf
dotnet test Pokedex.Tests
```

### Web seul

```bash
cd web
npm install
npm run dev
npm run test
npm run build
```

## Structure

```
Pokedex/
├── csharp/
│   ├── Pokedex.Core/      # modèles, type chart, recherche
│   ├── Pokedex.Data/      # client PokéAPI
│   ├── Pokedex.Wpf/       # interface graphique
│   ├── Pokedex.Console/   # CLI
│   └── Pokedex.Tests/
└── web/
    └── src/
```

## Licence

Données Pokémon © Nintendo / Game Freak / Creatures. Ce projet est un outil fan non commercial.
