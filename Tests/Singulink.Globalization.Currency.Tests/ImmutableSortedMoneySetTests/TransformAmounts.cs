using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class TransformAmounts
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    [TestMethod]
    public void AllAmountsTransformed_UpdatesValues()
    {
        var resultSet = Set.TransformAmounts(x => x * 2);
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void IdentityTransform_NoChange()
    {
        var resultSet = Set.TransformAmounts(x => x);
        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void EmptySet_NoChange()
    {
        ImmutableSortedMoneySet emptySet = [];
        var resultSet = emptySet.TransformAmounts(x => x * 2);
        resultSet.ShouldBeSameAs(emptySet);
    }
}
