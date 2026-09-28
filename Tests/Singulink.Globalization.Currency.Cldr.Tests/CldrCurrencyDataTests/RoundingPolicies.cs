namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class RoundingPolicies
{
    private static Currency Get(string code) => CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.All)[code];

    [TestMethod]
    public void StandardDecimalDigits()
    {
        Get("USD").RoundingPolicy.ShouldBe(new RoundingPolicy(2));
        Get("EUR").RoundingPolicy.ShouldBe(new RoundingPolicy(2));
        Get("JPY").RoundingPolicy.ShouldBe(new RoundingPolicy(0));
        Get("KRW").RoundingPolicy.ShouldBe(new RoundingPolicy(0));
        Get("BHD").RoundingPolicy.ShouldBe(new RoundingPolicy(3));
        Get("KWD").RoundingPolicy.ShouldBe(new RoundingPolicy(3));
        Get("CLF").RoundingPolicy.ShouldBe(new RoundingPolicy(4));
        Get("XAU").RoundingPolicy.ShouldBe(new RoundingPolicy(2)); // CLDR default
    }

    [TestMethod]
    public void EveryCurrencyHasCashRoundingPolicy()
    {
        CldrCurrencyData.AllCurrencies.ShouldAllBe(c => c.CashRoundingPolicy != null);
    }

    [TestMethod]
    public void CashRoundingSameAsStandardByDefault()
    {
        Get("USD").CashRoundingPolicy.ShouldBe(new RoundingPolicy(2));
        Get("EUR").CashRoundingPolicy.ShouldBe(new RoundingPolicy(2));
        Get("JPY").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
    }

    [TestMethod]
    public void CashRoundingIncrements()
    {
        Get("CAD").CashRoundingPolicy.ShouldBe(new RoundingPolicy(2, 5));  // Nearest 0.05 (penny withdrawn)
        Get("CHF").CashRoundingPolicy.ShouldBe(new RoundingPolicy(2, 5));  // Nearest 0.05
        Get("DKK").CashRoundingPolicy.ShouldBe(new RoundingPolicy(2, 50)); // Nearest 0.50
        Get("HUF").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0, 5));  // Nearest 5 forints
    }

    [TestMethod]
    public void CashRoundingToWholeUnits()
    {
        Get("SEK").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
        Get("NOK").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
        Get("CZK").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
        Get("TWD").CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
    }

    [TestMethod]
    public void RoundToCash()
    {
        var cad = Get("CAD");

        new MonetaryValue(10.02m, cad).RoundToCash().ShouldBe(new MonetaryValue(10.00m, cad));
        new MonetaryValue(10.03m, cad).RoundToCash().ShouldBe(new MonetaryValue(10.05m, cad));
        new MonetaryValue(10.03m, cad).Round().ShouldBe(new MonetaryValue(10.03m, cad));

        var sek = Get("SEK");

        new MonetaryValue(10.49m, sek).RoundToCash().ShouldBe(new MonetaryValue(10m, sek));
        new MonetaryValue(10.51m, sek).RoundToCash().ShouldBe(new MonetaryValue(11m, sek));
        new MonetaryValue(10.49m, sek).Round().ShouldBe(new MonetaryValue(10.49m, sek));
    }
}
