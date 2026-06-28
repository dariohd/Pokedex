using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Pokedex.Data.Services;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Pokedex.Tests;

[TestClass]
public class PokemonServiceTests
{
    private Mock<HttpMessageHandler>? _handler;
    private HttpClient? _httpClient;
    private IPokemonService? _service;
    private IMemoryCache? _cache;

    [TestInitialize]
    public void Initialize()
    {
        _handler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handler.Object) { BaseAddress = new Uri("https://pokeapi.co/api/v2/") };
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new PokeApiService(_httpClient, _cache);
    }

    [TestMethod]
    public async Task GetPokemonByName_ReturnsMappedPokemon()
    {
        const string slug = "pikachu";
        Setup("pokemon/pikachu", """
        {
          "id": 25,
          "name": "pikachu",
          "height": 4,
          "weight": 60,
          "types": [{ "type": { "name": "electric" } }],
          "stats": [{ "base_stat": 35, "stat": { "name": "speed" } }],
          "abilities": [
            { "ability": { "name": "static", "url": "https://pokeapi.co/api/v2/ability/9/" }, "is_hidden": false, "slot": 1 },
            { "ability": { "name": "lightning-rod", "url": "https://pokeapi.co/api/v2/ability/31/" }, "is_hidden": true, "slot": 3 }
          ]
        }
        """);

        Setup("pokemon-species/25", """
        {
          "names": [
            { "name": "Pikachu", "language": { "name": "fr" } },
            { "name": "Pikachu", "language": { "name": "en" } }
          ],
          "genera": [{ "genus": "Pokémon Souris", "language": { "name": "fr" } }],
          "flavor_text_entries": [
            { "flavor_text": "Quand plusieurs de ces Pokémon se réunissent, ils provoquent de gigantesques orages.", "language": { "name": "fr" } }
          ],
          "evolution_chain": { "url": "https://pokeapi.co/api/v2/evolution-chain/10/" },
          "capture_rate": 190,
          "base_experience": 112,
          "is_legendary": false,
          "is_mythical": false
        }
        """);

        Setup("evolution-chain/10", """
        {
          "chain": {
            "species": { "name": "pichu" },
            "evolves_to": [{
              "species": { "name": "pikachu" },
              "evolution_details": [{ "min_level": null, "trigger": { "name": "level-up" } }],
              "evolves_to": [{
                "species": { "name": "raichu" },
                "evolution_details": [{ "min_level": null, "trigger": { "name": "use-item" }, "item": { "name": "thunder-stone" } }]
              }]
            }]
          }
        }
        """);

        Setup("ability/static", """{ "names": [{ "name": "Statik", "language": { "name": "fr" } }] }""");
        Setup("ability/lightning-rod", """{ "names": [{ "name": "Paratonnerre", "language": { "name": "fr" } }] }""");

        var pokemon = await _service!.GetPokemonByNameAsync(slug);

        Assert.IsNotNull(pokemon);
        Assert.AreEqual(25, pokemon.Id);
        Assert.AreEqual("Pikachu", pokemon.Name);
        Assert.AreEqual("Pikachu", pokemon.FrenchName);
        Assert.AreEqual("Électrik", pokemon.Types[0]);
        Assert.AreEqual("Statik", pokemon.Abilities[0].FrenchName);
        Assert.IsTrue(pokemon.Abilities[1].IsHidden);
        Assert.IsTrue(pokemon.Description.Contains("orages"));
        Assert.IsTrue(pokemon.Evolutions.Count >= 1);
        Assert.IsNotNull(pokemon.EvolutionTree);
    }

    [TestMethod]
    public async Task GetAllPokemonAsync_FiltersAlternateForms()
    {
        Setup("pokemon?offset=0&limit=5", """
        {
          "count": 1302,
          "results": [
            { "name": "bulbasaur", "url": "https://pokeapi.co/api/v2/pokemon/1/" },
            { "name": "charizard-mega-x", "url": "https://pokeapi.co/api/v2/pokemon/10034/" },
            { "name": "pikachu", "url": "https://pokeapi.co/api/v2/pokemon/25/" }
          ]
        }
        """);

        var list = await _service!.GetAllPokemonAsync(0, 5);

        Assert.AreEqual(2, list.Count);
        Assert.IsFalse(list.Any(p => p.Name.Contains("Mega", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task GetTotalPokemonCountAsync_CapsAtMainDexLimit()
    {
        Setup("pokemon?limit=1&offset=0", """{ "count": 5000 }""");

        var count = await _service!.GetTotalPokemonCountAsync();
        Assert.AreEqual(1025, count);
    }

    private void Setup(string pathSuffix, string json)
    {
        _handler!
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.AbsoluteUri.Contains(pathSuffix, StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });
    }
}
