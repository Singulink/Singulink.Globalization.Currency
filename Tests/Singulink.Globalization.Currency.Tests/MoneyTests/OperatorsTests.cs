using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

#pragma warning disable CS1718 // Comparison made to same variable

[TestClass]
public class OperatorsTests
{
    private static readonly Money _usdMinus200 = new(-200m, "USD");
    private static readonly Money _usd0 = new(0m, "USD");
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _usd200 = new(200m, "USD");
    private static readonly Money _usd300 = new(300m, "USD");
    private static readonly Money _cad100 = new(100m, "CAD");
    private static readonly Money _cad200 = new(200m, "CAD");

    [TestMethod]
    public void Equal_EqualValues_ReturnsTrue()
    {
        (_usd100 == _usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void Equal_DifferentValues_ReturnsFalse()
    {
        (_usd100 == _usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void Inequality_DifferentValues_ReturnsTrue()
    {
        (_usd100 != _usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void Inequality_DifferentValues_ReturnsFalse()
    {
        (_usd100 != _usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_LessThan_ReturnsTrue()
    {
        (_usd100 < _usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThan_GreaterThan_ReturnsFalse()
    {
        (_usd200 < _usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_EqualValues_ReturnsFalse()
    {
        (_usd100 < _usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd200 < _cad100);
    }

    [TestMethod]
    public void GreaterThan_GreaterThan_ReturnsTrue()
    {
        (_usd200 > _usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThan_LessThan_ReturnsFalse()
    {
        (_usd100 > _usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_EqualValues_ReturnsFalse()
    {
        (_usd100 > _usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd200 > _cad100);
    }

    [TestMethod]
    public void LessThanOrEqual_LessThan_ReturnsTrue()
    {
        (_usd100 <= _usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_Equal_ReturnsTrue()
    {
        (_usd100 <= _usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_GreaterThan_ReturnsFalse()
    {
        (_usd200 <= _usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd100 <= _cad200);
    }

    [TestMethod]
    public void GreaterThanOrEqual_GreaterThan_ReturnsTrue()
    {
        (_usd200 >= _usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_Equal_ReturnsTrue()
    {
        (_usd100 >= _usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_LessThan_ReturnsFalse()
    {
        (_usd100 >= _usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd100 >= _cad200);
    }

    [TestMethod]
    public void Addition_SameCurrency_ReturnsCorrectResult()
    {
        (_usd100 + _usd200).ShouldBe(_usd300);
    }

    [TestMethod]
    public void Addition_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd100 + _cad200);
    }

    [TestMethod]
    public void Addition_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (_usd100 + 200m).ShouldBe(_usd300);
    }

    [TestMethod]
    public void Addition_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (_usd200 + -100m).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Addition_DecimalAndMoney_ReturnsCorrectResult()
    {
        (100m + _usd200).ShouldBe(_usd300);
    }

    [TestMethod]
    public void Addition_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-100m + _usd200).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Subtraction_SameCurrency_ReturnsCorrectResult()
    {
        (_usd200 - _usd100).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Subtraction_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd200 - _cad100);
    }

    [TestMethod]
    public void Subtraction_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (_usd200 - 100m).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Subtraction_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (_usd200 - -100m).ShouldBe(_usd300);
    }

    [TestMethod]
    public void Subtraction_DecimalAndMoney_ReturnsCorrectResult()
    {
        (200m - _usd100).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Subtraction_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-100m - _usd200).ShouldBe(-_usd300);
    }

    [TestMethod]
    public void Subtraction_DecimalMinusMoney_ReturnsCorrectResult()
    {
        (200m - _usd100).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Subtraction_NegativeDecimalMinusMoney_ReturnsCorrectResult()
    {
        (-100m - _usd200).ShouldBe(-_usd300);
    }

    [TestMethod]
    public void Multiplication_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (_usd100 * 2m).ShouldBe(_usd200);
    }

    [TestMethod]
    public void Multiplication_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (_usd100 * -2m).ShouldBe(-_usd200);
    }

    [TestMethod]
    public void Multiplication_MoneyAndZero_ReturnsZero()
    {
        (_usd100 * 0m).ShouldBe(_usd0);
    }

    [TestMethod]
    public void Multiplication_DecimalAndMoney_ReturnsCorrectResult()
    {
        (2m * _usd100).ShouldBe(_usd200);
    }

    [TestMethod]
    public void Multiplication_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-2m * _usd100).ShouldBe(-_usd200);
    }

    [TestMethod]
    public void Multiplication_ZeroAndMoney_ReturnsZero()
    {
        (0m * _usd100).ShouldBe(_usd0);
    }

    [TestMethod]
    public void Division_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (_usd200 / 2m).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Division_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (_usd300 / -1.5m).ShouldBe(_usdMinus200);
    }

    [TestMethod]
    public void Division_MoneyAndOne_ReturnsSameMoney()
    {
        (_usd100 / 1m).ShouldBe(_usd100);
    }

    [TestMethod]
    public void Division_MoneyAndZero_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => _usd100 / 0m);
    }

    [TestMethod]
    public void Division_DecimalAndMoney_ReturnsCorrectResult()
    {
        (20000m / _usd100).ShouldBe(_usd200);
    }

    [TestMethod]
    public void Division_NegativeDecimalAndMoney_ReturnsCorrectResult()
    {
        (-20000m / _usd100).ShouldBe(_usdMinus200);
    }

    [TestMethod]
    public void Division_DecimalAndZeroMoney_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => 200m / _usd0);
    }

    [TestMethod]
    public void Increment_ReturnsCorrectResult()
    {
        var value = _usd100;
        (++value).ShouldBe(new Money(101m, "USD"));
    }

    [TestMethod]
    public void Decrement_ReturnsCorrectResult()
    {
        var value = _usd100;
        (--value).ShouldBe(new Money(99m, "USD"));
    }

    [TestMethod]
    public void UnaryPlus_PositiveValue_ReturnsSameValue()
    {
        (+_usd100).ShouldBe(_usd100);
    }

    [TestMethod]
    public void UnaryPlus_NegativeValue_ReturnsSameValue()
    {
        (+_usdMinus200).ShouldBe(_usdMinus200);
    }

    [TestMethod]
    public void UnaryMinus_PositiveValue_ReturnsNegativeValue()
    {
        (-_usd200).ShouldBe(_usdMinus200);
    }

    [TestMethod]
    public void UnaryMinus_NegativeValue_ReturnsPositiveValue()
    {
        (-_usdMinus200).ShouldBe(_usd200);
    }
}