using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Pokedex.Core.Localization;
using Pokedex.Core.Models;
using Pokedex.Core.Search;
using Pokedex.Core.TypeChart;

namespace Pokedex.Data.Services;

public class PokeApiService : IPokemonService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string BaseUrl = "https://pokeapi.co/api/v2/";
    private const int MaxMainPokemonId = 1025;
    private const int CacheHours = 24;

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static readonly HashSet<string> ExcludedFormSuffixes =
    [
        "-mega", "-gmax", "-alola", "-galar", "-hisui", "-paldea",
        "-white", "-black", "-incarnate", "-therian", "-origin",
        "-altered", "-primal", "-eternal", "-form"
    ];

    public PokeApiService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _httpClient.BaseAddress ??= new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<Pokemon?> GetPokemonByIdAsync(int id) =>
        GetPokemonByNameAsync(id.ToString());

    public async Task<Pokemon?> SearchPokemonAsync(string query)
    {
        var q = PokemonSearchHelper.NormalizeQuery(query);
        if (string.IsNullOrWhiteSpace(q)) return null;

        if (PokemonSearchHelper.TryParseId(q, out var id))
            return await GetPokemonByIdAsync(id);

        var direct = await GetPokemonByNameAsync(q);
        if (direct != null) return direct;

        foreach (var variant in new[] { q.Replace(' ', '-'), q.Replace('-', ' '), q.Replace("é", "e").Replace("è", "e") })
        {
            if (variant == q) continue;
            var match = await GetPokemonByNameAsync(variant);
            if (match != null) return match;
        }

        return null;
    }

    private async Task<HttpResponseMessage> GetWithRetryAsync(string requestUri, int retries = 2)
    {
        Exception? last = null;
        for (var attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUri);
                if ((int)response.StatusCode >= 500 && attempt < retries)
                {
                    await Task.Delay(300 * (attempt + 1));
                    continue;
                }
                return response;
            }
            catch (Exception ex) when (attempt < retries)
            {
                last = ex;
                await Task.Delay(300 * (attempt + 1));
            }
        }
        throw last ?? new HttpRequestException($"Échec réseau : {requestUri}");
    }

    public async Task<Pokemon?> GetPokemonByNameAsync(string name)
    {
        var normalizedName = NormalizePokemonName(name);
        var cacheKey = $"pokemon_{normalizedName}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(CacheHours));
            return await FetchPokemonAsync(normalizedName);
        });
    }

    public async Task<int> GetTotalPokemonCountAsync()
    {
        return await _cache.GetOrCreateAsync("pokemon_count", async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(CacheHours));
            try
            {
                var response = await _httpClient.GetAsync("pokemon?limit=1&offset=0");
                if (!response.IsSuccessStatusCode) return MaxMainPokemonId;

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonDocument>(content);
                var totalCount = data?.RootElement.GetProperty("count").GetInt32() ?? MaxMainPokemonId;
                return Math.Min(totalCount, MaxMainPokemonId);
            }
            catch
            {
                return MaxMainPokemonId;
            }
        });
    }

    public async Task<List<BasePokemon>> GetAllPokemonAsync(int offset = 0, int limit = 20)
    {
        var cacheKey = $"pokemon_list_{offset}_{limit}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(CacheHours));
            return await FetchPokemonListAsync(offset, limit);
        }) ?? [];
    }

    public async Task<List<PokemonFormInfo>> GetPokemonFormsAsync(int pokemonId, string pokemonName)
    {
        var cacheKey = $"pokemon_forms_{pokemonId}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(CacheHours));
            return await BuildFormsAsync(pokemonId, pokemonName);
        }) ?? [];
    }

    private async Task<Pokemon?> FetchPokemonAsync(string normalizedName)
    {
        try
        {
            var response = await GetWithRetryAsync($"pokemon/{normalizedName}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var pokemonDetail = JsonSerializer.Deserialize<PokemonDetailResponse>(content, CamelCaseOptions);
            if (pokemonDetail == null) return null;

            var speciesResponse = await GetWithRetryAsync($"pokemon-species/{pokemonDetail.Id}");
            if (!speciesResponse.IsSuccessStatusCode) return null;

            var speciesContent = await speciesResponse.Content.ReadAsStringAsync();
            var species = JsonSerializer.Deserialize<PokemonSpeciesResponse>(speciesContent, SnakeCaseOptions);
            if (species == null) return null;

            EvolutionChainDetailResponse evoChain = new();
            if (!string.IsNullOrEmpty(species.Evolution_chain.Url))
            {
                var evoResponse = await _httpClient.GetAsync(species.Evolution_chain.Url);
                if (evoResponse.IsSuccessStatusCode)
                {
                    var evoContent = await evoResponse.Content.ReadAsStringAsync();
                    evoChain = JsonSerializer.Deserialize<EvolutionChainDetailResponse>(evoContent, CamelCaseOptions) ?? evoChain;
                }
            }

            var englishName = FormatName(pokemonDetail.Name);
            var frenchName = GetLocalizedName(species.Names, "fr") ?? englishName;
            var types = pokemonDetail.Types.Select(t => PokemonLabels.Type(t.Type.Name)).ToList();
            var abilities = await MapAbilitiesAsync(pokemonDetail.Abilities);

            return new Pokemon
            {
                Id = pokemonDetail.Id,
                Name = englishName,
                FrenchName = frenchName,
                Types = types,
                Height = pokemonDetail.Height,
                Weight = pokemonDetail.Weight,
                Stats = pokemonDetail.Stats.Select(s => new Stats
                {
                    Name = PokemonLabels.Stat(s.Stat.Name),
                    Value = s.Base_stat
                }).ToList(),
                Abilities = abilities,
                Description = ExtractDescription(species.Flavor_text_entries, "fr")
                              ?? ExtractDescription(species.Flavor_text_entries, "en")
                              ?? "Aucune description disponible.",
                Genus = GetGenus(species.Genera, "fr") ?? GetGenus(species.Genera, "en") ?? "",
                CaptureRate = species.Capture_rate,
                BaseExperience = species.Base_experience,
                IsLegendary = species.Is_legendary,
                IsMythical = species.Is_mythical,
                Habitat = FormatName(species.Habitat?.Name ?? ""),
                Generation = FormatName(species.Generation?.Name ?? ""),
                CryUrl = pokemonDetail.Cries?.Latest ?? "",
                Weaknesses = global::Pokedex.Core.TypeChart.TypeChart.GetWeaknesses(types).ToList(),
                Resistances = global::Pokedex.Core.TypeChart.TypeChart.GetResistances(types).ToList(),
                Evolutions = FlattenEvolutions(evoChain.Chain),
                EvolutionTree = await BuildEvolutionTreeAsync(evoChain.Chain)
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<BasePokemon>> FetchPokemonListAsync(int offset, int limit)
    {
        try
        {
            var response = await _httpClient.GetAsync($"pokemon?offset={offset}&limit={limit}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var listResponse = JsonSerializer.Deserialize<PokemonListResponse>(content, CamelCaseOptions);
            if (listResponse?.Results == null) return [];

            var results = new List<BasePokemon>();
            foreach (var item in listResponse.Results)
            {
                if (item.Url == null || item.Name == null) continue;
                if (IsAlternateForm(item.Name)) continue;

                var urlParts = item.Url.Split('/');
                if (!int.TryParse(urlParts[^2], out var id)) continue;
                if (id > MaxMainPokemonId) continue;

                var formattedName = FormatName(item.Name);
                results.Add(new BasePokemon
                {
                    Id = id,
                    Name = formattedName,
                    FrenchName = formattedName
                });
            }

            return results;
        }
        catch
        {
            return [];
        }
    }

    private async Task<List<Ability>> MapAbilitiesAsync(List<AbilityResponse> abilities)
    {
        var result = new List<Ability>();
        foreach (var ability in abilities.OrderBy(a => a.Slot).ThenBy(a => a.Is_hidden))
        {
            var englishName = FormatName(ability.Ability.Name);
            var frenchName = await GetAbilityFrenchNameAsync(ability.Ability.Name) ?? englishName;
            result.Add(new Ability
            {
                Name = englishName,
                FrenchName = frenchName + (ability.Is_hidden ? " (caché)" : ""),
                IsHidden = ability.Is_hidden
            });
        }
        return result;
    }

    private async Task<string?> GetAbilityFrenchNameAsync(string abilitySlug)
    {
        var cacheKey = $"ability_fr_{abilitySlug}";
        if (_cache.TryGetValue(cacheKey, out string? cached)) return cached;

        try
        {
            var response = await _httpClient.GetAsync($"ability/{abilitySlug}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var detail = JsonSerializer.Deserialize<AbilityDetailResponse>(content, SnakeCaseOptions);
            var french = GetLocalizedName(detail?.Names ?? [], "fr");
            if (french != null)
                _cache.Set(cacheKey, french, TimeSpan.FromHours(CacheHours));
            return french;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<PokemonFormInfo>> BuildFormsAsync(int pokemonId, string pokemonName)
    {
        var pokemon = await GetPokemonByNameAsync(pokemonName);
        var forms = new List<PokemonFormInfo>
        {
            new()
            {
                Name = pokemon?.DisplayName ?? pokemonName,
                ImageUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{pokemonId}.png",
                Types = pokemon?.Types ?? [],
                FormId = pokemonId
            }
        };

        if (PokemonFormRegistry.TryGetFormSuffixes(pokemonName, out var suffixes))
        {
            foreach (var suffix in suffixes)
            {
                var form = await TryResolveFormAsync(pokemonName, suffix);
                if (form != null) forms.Add(form);
            }
        }

        return forms;
    }

    private async Task<PokemonFormInfo?> TryResolveFormAsync(string pokemonName, string suffix)
    {
        var formSlug = suffix.Contains('-')
            ? $"{pokemonName.ToLower()}-{suffix}"
            : $"{pokemonName.ToLower()}-{suffix}";

        if (PokemonFormRegistry.TryGetMegaId(formSlug, out var megaId))
        {
            var basePokemon = await GetPokemonByNameAsync(pokemonName);
            return new PokemonFormInfo
            {
                Name = $"{pokemonName} ({PokemonFormRegistry.FormatSuffix(suffix)})",
                ImageUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{megaId}.png",
                AlternateImageUrls =
                [
                    $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{megaId}.png"
                ],
                Types = basePokemon?.Types ?? [],
                FormId = megaId
            };
        }

        var formPokemon = await GetPokemonByNameAsync(formSlug);
        if (formPokemon == null) return null;

        return new PokemonFormInfo
        {
            Name = $"{pokemonName} ({PokemonFormRegistry.FormatSuffix(suffix)})",
            ImageUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{formPokemon.Id}.png",
            AlternateImageUrls =
            [
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{formPokemon.Id}.png"
            ],
            Types = formPokemon.Types,
            FormId = formPokemon.Id
        };
    }

    private static bool IsAlternateForm(string slug)
    {
        if (slug.EndsWith("-form")) return true;
        return ExcludedFormSuffixes.Any(suffix => slug.Contains(suffix, StringComparison.Ordinal));
    }

    private static string NormalizePokemonName(string name)
    {
        var normalized = name.ToLower().Trim().Replace(' ', '-');

        if (normalized.Contains('('))
        {
            var start = normalized.IndexOf('(');
            var end = normalized.IndexOf(')');
            if (start < end)
                normalized = normalized.Remove(start, end - start + 1).Trim('-');
        }

        return PokemonFormRegistry.NormalizeSlug(normalized);
    }

    private static string FormatName(string name) =>
        string.Join(" ", name.Split('-').Select(part =>
            part.Length > 0 ? char.ToUpper(part[0]) + part[1..] : part));

    private static string? GetLocalizedName(IEnumerable<LocalizedName> names, string language) =>
        names.FirstOrDefault(n => n.Language?.Name?.Equals(language, StringComparison.OrdinalIgnoreCase) == true)?.Name;

    private static string? GetGenus(IEnumerable<GenusEntry> genera, string language) =>
        genera.FirstOrDefault(g => g.Language?.Name?.Equals(language, StringComparison.OrdinalIgnoreCase) == true)?.Genus;

    private static string? ExtractDescription(IEnumerable<FlavorTextEntry> entries, string language)
    {
        var entry = entries
            .Where(e => e.Language?.Name?.Equals(language, StringComparison.OrdinalIgnoreCase) == true)
            .Select(e => e.Flavor_text)
            .LastOrDefault(t => !string.IsNullOrWhiteSpace(t));

        if (entry == null) return null;

        return entry
            .Replace("\f", " ")
            .Replace("\u000c", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("POKéMON", "Pokémon")
            .Replace("POKeMON", "Pokémon")
            .Replace("  ", " ")
            .Trim();
    }

    private static List<EvolutionChain> FlattenEvolutions(ChainLink? root)
    {
        var result = new List<EvolutionChain>();
        if (root == null) return result;
        WalkEvolutions(root, result);
        return result;
    }

    private static void WalkEvolutions(ChainLink node, List<EvolutionChain> result)
    {
        foreach (var child in node.Evolves_to ?? [])
        {
            if (child.Species?.Name == null) continue;
            var detail = child.Evolution_details?.FirstOrDefault();
            result.Add(new EvolutionChain
            {
                Name = FormatName(child.Species.Name),
                Level = detail?.Min_level ?? 0,
                Trigger = FormatEvolutionTrigger(detail)
            });
            WalkEvolutions(child, result);
        }
    }

    private async Task<string?> GetFrenchNameForSpeciesAsync(int speciesId)
    {
        var cacheKey = $"species_fr_{speciesId}";
        if (_cache.TryGetValue(cacheKey, out string? cached)) return cached;

        try
        {
            var response = await _httpClient.GetAsync($"pokemon-species/{speciesId}");
            if (!response.IsSuccessStatusCode) return null;
            var content = await response.Content.ReadAsStringAsync();
            var species = JsonSerializer.Deserialize<PokemonSpeciesResponse>(content, SnakeCaseOptions);
            var french = GetLocalizedName(species?.Names ?? [], "fr");
            if (french != null)
                _cache.Set(cacheKey, french, TimeSpan.FromHours(CacheHours));
            return french;
        }
        catch
        {
            return null;
        }
    }

    private async Task<EvolutionNode?> BuildEvolutionTreeAsync(ChainLink? root)
    {
        if (root?.Species?.Name == null) return null;
        return await BuildNodeAsync(root);
    }

    private async Task<EvolutionNode> BuildNodeAsync(ChainLink link)
    {
        var slug = link.Species.Name;
        var id = await ResolvePokemonIdAsync(slug);
        var french = id > 0 ? await GetFrenchNameForSpeciesAsync(id) : null;

        var node = new EvolutionNode
        {
            Id = id,
            Name = FormatName(slug),
            FrenchName = french ?? FormatName(slug)
        };

        foreach (var child in link.Evolves_to ?? [])
        {
            if (child.Species?.Name == null) continue;
            var detail = child.Evolution_details?.FirstOrDefault();
            var childNode = await BuildNodeAsync(child);
            childNode.MinLevel = detail?.Min_level ?? 0;
            childNode.Trigger = FormatEvolutionTrigger(detail);
            node.Children.Add(childNode);
        }

        return node;
    }

    private async Task<int> ResolvePokemonIdAsync(string slug)
    {
        var cacheKey = $"id_{slug}";
        if (_cache.TryGetValue(cacheKey, out int id)) return id;

        try
        {
            var response = await _httpClient.GetAsync($"pokemon/{slug}");
            if (!response.IsSuccessStatusCode) return 0;
            var content = await response.Content.ReadAsStringAsync();
            var detail = JsonSerializer.Deserialize<PokemonDetailResponse>(content, CamelCaseOptions);
            id = detail?.Id ?? 0;
            if (id > 0)
                _cache.Set(cacheKey, id, TimeSpan.FromHours(CacheHours));
            return id;
        }
        catch
        {
            return 0;
        }
    }

    private static string FormatEvolutionTrigger(EvolutionDetail? detail)
    {
        if (detail == null) return "";
        if (detail.Min_level is > 0)
            return $"Niveau {detail.Min_level}";
        if (detail.Item?.Name != null)
            return $"Objet : {FormatName(detail.Item.Name)}";
        var trigger = detail.Trigger?.Name ?? "";
        return PokemonLabels.EvolutionTrigger(trigger);
    }
}
