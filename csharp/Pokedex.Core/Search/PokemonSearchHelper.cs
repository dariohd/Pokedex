namespace Pokedex.Core.Search;

public static class PokemonSearchHelper
{
    public static string NormalizeQuery(string query)
    {
        var q = query.Trim();
        if (q.StartsWith('#')) q = q[1..].Trim();
        return q;
    }

    public static bool TryParseId(string query, out int id)
    {
        id = 0;
        var q = NormalizeQuery(query);
        return int.TryParse(q, out id) && id > 0;
    }

    public static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b.Length;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var d = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) d[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = char.ToLowerInvariant(a[i - 1]) == char.ToLowerInvariant(b[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[a.Length, b.Length];
    }

    public static bool IsFuzzyMatch(string query, string candidate, int maxDistance = 2)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(candidate)) return false;
        if (candidate.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
        if (query.Length < 4) return false;
        return LevenshteinDistance(query.ToLowerInvariant(), candidate.ToLowerInvariant()) <= maxDistance;
    }
}
