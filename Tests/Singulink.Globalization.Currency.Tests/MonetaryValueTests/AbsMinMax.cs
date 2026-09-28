namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class AbsMinMax
{
    private static readonly MonetaryValue Negative = new(-10.5m, "USD");
    private static readonly MonetaryValue Positive = new(10.5m, "USD");
    private static readonly MonetaryValue Larger = new(20m, "USD");
    private static readonly MonetaryValue Euro = new(10.5m, "EUR");

    [TestMethod]
    public void Abs()
    {
        Negative.Abs().ShouldBe(Positive);
        Positive.Abs().ShouldBe(Positive);
        new MonetaryValue(0m, "USD").Abs().ShouldBe(new MonetaryValue(0m, "USD"));
        MonetaryValue.Default.Abs().ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Min()
    {
        MonetaryValue.Min(Positive, Larger).ShouldBe(Positive);
        MonetaryValue.Min(Larger, Positive).ShouldBe(Positive);
        MonetaryValue.Min(Negative, Positive).ShouldBe(Negative);
        MonetaryValue.Min(Positive, Positive).ShouldBe(Positive);
        MonetaryValue.Min(MonetaryValue.Default, MonetaryValue.Default).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Max()
    {
        MonetaryValue.Max(Positive, Larger).ShouldBe(Larger);
        MonetaryValue.Max(Larger, Positive).ShouldBe(Larger);
        MonetaryValue.Max(Negative, Positive).ShouldBe(Positive);
        MonetaryValue.Max(Positive, Positive).ShouldBe(Positive);
    }

    [TestMethod]
    public void MinMaxDifferentCurrenciesThrow()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.Min(Positive, Euro));
        Should.Throw<ArgumentException>(() => MonetaryValue.Max(Positive, Euro));
        Should.Throw<ArgumentException>(() => MonetaryValue.Min(Positive, MonetaryValue.Default));
    }
}
