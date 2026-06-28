namespace Pokedex.Core.Models;

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FrenchName { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(FrenchName) ? Name : FrenchName;
    public List<string> Types { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public List<EvolutionChain> Evolutions { get; set; } = new();
    public EvolutionNode? EvolutionTree { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public List<Stats> Stats { get; set; } = new();
    public List<Ability> Abilities { get; set; } = new();
    public string Genus { get; set; } = string.Empty;
    public int CaptureRate { get; set; }
    public int BaseExperience { get; set; }
    public bool IsLegendary { get; set; }
    public bool IsMythical { get; set; }
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Resistances { get; set; } = new();
    public string Habitat { get; set; } = string.Empty;
    public string Generation { get; set; } = string.Empty;
    public string CryUrl { get; set; } = string.Empty;
}

public class EvolutionChain
{
    public string Name { get; set; } = string.Empty;
    public string FrenchName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Trigger { get; set; } = string.Empty;
}

public class EvolutionNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FrenchName { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(FrenchName) ? Name : FrenchName;
    public int MinLevel { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public List<EvolutionNode> Children { get; set; } = new();
}

public class Stats
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class Ability
{
    public string Name { get; set; } = string.Empty;
    public string FrenchName { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(FrenchName) ? Name : FrenchName;
    public bool IsHidden { get; set; }
}

public class PokemonFormInfo
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> AlternateImageUrls { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public int FormId { get; set; }
}
