using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[TestClass]
public class OperatorsTests
{
    [TestMethod]
    public void Equal_EqualValues_ReturnsTrue()
    {
        (new Money(100m, "USD") == new Money(100m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void Equal_DifferentValues_ReturnsFalse()
    {
        (new Money(100m, "USD") == new Money(200m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void Inequality_DifferentValues_ReturnsTrue()
    {
        (new Money(100m, "USD") != new Money(200m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void Inequality_DifferentValues_ReturnsFalse()
    {
        (new Money(100m, "USD") != new Money(100m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_LessThan_ReturnsTrue()
    {
        (new Money(100m, "USD") < new Money(200m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThan_GreaterThan_ReturnsFalse()
    {
        (new Money(200m, "USD") < new Money(100m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_EqualValues_ReturnsFalse()
    {
        (new Money(100m, "USD") < new Money(100m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(200m, "USD") < new Money(100m, "CAD"));
    }

    [TestMethod]
    public void GreaterThan_GreaterThan_ReturnsTrue()
    {
        (new Money(200m, "USD") > new Money(100m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThan_LessThan_ReturnsFalse()
    {
        (new Money(100m, "USD") > new Money(200m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_EqualValues_ReturnsFalse()
    {
        (new Money(100m, "USD") > new Money(100m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(200m, "USD") > new Money(100m, "CAD"));
    }

    [TestMethod]
    public void LessThanOrEqual_LessThan_ReturnsTrue()
    {
        (new Money(100m, "USD") <= new Money(200m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_Equal_ReturnsTrue()
    {
        (new Money(100m, "USD") <= new Money(100m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_GreaterThan_ReturnsFalse()
    {
        (new Money(200m, "USD") <= new Money(100m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(100m, "USD") <= new Money(200m, "CAD"));
    }

    [TestMethod]
    public void GreaterThanOrEqual_GreaterThan_ReturnsTrue()
    {
        (new Money(200m, "USD") >= new Money(100m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_Equal_ReturnsTrue()
    {
        (new Money(100m, "USD") >= new Money(100m, "USD")).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_LessThan_ReturnsFalse()
    {
        (new Money(100m, "USD") >= new Money(200m, "USD")).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(100m, "USD") >= new Money(200m, "CAD"));
    }

    [TestMethod]
    public void Addition_SameCurrency_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") + new Money(200m, "USD")).ShouldBe(new Money(300m, "USD"));
    }

    [TestMethod]
    public void Addition_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(100m, "USD") + new Money(200m, "CAD"));
    }

    [TestMethod]
    public void Addition_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") + 200m).ShouldBe(new Money(300m, "USD"));
    }

    [TestMethod]
    public void Addition_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") + -50m).ShouldBe(new Money(50m, "USD"));
    }

    [TestMethod]
    public void Addition_DecimalAndMoney_ReturnsCorrectResult()
    {
        (100m + new Money(200m, "USD")).ShouldBe(new Money(300m, "USD"));
    }

    [TestMethod]
    public void Addition_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-50m + new Money(100m, "USD")).ShouldBe(new Money(50m, "USD"));
    }

    [TestMethod]
    public void Subtraction_SameCurrency_ReturnsCorrectResult()
    {
        (new Money(200m, "USD") - new Money(100m, "USD")).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void Subtraction_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new Money(200m, "USD") - new Money(100m, "CAD"));
    }

    [TestMethod]
    public void Subtraction_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (new Money(200m, "USD") - 100m).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void Subtraction_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (new Money(200m, "USD") - -100m).ShouldBe(new Money(300m, "USD"));
    }

    [TestMethod]
    public void Subtraction_DecimalAndMoney_ReturnsCorrectResult()
    {
        (200m - new Money(100m, "USD")).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void Subtraction_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-100m - new Money(200m, "USD")).ShouldBe(new Money(-300m, "USD"));
    }

    [TestMethod]
    public void Subtraction_DecimalMinusMoney_ReturnsCorrectResult()
    {
        (200m - new Money(100m, "USD")).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void Subtraction_NegativeDecimalMinusMoney_ReturnsCorrectResult()
    {
        (-100m - new Money(200m, "USD")).ShouldBe(new Money(-300m, "USD"));
    }

    [TestMethod]
    public void Multiplication_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") * 2m).ShouldBe(new Money(200m, "USD"));
    }

    [TestMethod]
    public void Multiplication_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") * -2m).ShouldBe(new Money(-200m, "USD"));
    }

    [TestMethod]
    public void Multiplication_MoneyAndZero_ReturnsZero()
    {
        (new Money(100m, "USD") * 0m).ShouldBe(new Money(0m, "USD"));
    }

    [TestMethod]
    public void Multiplication_DecimalAndMoney_ReturnsCorrectResult()
    {
        (2m * new Money(100m, "USD")).ShouldBe(new Money(200m, "USD"));
    }

    [TestMethod]
    public void Multiplication_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-2m * new Money(100m, "USD")).ShouldBe(new Money(-200m, "USD"));
    }

    [TestMethod]
    public void Multiplication_ZeroAndMoney_ReturnsZero()
    {
        (0m * new Money(100m, "USD")).ShouldBe(new Money(0m, "USD"));
    }

    [TestMethod]
    public void Division_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") / 2m).ShouldBe(new Money(50m, "USD"));
    }

    [TestMethod]
    public void Division_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (new Money(100m, "USD") / -2m).ShouldBe(new Money(-50m, "USD"));
    }

    [TestMethod]
    public void Division_MoneyAndOne_ReturnsSameMoney()
    {
        (new Money(100m, "USD") / 1m).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void Division_MoneyAndZero_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => new Money(100m, "USD") / 0m);
    }

    [TestMethod]
    public void Division_DecimalAndMoney_ReturnsCorrectResult()
    {
        (200m / new Money(100m, "USD")).ShouldBe(new Money(2m, "USD"));
    }

    [TestMethod]
    public void Division_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-200m / new Money(100m, "USD")).ShouldBe(new Money(-2m, "USD"));
    }

    [TestMethod]
    public void Division_DecimalAndOneMoney_ReturnsSameDecimal()
    {
        (200m / new Money(1m, "USD")).ShouldBe(new Money(200m, "USD"));
    }

    [TestMethod]
    public void Division_DecimalAndZeroMoney_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => 200m / new Money(0m, "USD"));
    }

    [TestMethod]
    public void Increment_ReturnsCorrectResult()
    {
        var money = new Money(100m, "USD");
        (++money).ShouldBe(new Money(101m, "USD"));
    }

    [TestMethod]
    public void Decrement_ReturnsCorrectResult()
    {
        var money = new Money(100m, "USD");
        (--money).ShouldBe(new Money(99m, "USD"));
    }

    [TestMethod]
    public void UnaryPlus_ReturnsSameValue()
    {
        (+new Money(100m, "USD")).ShouldBe(new Money(100m, "USD"));
    }

    [TestMethod]
    public void UnaryPlus_NegativeValue_ReturnsSameValue()
    {
        (+new Money(-100m, "USD")).ShouldBe(new Money(-100m, "USD"));
    }

    [TestMethod]
    public void UnaryMinus_PositiveValue_ReturnsNegativeValue()
    {
        (-new Money(100m, "USD")).ShouldBe(new Money(-100m, "USD"));
    }

    [TestMethod]
    public void UnaryMinus_NegativeValue_ReturnsPositiveValue()
    {
        (-new Money(-100m, "USD")).ShouldBe(new Money(100m, "USD"));
    }
}