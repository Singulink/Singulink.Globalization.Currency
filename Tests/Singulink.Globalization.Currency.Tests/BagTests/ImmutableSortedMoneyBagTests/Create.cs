namespace Singulink.Globalization.Tests.BagTests.ImmutableSortedMoneyBagTests;

[PrefixTestClass]
public class Create
{
    private static readonly MonetaryValue Usd100 = new(100m, "USD");
    private static readonly ImmutableArray<MonetaryValue> Values = [Usd100, new(50m, "CAD"), new(25m, "EUR")];

    [TestMethod]
    public void SingleValue()
    {
        var bag = ImmutableSortedMoneyBag.Create(Usd100);
        bag.Count.ShouldBe(1);
        bag.ShouldBe([Usd100]);
    }

    [TestMethod]
    public void SingleDefaultValue()
    {
        var bag = ImmutableSortedMoneyBag.Create(MonetaryValue.Default);
        bag.Count.ShouldBe(0);
        bag.ShouldBe([]);
    }

    [TestMethod]
    public void EmptyArray()
    {
        var bag = ImmutableSortedMoneyBag.Create(Array.Empty<MonetaryValue>());
        bag.Count.ShouldBe(0);
        bag.ShouldBe([]);
    }

    [TestMethod]
    public void SomeDefaultValues_IgnoresDefaultValues()
    {
        var bag = ImmutableSortedMoneyBag.Create([default, ..Values, default]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void ZeroValues_SkipsIfPresent()
    {
        var bag = ImmutableSortedMoneyBag.Create([..Values, new(0m, "USD")]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void MultipleSameCurrencyValues_AddsSameCurrencyValuesTogether()
    {
        var bag = ImmutableSortedMoneyBag.Create([..Values, ..Values, ..Values]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe([new(300m, "USD"), new(150m, "CAD"), new(75m, "EUR")], ignoreOrder: true);
    }
}