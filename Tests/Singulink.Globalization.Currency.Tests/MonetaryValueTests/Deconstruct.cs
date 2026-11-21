namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Deconstruct
{
    [TestMethod]
    public void Value_GivesAmountAndCurrency()
    {
        var (amount, currency) = new MonetaryValue(12.3m, "USD");

        amount.ShouldBe(12.3m);
        currency.ShouldBeSameAs(Currency.GetCurrency("USD"));
    }

    [TestMethod]
    public void DefaultValue_GivesZeroAndNull()
    {
        var (amount, currency) = MonetaryValue.Default;

        amount.ShouldBe(0m);
        currency.ShouldBeNull();
    }
}
