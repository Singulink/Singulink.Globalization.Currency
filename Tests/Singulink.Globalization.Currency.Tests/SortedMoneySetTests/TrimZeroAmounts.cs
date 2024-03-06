using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class TrimZeroAmounts
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Money Gbp0 = new(0m, "GBP");
    private static readonly Money Jpy0 = new(0m, "JPY");

    private static readonly ImmutableSortedMoneySet AllAmountsSet = [Usd100, Cad50, Gbp0, Eur25, Jpy0];
    private static readonly ImmutableSortedMoneySet NoZeroAmountsSet = [Usd100, Cad50, Eur25];
    private static readonly ImmutableSortedMoneySet AllZeroAmountsSet = [Gbp0, Jpy0];

    [TestMethod]
    public void SomeZeroAmounts_RemovesZeroAmountValues()
    {
        var set = AllAmountsSet.ToSet();
        set.TrimZeroAmounts();
        set.Count.ShouldBe(3);
        set.ShouldBe(NoZeroAmountsSet);
    }

    [TestMethod]
    public void NoZeroAmounts_NoChange()
    {
        var set = NoZeroAmountsSet.ToSet();
        set.TrimZeroAmounts();
        set.ShouldBe(NoZeroAmountsSet);
    }

    [TestMethod]
    public void AllZeroAmounts_RemovesAllValues()
    {
        var set = AllZeroAmountsSet.ToSet();
        set.TrimZeroAmounts();
        set.Count.ShouldBe(0);
        set.ShouldBe([]);
    }
}
