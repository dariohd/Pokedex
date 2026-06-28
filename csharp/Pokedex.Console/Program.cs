using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pokedex.Data.Services;
using Pokedex.Console.UI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IPokemonService, PokeApiService>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ConsoleMenu>();

var host = builder.Build();
var menu = host.Services.GetRequiredService<ConsoleMenu>();
await menu.ShowMainMenu();
