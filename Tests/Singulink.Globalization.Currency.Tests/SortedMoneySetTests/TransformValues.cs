using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class TransformValues
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    [TestMethod]
    public void AllAmountsTransformed_UpdatesValues()
    {
        _set.TransformValues(x => x.Amount * 2);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void IdentityTransform_NoChange()
    {
        _set.TransformValues(x => x.Amount);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void EmptySet_NoChange()
    {
        SortedMoneySet emptySet = [];
        emptySet.TransformValues(x => x.Amount * 2);
        emptySet.Count.ShouldBe(0);
    }
}