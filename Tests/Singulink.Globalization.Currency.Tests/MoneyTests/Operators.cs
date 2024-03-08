namespace Singulink.Globalization.Tests.MoneyTests;

#pragma warning disable CS1718 // Comparison made to same variable

[PrefixTestClass]
public class Operators
{
    private static readonly Money UsdMinus200 = new(-200m, "USD");
    private static readonly Money Usd0 = new(0m, "USD");
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Usd200 = new(200m, "USD");
    private static readonly Money Usd300 = new(300m, "USD");
    private static readonly Money Cad100 = new(100m, "CAD");
    private static readonly Money Cad200 = new(200m, "CAD");

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
    public void Addition_BothMoneyOneDefault_ReturnsNonDefaultValue()
    {
        (Usd100 + Money.Default).ShouldBe(Usd100);
        (Money.Default + Usd100).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Addition_BothDefaultMoney_ReturnsDefault()
    {
        (Money.Default + Money.Default).ShouldBe(Money.Default);
    }

    [TestMethod]
    public void Addition_DefaultMoneyAndZero_ReturnsDefault()
    {
        (Money.Default + 0m).ShouldBe(Money.Default);
    }

    [TestMethod]
    public void Addition_DefaultMoneyAndDecimal_Throws()
    {
        Should.Throw<ArgumentException>(() => Money.Default + 5);
    }

    [TestMethod]
    public void Addition_MoneyAndDecimal_ReturnsCorrectResult()
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
    public void Subtraction_BothMoneyOneDefault_ReturnsNonDefaultValue()
    {
        (Usd100 - Money.Default).ShouldBe(Usd100);
        (Money.Default - Usd100).ShouldBe(-Usd100);
    }

    [TestMethod]
    public void Subtraction_BothDefaultMoney_ReturnsDefault()
    {
        (Money.Default - Money.Default).ShouldBe(Money.Default);
    }

    [TestMethod]
    public void Subtraction_DefaultMoneyAndZero_ReturnsDefault()
    {
        (Money.Default - 0m).ShouldBe(Money.Default);
    }

    [TestMethod]
    public void Subtraction_DefaultMoneyAndDecimal_Throws()
    {
        Should.Throw<ArgumentException>(() => Money.Default - 5);
    }

    [TestMethod]
    public void Subtraction_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (Usd200 - 100m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Subtraction_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd200 - -100m).ShouldBe(Usd300);
    }

    [TestMethod]
    public void Multiplication_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (Usd100 * 2m).ShouldBe(Usd200);
    }

    [TestMethod]
    public void Multiplication_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd100 * -2m).ShouldBe(-Usd200);
    }

    [TestMethod]
    public void Multiplication_MoneyAndZero_ReturnsZero()
    {
        (Usd100 * 0m).ShouldBe(Usd0);
    }

    [TestMethod]
    public void Division_MoneyAndDecimal_ReturnsCorrectResult()
    {
        (Usd200 / 2m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Division_MoneyAndNegativeDecimal_ReturnsCorrectResult()
    {
        (Usd300 / -1.5m).ShouldBe(UsdMinus200);
    }

    [TestMethod]
    public void Division_MoneyAndOne_ReturnsSameMoney()
    {
        (Usd100 / 1m).ShouldBe(Usd100);
    }

    [TestMethod]
    public void Division_MoneyAndZero_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => Usd100 / 0m);
    }

    [TestMethod]
    public void Division_SameCurrency_ReturnsCorrectResult()
    {
        (Usd200 / Usd100).ShouldBe(2m);
    }

    [TestMethod]
    public void Division_OneDefaultMoney_Throws()
    {
        Should.Throw<ArgumentException>(() => Money.Default / Usd100);
        Should.Throw<ArgumentException>(() => Usd100 / Money.Default);
    }

    [TestMethod]
    public void Division_DifferentCurrencies_Throws()
    {
        Should.Throw<ArgumentException>(() => Usd100 / Cad100);
    }

    [TestMethod]
    public void Division_BothDefaultMoney_ThrowsDivideByZeroException()
    {
        Should.Throw<DivideByZeroException>(() => Money.Default / Money.Default);
    }

    [TestMethod]
    public void Increment_ReturnsCorrectResult()
    {
        var value = Usd100;
        (++value).ShouldBe(new Money(101m, "USD"));
    }

    [TestMethod]
    public void Increment_DefaultValue_Throws()
    {
        var value = Money.Default;
        Should.Throw<ArgumentException>(() => value++);
    }

    [TestMethod]
    public void Decrement_ReturnsCorrectResult()
    {
        var value = Usd100;
        (--value).ShouldBe(new Money(99m, "USD"));
    }

    [TestMethod]
    public void Decrement_DefaultValue_Throws()
    {
        var value = Money.Default;
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
        (+Money.Default).ShouldBe(Money.Default);
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
        (-Money.Default).ShouldBe(Money.Default);
    }
}