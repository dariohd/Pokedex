using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pokedex.Core.TypeChart;

namespace Pokedex.Tests;

[TestClass]
public class TypeChartTests
{
    [TestMethod]
    public void Electric_HasGroundWeakness()
    {
        var weaknesses = TypeChart.GetWeaknesses(["Électrik"]);
        CollectionAssert.Contains(weaknesses.ToList(), "Sol");
    }

    [TestMethod]
    public void DualType_CombinesMultipliers()
    {
        var weaknesses = TypeChart.GetWeaknesses(["Feu", "Glace"]);
        CollectionAssert.Contains(weaknesses.ToList(), "Combat");
    }

    [TestMethod]
    public void SteelType_HasResistances()
    {
        var resistances = TypeChart.GetResistances(["Acier"]);
        Assert.IsTrue(resistances.Count > 5);
        CollectionAssert.Contains(resistances.ToList(), "Normal");
    }
}
