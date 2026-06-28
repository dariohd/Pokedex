using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace Pokedex.Wpf.Services;

public interface IPokemonImageService
{
    Task<BitmapImage?> LoadSpriteAsync(int pokemonId, bool shiny = false);
}

public class PokemonImageService : IPokemonImageService
{
    private readonly HttpClient _httpClient;

    public PokemonImageService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<BitmapImage?> LoadSpriteAsync(int pokemonId, bool shiny = false)
    {
        string[] paths = shiny
            ? new[]
            {
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/shiny/{pokemonId}.png",
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/{pokemonId}.png"
            }
            : new[]
            {
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{pokemonId}.png",
                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemonId}.png"
            };

        foreach (var url in paths)
        {
            var image = await TryLoadAsync(url);
            if (image != null) return image;
        }

        return null;
    }

    private async Task<BitmapImage?> TryLoadAsync(string url)
    {
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url);
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }
}
