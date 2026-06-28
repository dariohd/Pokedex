using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pokedex.Data.Services;
using Pokedex.Wpf.Services;
using Pokedex.Wpf.ViewModels;

namespace Pokedex.Wpf;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMemoryCache();
                services.AddHttpClient<IPokemonService, PokeApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
                services.AddHttpClient<IPokemonImageService, PokemonImageService>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                });
                services.AddSingleton<FavoritesService>();
                services.AddSingleton<SearchHistoryService>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
