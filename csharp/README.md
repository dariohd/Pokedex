# Pokédex C# — WPF

Application Pokédex native Windows connectée à [PokéAPI](https://pokeapi.co/).

## Projets

| Projet | Rôle |
|--------|------|
| `Pokedex.Wpf` | **Interface graphique** (WPF + MVVM) |
| `Pokedex.Core` | Modèles et libellés FR |
| `Pokedex.Data` | Service API, cache, formes |
| `Pokedex.Console` | Menu console (optionnel) |
| `Pokedex.Tests` | Tests unitaires |

Version web séparée : `../PokedexWeb`

## Fonctionnalités

- Liste paginée (1025 espèces principales)
- Noms et descriptions en français (API espèces)
- Talents traduits, types FR, stats + BST
- Arbre d'évolutions avec branches
- Mode chromatique, recherche, filtre liste
- Cache mémoire 24 h

## Prérequis

- Windows 10/11
- .NET 10 SDK (ou runtime Windows Desktop)

## Lancement

```bash
# Interface graphique WPF
dotnet run --project Pokedex.Wpf

# Ou double-clic
RunPokedex.bat
```

```bash
dotnet test
```

## Stack

- .NET 10 + WPF
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Hosting + HttpClient typé
- PokéAPI REST
