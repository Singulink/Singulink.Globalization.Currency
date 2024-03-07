namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class TransformAmounts
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    [TestMethod]
    public void NonNullOutput_AllAmountsTransformed_UpdatesValue()
    {
        _set.TransformAmounts(x => x * 2);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void NonNullOutput_IdentityTransform_NoChange()
    {
        _set.TransformAmounts(x => x);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void NonNullOutput_EmptySet_NoChange()
    {
        SortedMoneySet emptySet = [];
        emptySet.TransformAmounts(x => x * 2);
        emptySet.Count.ShouldBe(0);
    }

    [TestMethod]
    public void NullableOutput_AllAmountsTransformed_UpdatesValue()
    {
        _set.TransformAmounts(x => (decimal?)x * 2);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void NullableOutput_IdentityTransform_NoChange()
    {
        _set.TransformAmounts(x => (decimal?)x);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void NullableOutput_EmptySet_NoChange()
    {
        SortedMoneySet emptySet = [];
        emptySet.TransformAmounts(x => (decimal?)x * 2);
        emptySet.Count.ShouldBe(0);
    }

    [TestMethod]
    public void NullableOutput_NullTransform_RemovesNullValues()
    {
        _set.TransformAmounts(x => x == 100m ? null : x);
        _set.Count.ShouldBe(2);
        _set.ShouldBe([new(50m, "CAD"), new(25m, "EUR")]);
    }
}
