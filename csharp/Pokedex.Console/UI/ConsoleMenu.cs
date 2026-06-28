using Microsoft.Extensions.DependencyInjection;
using Pokedex.Core.Models;
using Pokedex.Data.Services;

namespace Pokedex.Console.UI;

public class ConsoleMenu
{
    private readonly IPokemonService _pokemonService;
    private int _currentPage;
    private const int PageSize = 20;

    public ConsoleMenu(IPokemonService pokemonService)
    {
        _pokemonService = pokemonService;
    }

    public async Task ShowMainMenu()
    {
        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("=== Pokédex ===");
            System.Console.WriteLine("1. Rechercher un Pokémon");
            System.Console.WriteLine("2. Lister les Pokémon");
            System.Console.WriteLine("3. Ouvrir l'interface graphique");
            System.Console.WriteLine("4. Quitter");
            System.Console.Write("\nChoix : ");

            switch (System.Console.ReadLine())
            {
                case "1":
                    await SearchPokemon();
                    break;
                case "2":
                    await ListPokemon();
                    break;
                case "3":
                    LaunchGui();
                    break;
                case "4":
                    return;
            }
        }
    }

    private void LaunchGui()
    {
        var exeDir = AppContext.BaseDirectory;
        var wpfExe = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "..", "Pokedex.Wpf", "bin", "Debug", "net10.0-windows", "Pokedex.Wpf.exe"));
        if (File.Exists(wpfExe))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(wpfExe) { UseShellExecute = true });
            return;
        }
        System.Console.WriteLine("Interface WPF : lancez RunPokedex.bat ou dotnet run --project Pokedex.Wpf");
        System.Console.WriteLine("Appuyez sur une touche...");
        System.Console.ReadKey();
    }

    private async Task SearchPokemon()
    {
        System.Console.Clear();
        System.Console.Write("Nom du Pokémon : ");
        var name = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) return;

        var pokemon = await _pokemonService.GetPokemonByNameAsync(name);
        if (pokemon == null)
            System.Console.WriteLine("Pokémon introuvable.");
        else
            DisplayPokemonDetails(pokemon);

        System.Console.WriteLine("\nAppuyez sur une touche pour continuer...");
        System.Console.ReadKey();
    }

    private async Task ListPokemon()
    {
        while (true)
        {
            System.Console.Clear();
            var pokemons = await _pokemonService.GetAllPokemonAsync(_currentPage * PageSize, PageSize);
            var totalCount = await _pokemonService.GetTotalPokemonCountAsync();
            var totalPages = Math.Max(1, (totalCount + PageSize - 1) / PageSize);

            foreach (var pokemon in pokemons)
                System.Console.WriteLine($"{pokemon.Id,4} - {pokemon.Name}");

            System.Console.WriteLine($"\nPage {_currentPage + 1}/{totalPages}");
            System.Console.WriteLine("\n[P]récédent  [S]uivant  [numéro] détails  [R]etour");
            var input = System.Console.ReadLine()?.ToUpperInvariant();

            if (input == "R") break;
            if (input == "P" && _currentPage > 0) _currentPage--;
            if (input == "S" && _currentPage + 1 < totalPages) _currentPage++;
            if (int.TryParse(input, out var number))
            {
                var selected = pokemons.FirstOrDefault(p => p.Id == number);
                if (selected != null)
                {
                    var detailed = await _pokemonService.GetPokemonByNameAsync(selected.Name);
                    if (detailed != null)
                    {
                        DisplayPokemonDetails(detailed);
                        System.Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                        System.Console.ReadKey();
                    }
                }
            }
        }
    }

    private static void DisplayPokemonDetails(Pokemon pokemon)
    {
        var bst = pokemon.Stats.Sum(s => s.Value);
        System.Console.WriteLine($"\n=== {pokemon.DisplayName.ToUpper()} ===");
        System.Console.WriteLine($"N° {pokemon.Id}  ({pokemon.Name})");
        if (!string.IsNullOrEmpty(pokemon.Genus))
            System.Console.WriteLine(pokemon.Genus);
        System.Console.WriteLine($"Types : {string.Join(", ", pokemon.Types)}");
        System.Console.WriteLine($"Taille : {pokemon.Height / 10.0:F1} m");
        System.Console.WriteLine($"Poids : {pokemon.Weight / 10.0:F1} kg");
        System.Console.WriteLine($"BST : {bst}");

        System.Console.WriteLine("\nStats :");
        foreach (var stat in pokemon.Stats)
        {
            var color = stat.Value switch
            {
                >= 130 => ConsoleColor.Green,
                >= 90 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red
            };
            var prev = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine($"  {stat.Name,-12} {stat.Value,3}");
            System.Console.ForegroundColor = prev;
        }

        System.Console.WriteLine("\nTalents :");
        foreach (var ability in pokemon.Abilities)
        {
            var hidden = ability.IsHidden ? " (caché)" : "";
            System.Console.WriteLine($"  - {ability.DisplayName}{hidden}");
        }

        System.Console.WriteLine($"\nPokédex :\n{pokemon.Description}");

        if (pokemon.Evolutions.Count > 0)
        {
            System.Console.WriteLine("\nÉvolutions :");
            foreach (var evo in pokemon.Evolutions)
            {
                var trigger = !string.IsNullOrEmpty(evo.Trigger) ? evo.Trigger : $"Niveau {evo.Level}";
                System.Console.WriteLine($"  → {evo.Name} ({trigger})");
            }
        }
    }
}
