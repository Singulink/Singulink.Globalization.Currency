namespace Singulink.Globalization.Tests.BagTests.SortedMoneyBagTests;

[PrefixTestClass]
public class Ctor
{
    private static readonly MonetaryValue Usd100 = new(100m, "USD");
    private static readonly ImmutableArray<MonetaryValue> Values = [Usd100, new(50m, "CAD"), new(25m, "EUR")];

    [TestMethod]
    public void EmptyArray()
    {
        var bag = new SortedMoneyBag(Array.Empty<MonetaryValue>());
        bag.Count.ShouldBe(0);
        bag.ShouldBe([]);
    }

    [TestMethod]
    public void SomeDefaultValues_IgnoresDefaultValues()
    {
        var bag = new SortedMoneyBag([default, ..Values, default]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void ZeroValues_SkipsIfPresent()
    {
        var bag = new SortedMoneyBag([..Values, new(0m, "USD")]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe(Values, ignoreOrder: true);
    }

    [TestMethod]
    public void MultipleSameCurrencyValues_AddsSameCurrencyValuesTogether()
    {
        var bag = new SortedMoneyBag([..Values, ..Values, ..Values]);
        bag.Count.ShouldBe(3);
        bag.ShouldBe([new(300m, "USD"), new(150m, "CAD"), new(75m, "EUR")], ignoreOrder: true);
    }
}