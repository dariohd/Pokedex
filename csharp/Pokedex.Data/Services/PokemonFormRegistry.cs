namespace Pokedex.Data.Services;

public static class PokemonFormRegistry
{
    private static readonly Dictionary<string, List<string>> SpecialForms =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Venusaur"] = ["mega"],
            ["Charizard"] = ["mega-x", "mega-y"],
            ["Blastoise"] = ["mega"],
            ["Gengar"] = ["mega"],
            ["Mewtwo"] = ["mega-x", "mega-y"],
            ["Blaziken"] = ["mega"],
            ["Lucario"] = ["mega"],
            ["Gardevoir"] = ["mega"],
            ["Salamence"] = ["mega"],
            ["Metagross"] = ["mega"],
            ["Rayquaza"] = ["mega"],
            ["Garchomp"] = ["mega"],
            ["Latios"] = ["mega"],
            ["Latias"] = ["mega"],
            ["Gyarados"] = ["mega"],
            ["Tyranitar"] = ["mega"],
            ["Giratina"] = ["origin", "altered"],
            ["Shaymin"] = ["sky", "land"],
            ["Deoxys"] = ["attack", "defense", "speed"],
            ["Rotom"] = ["heat", "wash", "frost", "fan", "mow"],
            ["Castform"] = ["sunny", "rainy", "snowy"],
            ["Kyurem"] = ["white", "black"]
        };

    private static readonly Dictionary<string, int> MegaIds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["venusaur-mega"] = 10033,
            ["charizard-mega-x"] = 10034,
            ["charizard-mega-y"] = 10035,
            ["blastoise-mega"] = 10036,
            ["alakazam-mega"] = 10037,
            ["gengar-mega"] = 10038,
            ["pinsir-mega"] = 10040,
            ["gyarados-mega"] = 10041,
            ["aerodactyl-mega"] = 10042,
            ["mewtwo-mega-x"] = 10043,
            ["mewtwo-mega-y"] = 10044,
            ["ampharos-mega"] = 10045,
            ["scizor-mega"] = 10046,
            ["heracross-mega"] = 10047,
            ["houndoom-mega"] = 10048,
            ["tyranitar-mega"] = 10049,
            ["blaziken-mega"] = 10050,
            ["gardevoir-mega"] = 10051,
            ["mawile-mega"] = 10052,
            ["aggron-mega"] = 10053,
            ["medicham-mega"] = 10055,
            ["manectric-mega"] = 10056,
            ["banette-mega"] = 10056,
            ["absol-mega"] = 10057,
            ["garchomp-mega"] = 10058,
            ["lucario-mega"] = 10059,
            ["abomasnow-mega"] = 10060,
            ["latias-mega"] = 10061,
            ["latios-mega"] = 10062,
            ["salamence-mega"] = 10089,
            ["metagross-mega"] = 10076,
            ["rayquaza-mega"] = 10079
        };

    private static readonly Dictionary<string, string> SlugAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["farfetchd-galar"] = "farfetchd-galar",
            ["mr-mime-galar"] = "mr-mime-galar",
            ["darmanitan-galar"] = "darmanitan-galar-standard"
        };

    public static bool TryGetFormSuffixes(string pokemonName, out List<string> suffixes) =>
        SpecialForms.TryGetValue(pokemonName, out suffixes!);

    public static bool TryGetMegaId(string formSlug, out int id) =>
        MegaIds.TryGetValue(formSlug, out id);

    public static string NormalizeSlug(string slug) =>
        SlugAliases.TryGetValue(slug, out var alias) ? alias : slug;

    public static string FormatSuffix(string suffix) => suffix.ToLower() switch
    {
        "gmax" => "Gigamax",
        "mega-x" => "Méga X",
        "mega-y" => "Méga Y",
        "mega" => "Méga",
        "alola" => "Forme Alola",
        "galar" => "Forme Galar",
        "hisui" => "Forme Hisui",
        "origin" => "Forme Originelle",
        "altered" => "Forme Alternative",
        "sky" => "Forme Céleste",
        "land" => "Forme Terrestre",
        "white" => "Kyurem Blanc",
        "black" => "Kyurem Noir",
        _ => string.Join(" ", suffix.Split('-').Select(w =>
            w.Length > 0 ? char.ToUpper(w[0]) + w[1..] : w))
    };
}
