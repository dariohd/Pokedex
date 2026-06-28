namespace Pokedex.Core.Localization;

public static class PokemonLabels
{
    public static string Type(string englishType) => englishType.ToLower() switch
    {
        "normal" => "Normal",
        "fire" => "Feu",
        "water" => "Eau",
        "electric" => "Électrik",
        "grass" => "Plante",
        "ice" => "Glace",
        "fighting" => "Combat",
        "poison" => "Poison",
        "ground" => "Sol",
        "flying" => "Vol",
        "psychic" => "Psy",
        "bug" => "Insecte",
        "rock" => "Roche",
        "ghost" => "Spectre",
        "dragon" => "Dragon",
        "dark" => "Ténèbres",
        "steel" => "Acier",
        "fairy" => "Fée",
        _ => englishType
    };

    public static string Stat(string statName) => statName.ToLower() switch
    {
        "hp" => "PV",
        "attack" => "Attaque",
        "defense" => "Défense",
        "special-attack" or "sp. attack" => "Att. Spé.",
        "special-defense" or "sp. defense" => "Déf. Spé.",
        "speed" => "Vitesse",
        _ => statName
    };

    public static string EvolutionTrigger(string trigger) => trigger.ToLower() switch
    {
        "level-up" => "Niveau",
        "use-item" => "Objet",
        "trade" => "Échange",
        "shed" => "Mue",
        "spin" => "Rotation",
        "tower-of-darkness" => "Tour des Ténèbres",
        "tower-of-waters" => "Tour des Eaux",
        "three-critical-hits" => "3 coups critiques",
        "take-damage" => "Dégâts subis",
        "other" => "Spécial",
        "" => "",
        _ => trigger
    };
}
