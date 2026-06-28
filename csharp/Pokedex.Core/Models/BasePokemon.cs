namespace Pokedex.Core.Models;

public class BasePokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FrenchName { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(FrenchName) ? Name : FrenchName;
}
