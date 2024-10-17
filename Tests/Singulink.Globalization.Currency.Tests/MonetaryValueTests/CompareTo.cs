namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class CompareTo
{
    private static readonly MonetaryValue Usd100 = new(100m, "USD");
    private static readonly MonetaryValue Usd200 = new(200m, "USD");

    [TestMethod]
    public void LessThan_MinusOneResult()
    {
        Usd100.CompareTo(Usd200).ShouldBe(-1);
    }

    [TestMethod]
    public void GreaterThan_PlusOneResult()
    {
        Usd200.CompareTo(Usd100).ShouldBe(1);
    }

    [TestMethod]
    public void Equal_ZeroResult()
    {
        Usd100.CompareTo(Usd100).ShouldBe(0);
    }

    [TestMethod]
    public void DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100.CompareTo(new MonetaryValue(200m, "CAD")));
    }
}