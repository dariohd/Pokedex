namespace Pokedex.Core.Models;

public class PokemonListResponse
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<PokemonListItem> Results { get; set; } = new();
}

public class PokemonListItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class PokemonDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PokemonTypeResponse> Types { get; set; } = new();
    public SpeciesResponse Species { get; set; } = new();
    public int Height { get; set; }
    public int Weight { get; set; }
    public List<StatResponse> Stats { get; set; } = new();
    public List<AbilityResponse> Abilities { get; set; } = new();
    public PokemonSprites Sprites { get; set; } = new();
    public PokemonCries Cries { get; set; } = new();
}

public class PokemonCries
{
    public string Latest { get; set; } = string.Empty;
}

public class PokemonSprites
{
    public string Front_default { get; set; } = string.Empty;
    public string Front_shiny { get; set; } = string.Empty;
    public OfficialArtworkSprites Other { get; set; } = new();
}

public class OfficialArtworkSprites
{
    public OfficialArtwork Official_artwork { get; set; } = new();
}

public class OfficialArtwork
{
    public string Front_default { get; set; } = string.Empty;
    public string Front_shiny { get; set; } = string.Empty;
}

public class PokemonTypeResponse
{
    public TypeInfo Type { get; set; } = new();
}

public class TypeInfo
{
    public string Name { get; set; } = string.Empty;
}

public class SpeciesResponse
{
    public string Url { get; set; } = string.Empty;
}

public class PokemonSpeciesResponse
{
    public List<LocalizedName> Names { get; set; } = new();
    public List<FlavorTextEntry> Flavor_text_entries { get; set; } = new();
    public List<GenusEntry> Genera { get; set; } = new();
    public EvolutionChainResponse Evolution_chain { get; set; } = new();
    public int Capture_rate { get; set; }
    public int Base_experience { get; set; }
    public bool Is_legendary { get; set; }
    public bool Is_mythical { get; set; }
    public NamedResource Habitat { get; set; } = new();
    public NamedResource Generation { get; set; } = new();
}

public class LocalizedName
{
    public string Name { get; set; } = string.Empty;
    public NamedResource Language { get; set; } = new();
}

public class GenusEntry
{
    public string Genus { get; set; } = string.Empty;
    public NamedResource Language { get; set; } = new();
}

public class FlavorTextEntry
{
    public string Flavor_text { get; set; } = string.Empty;
    public NamedResource Language { get; set; } = new();
    public NamedResource Version { get; set; } = new();
}

public class NamedResource
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class EvolutionChainResponse
{
    public string Url { get; set; } = string.Empty;
}

public class ChainLink
{
    public ChainLink[] Evolves_to { get; set; } = Array.Empty<ChainLink>();
    public EvolutionDetail[] Evolution_details { get; set; } = Array.Empty<EvolutionDetail>();
    public Species Species { get; set; } = new();
}

public class EvolutionDetail
{
    public int? Min_level { get; set; }
    public NamedResource? Trigger { get; set; }
    public NamedResource? Item { get; set; }
    public bool? Held_item { get; set; }
}

public class Species
{
    public string Name { get; set; } = string.Empty;
}

public class EvolutionChainDetailResponse
{
    public ChainLink Chain { get; set; } = new();
}

public class StatResponse
{
    public int Base_stat { get; set; }
    public StatInfo Stat { get; set; } = new();
}

public class StatInfo
{
    public string Name { get; set; } = string.Empty;
}

public class AbilityResponse
{
    public AbilityInfo Ability { get; set; } = new();
    public bool Is_hidden { get; set; }
    public int Slot { get; set; }
}

public class AbilityInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class AbilityDetailResponse
{
    public List<LocalizedName> Names { get; set; } = new();
}
