namespace Singulink.Globalization.Tests.MonetaryValueTests;

#pragma warning disable CS1718 // Comparison made to same variable

[PrefixTestClass]
public class Operators
{
    private static readonly MonetaryValue UsdMinus200 = new(-200m, "USD");
    private static readonly MonetaryValue Usd0 = new(0m, "USD");
    private static readonly MonetaryValue Usd100 = new(100m, "USD");
    private static readonly MonetaryValue Usd200 = new(200m, "USD");
    private static readonly MonetaryValue Usd300 = new(300m, "USD");
    private static readonly MonetaryValue Cad100 = new(100m, "CAD");
    private static readonly MonetaryValue Cad200 = new(200m, "CAD");

    [TestMethod]
    public void Equal_EqualValues_ReturnsTrue()
    {
        (Usd100 == Usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void Equal_DifferentValues_ReturnsFalse()
    {
        (Usd100 == Usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void Inequality_DifferentValues_ReturnsTrue()
    {
        (Usd100 != Usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void Inequality_EqualValues_ReturnsFalse()
    {
        (Usd100 != Usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_LessThan_ReturnsTrue()
    {
        (Usd100 < Usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThan_GreaterThan_ReturnsFalse()
    {
        (Usd200 < Usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_EqualValues_ReturnsFalse()
    {
        (Usd100 < Usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd200 < Cad100);
    }

    [TestMethod]
    public void GreaterThan_GreaterThan_ReturnsTrue()
    {
        (Usd200 > Usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThan_LessThan_ReturnsFalse()
    {
        (Usd100 > Usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_EqualValues_ReturnsFalse()
    {
        (Usd100 > Usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThan_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd200 > Cad100);
    }

    [TestMethod]
    public void LessThanOrEqual_LessThan_ReturnsTrue()
    {
        (Usd100 <= Usd200).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_Equal_ReturnsTrue()
    {
        (Usd100 <= Usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void LessThanOrEqual_GreaterThan_ReturnsFalse()
    {
        (Usd200 <= Usd100).ShouldBeFalse();
    }

    [TestMethod]
    public void LessThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100 <= Cad200);
    }

    [TestMethod]
    public void GreaterThanOrEqual_GreaterThan_ReturnsTrue()
    {
        (Usd200 >= Usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_Equal_ReturnsTrue()
    {
        (Usd100 >= Usd100).ShouldBeTrue();
    }

    [TestMethod]
    public void GreaterThanOrEqual_LessThan_ReturnsFalse()
    {
        (Usd100 >= Usd200).ShouldBeFalse();
    }

    [TestMethod]
    public void GreaterThanOrEqual_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100 >= Cad200);
    }

    [TestMethod]
    public void Addition_SameCurrency_ReturnsCorrectResult()
    {
        (Usd100 + Usd200).ShouldBe(Usd300);
    }

    [TestMethod]
    public void Addition_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd100 + Cad200);
    }

    [TestMethod]
    public void Addition_BothMonetaryValueOneDefault_ReturnsNonDefaultValue()
    {
        (Usd100 + MonetaryValue.Default).ShouldBe(Usd100);
        (MonetaryValue.Default + Usd100).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Addition_BothDefaultMonetaryValue_ReturnsDefault()
    {
        (MonetaryValue.Default + MonetaryValue.Default).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Addition_DefaultMonetaryValueAndZero_ReturnsDefault()
    {
        (MonetaryValue.Default + 0m).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Addition_DefaultMonetaryValueAndDecimal_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.Default + 5);
    }

    [TestMethod]
    public void Addition_MonetaryValueAndDecimal_ReturnsCorrectResult()
    {
        (Usd100 + 200m).ShouldBe(Usd300);
    }

    [TestMethod]
    public void Subtraction_SameCurrency_ReturnsCorrectResult()
    {
        (Usd200 - Usd100).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Subtraction_DifferentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Usd200 - Cad100);
    }

    [TestMethod]
    public void Subtraction_BothMonetaryValueOneDefault_ReturnsNonDefaultValue()
    {
        (Usd100 - MonetaryValue.Default).ShouldBe(Usd100);
        (MonetaryValue.Default - Usd100).ShouldBe(-Usd100);
    }

    [TestMethod]
    public void Subtraction_BothDefaultMonetaryValue_ReturnsDefault()
    {
        (MonetaryValue.Default - MonetaryValue.Default).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Subtraction_DefaultMonetaryValueAndZero_ReturnsDefault()
    {
        (MonetaryValue.Default - 0m).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Subtraction_DefaultMonetaryValueAndDecimal_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.Default - 5);
    }

    [TestMethod]
    public void Subtraction_MonetaryValueAndDecimal_ReturnsCorrectResult()
    {
        (Usd200 - 100m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Subtraction_MonetaryValueAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd200 - -100m).ShouldBe(Usd300);
    }

    [TestMethod]
    public void Multiplication_MonetaryValueAndDecimal_ReturnsCorrectResult()
    {
        (Usd100 * 2m).ShouldBe(Usd200);
    }

    [TestMethod]
    public void Multiplication_MonetaryValueAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd100 * -2m).ShouldBe(-Usd200);
    }

    [TestMethod]
    public void Multiplication_MonetaryValueAndZero_ReturnsZero()
    {
        (Usd100 * 0m).ShouldBe(Usd0);
    }

    [TestMethod]
    public void Division_MonetaryValueAndDecimal_ReturnsCorrectResult()
    {
        (Usd200 / 2m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Division_MonetaryValueAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd300 / -1.5m).ShouldBe(UsdMinus200);
    }

    [TestMethod]
    public void Division_MonetaryValueAndOne_ReturnsSameValue()
    {
        (Usd100 / 1m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Division_MonetaryValueAndZero_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => Usd100 / 0m);
    }

    [TestMethod]
    public void Division_SameCurrency_ReturnsCorrectResult()
    {
        (Usd200 / Usd100).ShouldBe(2m);
    }

    [TestMethod]
    public void Division_OneDefaultMonetaryValue_Throws()
    {
        Should.Throw<ArgumentException>(() => MonetaryValue.Default / Usd100);
        Should.Throw<ArgumentException>(() => Usd100 / MonetaryValue.Default);
    }

    [TestMethod]
    public void Division_DifferentCurrencies_Throws()
    {
        Should.Throw<ArgumentException>(() => Usd100 / Cad100);
    }

    [TestMethod]
    public void Division_BothDefaultMonetaryValue_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => MonetaryValue.Default / MonetaryValue.Default);
    }

    [TestMethod]
    public void Increment_ReturnsCorrectResult()
    {
        var value = Usd100;
        (++value).ShouldBe(new MonetaryValue(101m, "USD"));
    }

    [TestMethod]
    public void Increment_DefaultValue_Throws()
    {
        var value = MonetaryValue.Default;
        Should.Throw<ArgumentException>(() => value++);
    }

    [TestMethod]
    public void Decrement_ReturnsCorrectResult()
    {
        var value = Usd100;
        (--value).ShouldBe(new MonetaryValue(99m, "USD"));
    }

    [TestMethod]
    public void Decrement_DefaultValue_Throws()
    {
        var value = MonetaryValue.Default;
        Should.Throw<ArgumentException>(() => value--);
    }

    [TestMethod]
    public void UnaryPlus_PositiveValue_ReturnsSameValue()
    {
        (+Usd100).ShouldBe(Usd100);
    }

    [TestMethod]
    public void UnaryPlus_NegativeValue_ReturnsSameValue()
    {
        (+UsdMinus200).ShouldBe(UsdMinus200);
    }

    [TestMethod]
    public void UnaryPlus_DefaultValue_ReturnsSameValue()
    {
        (+MonetaryValue.Default).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void UnaryMinus_PositiveValue_ReturnsNegativeValue()
    {
        (-Usd200).ShouldBe(UsdMinus200);
    }

    [TestMethod]
    public void UnaryMinus_NegativeValue_ReturnsPositiveValue()
    {
        (-UsdMinus200).ShouldBe(Usd200);
    }

    [TestMethod]
    public void UnaryMinus_DefaultValue_ReturnsSameValue()
    {
        (-MonetaryValue.Default).ShouldBe(MonetaryValue.Default);
    }
}