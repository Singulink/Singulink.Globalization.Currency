namespace Singulink.Globalization.Tests.CurrencyTests;

[PrefixTestClass]
public class RoundingPolicies
{
    [TestMethod]
    public void DefaultsForCustomCurrency()
    {
        var currency = new Currency("ABC", "Test Currency");

        currency.RoundingPolicy.ShouldBe(RoundingPolicy.Default);
        currency.DecimalDigits.ShouldBe(2);
        currency.CashRoundingPolicy.ShouldBeNull();
    }

    [TestMethod]
    public void InitializedPolicies()
    {
        var currency = new Currency("ABC", "Test Currency") {
            RoundingPolicy = new RoundingPolicy(3),
            CashRoundingPolicy = new RoundingPolicy(2, 5),
        };

        currency.RoundingPolicy.ShouldBe(new RoundingPolicy(3));
        currency.DecimalDigits.ShouldBe(3);
        currency.CashRoundingPolicy.ShouldBe(new RoundingPolicy(2, 5));
    }

    [TestMethod]
    public void SystemRegistryDecimalDigits()
    {
        Currency.GetCurrency("USD").DecimalDigits.ShouldBe(2);
        Currency.GetCurrency("USD").RoundingPolicy.DecimalUnits.ShouldBe(0);
        Currency.GetCurrency("JPY").DecimalDigits.ShouldBe(0);
        Currency.GetCurrency("BHD").DecimalDigits.ShouldBe(3);
    }

    [TestMethod]
    public void SystemRegistryHasNoCashRoundingPolicies()
    {
        CurrencyRegistry.Default.ShouldAllBe(c => c.CashRoundingPolicy == null);
    }

    [TestMethod]
    public void RoundToCashThrowsWithoutPolicy()
    {
        var value = new MonetaryValue(1.03m, "USD");

        Should.Throw<NotSupportedException>(() => value.RoundToCash()).Message.ShouldContain("USD");
        Should.Throw<NotSupportedException>(() => value.RoundToCash(MidpointRounding.AwayFromZero));
    }

    [TestMethod]
    public void RoundToCashWithPolicy()
    {
        var currency = new Currency("ABC", "Test Currency") {
            CashRoundingPolicy = new RoundingPolicy(2, 5),
        };

        var value = new MonetaryValue(1.03m, currency);

        value.Round().ShouldBe(new MonetaryValue(1.03m, currency));
        value.RoundToCash().ShouldBe(new MonetaryValue(1.05m, currency));
        new MonetaryValue(1.02m, currency).RoundToCash().ShouldBe(new MonetaryValue(1.00m, currency));
        new MonetaryValue(1.025m, currency).RoundToCash(MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(1.05m, currency));
        MonetaryValue.Default.RoundToCash().ShouldBe(MonetaryValue.Default);
    }
}
