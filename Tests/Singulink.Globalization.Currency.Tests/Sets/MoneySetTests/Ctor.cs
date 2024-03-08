namespace Singulink.Globalization.Tests.Sets.MoneySetTests;

[PrefixTestClass]
public class Ctor
{
    private static readonly Money Usd100 = new Money(100m, "USD");
    private static readonly ImmutableArray<Money> SetValues = [Usd100, new(50m, "CAD"), new(25m, "EUR")];

    [TestMethod]
    public void EmptyArray()
    {
        var set = new MoneySet(Array.Empty<Money>());
        set.Count.ShouldBe(0);
        set.ShouldBe([]);
    }

    [TestMethod]
    public void SomeDefaultValues_IgnoresDefaultValues()
    {
        var set = new MoneySet([default, ..SetValues, default]);
        set.Count.ShouldBe(3);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void ZeroValues_SkipsIfPresent()
    {
        var set = new MoneySet([..SetValues, new(0m, "USD")]);
        set.Count.ShouldBe(3);
        set.ShouldBe(SetValues, ignoreOrder: true);
    }

    [TestMethod]
    public void MultipleSameCurrencyValues_AddsSameCurrencyValuesTogether()
    {
        var set = new MoneySet([..SetValues, ..SetValues, ..SetValues]);
        set.Count.ShouldBe(3);
        set.ShouldBe([new(300m, "USD"), new(150m, "CAD"), new(75m, "EUR")], ignoreOrder: true);
    }
}