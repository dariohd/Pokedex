using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pokedex.Core.Search;

namespace Pokedex.Tests;

[TestClass]
public class SearchHelperTests
{
    [TestMethod]
    public void NormalizeQuery_StripsHashPrefix()
    {
        Assert.AreEqual("25", PokemonSearchHelper.NormalizeQuery("#25"));
    }

    [TestMethod]
    public void TryParseId_AcceptsNumericQuery()
    {
        Assert.IsTrue(PokemonSearchHelper.TryParseId("#150", out var id));
        Assert.AreEqual(150, id);
    }

    [TestMethod]
    public void LevenshteinDistance_MeasuresEdits()
    {
        Assert.AreEqual(0, PokemonSearchHelper.LevenshteinDistance("pikachu", "pikachu"));
        Assert.AreEqual(1, PokemonSearchHelper.LevenshteinDistance("pikachu", "pikach"));
    }

    [TestMethod]
    public void IsFuzzyMatch_FindsCloseNames()
    {
        Assert.IsTrue(PokemonSearchHelper.IsFuzzyMatch("dracaufeu", "dracaufeu"));
        Assert.IsTrue(PokemonSearchHelper.IsFuzzyMatch("dracaufe", "dracaufeu"));
        Assert.IsFalse(PokemonSearchHelper.IsFuzzyMatch("abc", "mewtwo"));
    }
}
