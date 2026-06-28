using System.Collections.ObjectModel;
using System.Media;
using System.Net.Http;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pokedex.Core.Models;
using Pokedex.Data.Services;
using Pokedex.Wpf.Services;

namespace Pokedex.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int PageSize = 60;

    private readonly IPokemonService _pokemonService;
    private readonly IPokemonImageService _imageService;
    private readonly FavoritesService _favorites;
    private readonly SearchHistoryService _searchHistory;
    private CancellationTokenSource? _loadCts;

    [ObservableProperty] private ObservableCollection<BasePokemon> _pokemonList = [];
    [ObservableProperty] private ObservableCollection<BasePokemon> _filteredList = [];
    [ObservableProperty] private ObservableCollection<PokemonFormInfo> _forms = [];
    [ObservableProperty] private ObservableCollection<string> _searchHistoryItems = [];
    [ObservableProperty] private BasePokemon? _selectedListItem;
    [ObservableProperty] private Pokemon? _selectedPokemon;
    [ObservableProperty] private Pokemon? _comparePokemon;
    [ObservableProperty] private BitmapImage? _spriteImage;
    [ObservableProperty] private string _filterText = "";
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _statusMessage = "Chargement...";
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private bool _isShiny;
    [ObservableProperty] private bool _compareMode;
    [ObservableProperty] private int _currentPage;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _selectedTabIndex;

    public int BaseStatTotal => SelectedPokemon?.Stats.Sum(s => s.Value) ?? 0;
    public int CompareStatTotal => ComparePokemon?.Stats.Sum(s => s.Value) ?? 0;
    public bool IsFavorite => SelectedPokemon != null && _favorites.IsFavorite(SelectedPokemon.Id);

    public MainViewModel(
        IPokemonService pokemonService,
        IPokemonImageService imageService,
        FavoritesService favorites,
        SearchHistoryService searchHistory)
    {
        _pokemonService = pokemonService;
        _imageService = imageService;
        _favorites = favorites;
        _searchHistory = searchHistory;
        SearchHistoryItems = new ObservableCollection<string>(_searchHistory.History);
        _ = LoadPageAsync(0);
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    partial void OnIsShinyChanged(bool value)
    {
        if (SelectedPokemon != null) _ = LoadSpriteAsync(SelectedPokemon.Id);
    }

    partial void OnSelectedListItemChanged(BasePokemon? value)
    {
        if (value != null) _ = LoadPokemonAsync(value.Id);
    }

    [RelayCommand]
    private async Task LoadPageAsync(int page)
    {
        if (page < 0) return;
        IsLoading = true;
        ErrorMessage = null;
        IsOffline = false;
        try
        {
            var total = await _pokemonService.GetTotalPokemonCountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page >= TotalPages) return;

            CurrentPage = page;
            var items = await _pokemonService.GetAllPokemonAsync(page * PageSize, PageSize);
            PokemonList = new ObservableCollection<BasePokemon>(items);
            ApplyFilter();
            StatusMessage = $"{total} Pokémon · page {CurrentPage + 1}/{TotalPages}";
        }
        catch (HttpRequestException)
        {
            IsOffline = true;
            ErrorMessage = "Connexion impossible. Vérifiez votre réseau ou réessayez.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand] private Task NextPageAsync() => LoadPageAsync(CurrentPage + 1);
    [RelayCommand] private Task PreviousPageAsync() => LoadPageAsync(CurrentPage - 1);

    [RelayCommand]
    private async Task SearchAsync()
    {
        var query = SearchText.Trim();
        if (string.IsNullOrEmpty(query)) return;

        IsLoading = true;
        ErrorMessage = null;
        IsOffline = false;
        try
        {
            var pokemon = await _pokemonService.SearchPokemonAsync(query);
            if (pokemon == null)
            {
                ErrorMessage = $"Aucun Pokémon trouvé pour « {query} ».";
                return;
            }

            _searchHistory.Add(query);
            SearchHistoryItems = new ObservableCollection<string>(_searchHistory.History);

            if (CompareMode && SelectedPokemon != null && ComparePokemon == null)
            {
                ComparePokemon = pokemon;
                StatusMessage = $"Comparaison : {SelectedPokemon.DisplayName} vs {pokemon.DisplayName}";
            }
            else
            {
                await SetPokemonAsync(pokemon);
                SelectedListItem = PokemonList.FirstOrDefault(p => p.Id == pokemon.Id)
                    ?? FilteredList.FirstOrDefault(p => p.Id == pokemon.Id);
            }
        }
        catch (HttpRequestException)
        {
            IsOffline = true;
            ErrorMessage = "API inaccessible. Réessayez dans quelques instants.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (SelectedPokemon == null) return;
        _favorites.Toggle(SelectedPokemon.Id);
        OnPropertyChanged(nameof(IsFavorite));
    }

    [RelayCommand]
    private void ToggleCompareMode()
    {
        CompareMode = !CompareMode;
        if (!CompareMode) ComparePokemon = null;
    }

    [RelayCommand]
    private void PlayCry()
    {
        if (SelectedPokemon == null || string.IsNullOrEmpty(SelectedPokemon.CryUrl)) return;
        try
        {
            using var player = new SoundPlayer(SelectedPokemon.CryUrl);
            player.Play();
        }
        catch
        {
            ErrorMessage = "Impossible de lire le cri.";
        }
    }

    [RelayCommand]
    private void UseHistory(string query)
    {
        SearchText = query;
        _ = SearchAsync();
    }

    private async Task LoadPokemonAsync(int id)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
            if (pokemon == null || token.IsCancellationRequested) return;
            await SetPokemonAsync(pokemon, token);
            Forms = new ObservableCollection<PokemonFormInfo>(
                await _pokemonService.GetPokemonFormsAsync(pokemon.Id, pokemon.Name));
        }
        catch (Exception ex)
        {
            if (!token.IsCancellationRequested) ErrorMessage = ex.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested) IsLoading = false;
        }
    }

    private async Task SetPokemonAsync(Pokemon pokemon, CancellationToken token = default)
    {
        SelectedPokemon = pokemon;
        OnPropertyChanged(nameof(BaseStatTotal));
        OnPropertyChanged(nameof(IsFavorite));

        var listItem = PokemonList.FirstOrDefault(p => p.Id == pokemon.Id);
        if (listItem != null) listItem.FrenchName = pokemon.FrenchName;

        StatusMessage = $"#{pokemon.Id:D3} {pokemon.DisplayName} · BST {pokemon.Stats.Sum(s => s.Value)}";
        await LoadSpriteAsync(pokemon.Id, token);
    }

    private async Task LoadSpriteAsync(int id, CancellationToken token = default)
    {
        var image = await _imageService.LoadSpriteAsync(id, IsShiny);
        if (!token.IsCancellationRequested) SpriteImage = image;
    }

    private void ApplyFilter()
    {
        var q = FilterText.Trim();
        var source = PokemonList.AsEnumerable();
        if (!string.IsNullOrEmpty(q))
        {
            source = source.Where(p =>
                p.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Id.ToString().Contains(q));
        }
        FilteredList = new ObservableCollection<BasePokemon>(source);
    }
}
