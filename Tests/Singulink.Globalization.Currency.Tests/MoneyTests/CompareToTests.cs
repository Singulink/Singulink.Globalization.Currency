using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[TestClass]
public class CompareToTests
{
    [TestMethod]
    public void LessThan_MinusOneResult()
    {
        new Money(100m, "USD").CompareTo(new Money(200m, "USD")).ShouldBe(-1);
    }

    [TestMethod]
    public void GreaterThan_PlusOneResult()
    {
        new Money(200m, "USD").CompareTo(new Money(100m, "USD")).ShouldBe(1);
    }

    [TestMethod]
    public void Equal_ZeroResult()
    {
        new Money(100m, "USD").CompareTo(new Money(100m, "USD")).ShouldBe(0);
    }

    [TestMethod]
    public void DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(100m, "USD").CompareTo(new Money(200m, "CAD")));
    }
}