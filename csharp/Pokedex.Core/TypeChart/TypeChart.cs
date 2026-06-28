namespace Pokedex.Core.TypeChart;

public static class TypeChart
{
    // Type défensif -> types offensifs super efficaces contre lui
    private static readonly Dictionary<string, string[]> WeakTo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Normal"] = ["Combat"],
        ["Feu"] = ["Eau", "Sol", "Roche"],
        ["Eau"] = ["Électrik", "Plante"],
        ["Électrik"] = ["Sol"],
        ["Plante"] = ["Feu", "Glace", "Vol", "Poison", "Insecte"],
        ["Glace"] = ["Feu", "Combat", "Roche", "Acier"],
        ["Combat"] = ["Vol", "Psy", "Fée"],
        ["Poison"] = ["Sol", "Psy"],
        ["Sol"] = ["Eau", "Glace", "Plante"],
        ["Vol"] = ["Électrik", "Glace", "Roche"],
        ["Psy"] = ["Insecte", "Spectre", "Ténèbres"],
        ["Insecte"] = ["Feu", "Vol", "Roche"],
        ["Roche"] = ["Eau", "Plante", "Combat", "Sol", "Acier"],
        ["Spectre"] = ["Spectre", "Ténèbres"],
        ["Dragon"] = ["Glace", "Dragon", "Fée"],
        ["Ténèbres"] = ["Combat", "Insecte", "Fée"],
        ["Acier"] = ["Feu", "Combat", "Sol"],
        ["Fée"] = ["Poison", "Acier"],
    };

    public static IReadOnlyList<string> GetWeaknesses(IEnumerable<string> types)
    {
        var typeList = types.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (typeList.Count == 0) return [];

        return WeakTo.Keys
            .Where(attacking => GetMultiplier(attacking, typeList) >= 2)
            .OrderBy(w => w)
            .ToList();
    }

    public static IReadOnlyList<string> GetResistances(IEnumerable<string> types)
    {
        var typeList = types.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (typeList.Count == 0) return [];

        return WeakTo.Keys
            .Where(attacking =>
            {
                var m = GetMultiplier(attacking, typeList);
                return m > 0 && m <= 0.5;
            })
            .OrderBy(w => w)
            .ToList();
    }

    private static double GetMultiplier(string attacking, List<string> defendingTypes)
    {
        double multiplier = 1;
        foreach (var def in defendingTypes)
        {
            if (WeakTo.TryGetValue(def, out var weaknesses) &&
                weaknesses.Contains(attacking, StringComparer.OrdinalIgnoreCase))
            {
                multiplier *= 2;
            }
            else if (IsImmune(attacking, def))
            {
                multiplier = 0;
            }
            else if (IsResistant(def, attacking))
            {
                multiplier *= 0.5;
            }
        }

        return multiplier;
    }

    private static bool IsImmune(string attacking, string defending)
    {
        return (attacking.Equals("Normal", StringComparison.OrdinalIgnoreCase) && defending.Equals("Spectre", StringComparison.OrdinalIgnoreCase))
            || (attacking.Equals("Combat", StringComparison.OrdinalIgnoreCase) && defending.Equals("Spectre", StringComparison.OrdinalIgnoreCase))
            || (attacking.Equals("Électrik", StringComparison.OrdinalIgnoreCase) && defending.Equals("Sol", StringComparison.OrdinalIgnoreCase))
            || (attacking.Equals("Psy", StringComparison.OrdinalIgnoreCase) && defending.Equals("Ténèbres", StringComparison.OrdinalIgnoreCase))
            || (attacking.Equals("Dragon", StringComparison.OrdinalIgnoreCase) && defending.Equals("Fée", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsResistant(string defending, string attacking)
    {
        var resistMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Feu"] = ["Feu", "Plante", "Glace", "Insecte", "Acier", "Fée"],
            ["Eau"] = ["Feu", "Eau", "Glace", "Acier"],
            ["Plante"] = ["Eau", "Électrik", "Plante", "Sol"],
            ["Électrik"] = ["Électrik", "Vol", "Acier"],
            ["Glace"] = ["Glace"],
            ["Combat"] = ["Roche", "Ténèbres", "Insecte"],
            ["Poison"] = ["Combat", "Poison", "Plante", "Fée", "Insecte"],
            ["Sol"] = ["Poison", "Roche"],
            ["Vol"] = ["Combat", "Insecte", "Plante", "Sol"],
            ["Psy"] = ["Combat", "Psy"],
            ["Insecte"] = ["Combat", "Plante", "Sol", "Insecte"],
            ["Roche"] = ["Normal", "Feu", "Poison", "Vol"],
            ["Spectre"] = ["Poison", "Insecte"],
            ["Dragon"] = ["Feu", "Eau", "Électrik", "Plante"],
            ["Ténèbres"] = ["Spectre", "Ténèbres"],
            ["Acier"] = ["Normal", "Plante", "Glace", "Vol", "Psy", "Insecte", "Roche", "Dragon", "Acier", "Fée"],
            ["Fée"] = ["Combat", "Insecte", "Ténèbres"],
        };

        return resistMap.TryGetValue(defending, out var resists) &&
               resists.Contains(attacking, StringComparer.OrdinalIgnoreCase);
    }
}
