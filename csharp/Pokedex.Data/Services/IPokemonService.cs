using Pokedex.Core.Models;

namespace Pokedex.Data.Services;

public interface IPokemonService
{
    Task<Pokemon?> GetPokemonByNameAsync(string name);
    Task<Pokemon?> GetPokemonByIdAsync(int id);
    Task<Pokemon?> SearchPokemonAsync(string query);
    Task<List<BasePokemon>> GetAllPokemonAsync(int offset = 0, int limit = 20);
    Task<int> GetTotalPokemonCountAsync();
    Task<List<PokemonFormInfo>> GetPokemonFormsAsync(int pokemonId, string pokemonName);
}
