using System.IO;
using System.Text.Json;

namespace Pokedex.Wpf.Services;

public class FavoritesService
{
    private readonly string _path;
    private HashSet<int> _ids = [];

    public FavoritesService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pokedex");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "favorites.json");
        Load();
    }

    public IReadOnlyCollection<int> Favorites => _ids;

    public bool IsFavorite(int id) => _ids.Contains(id);

    public void Toggle(int id)
    {
        if (!_ids.Add(id)) _ids.Remove(id);
        Save();
    }

    private void Load()
    {
        if (!File.Exists(_path)) return;
        try
        {
            var json = File.ReadAllText(_path);
            var list = JsonSerializer.Deserialize<List<int>>(json);
            if (list != null) _ids = list.ToHashSet();
        }
        catch { /* ignore */ }
    }

    private void Save()
    {
        File.WriteAllText(_path, JsonSerializer.Serialize(_ids.OrderBy(i => i).ToList()));
    }
}

public class SearchHistoryService
{
    private readonly string _path;
    private readonly LinkedList<string> _history = new();
    private const int MaxItems = 12;

    public SearchHistoryService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pokedex");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "search-history.json");
        Load();
    }

    public IReadOnlyCollection<string> History => _history;

    public void Add(string query)
    {
        var q = query.Trim();
        if (string.IsNullOrEmpty(q)) return;
        _history.Remove(q);
        _history.AddFirst(q);
        while (_history.Count > MaxItems) _history.RemoveLast();
        Save();
    }

    private void Load()
    {
        if (!File.Exists(_path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(_path));
            if (list != null)
                foreach (var item in list) _history.AddLast(item);
        }
        catch { /* ignore */ }
    }

    private void Save() =>
        File.WriteAllText(_path, JsonSerializer.Serialize(_history.ToList()));
}
