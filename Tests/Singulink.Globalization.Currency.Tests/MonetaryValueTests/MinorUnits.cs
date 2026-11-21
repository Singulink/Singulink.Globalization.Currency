namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class MinorUnits
{
    private static readonly Currency Bhd = Currency.GetCurrency("BHD");                                            // 3 decimal digits
    private static readonly Currency CashLike = new("CSH", "Rounded to 0.05") { RoundingPolicy = new RoundingPolicy(2, 5) };

    [TestMethod]
    public void FromMinorUnits_UsesTheCurrencysDigits()
    {
        MonetaryValue.FromMinorUnits(1234, "USD").ShouldBe(new MonetaryValue(12.34m, "USD"));
        MonetaryValue.FromMinorUnits(1234, Currency.GetCurrency("USD")).ShouldBe(new MonetaryValue(12.34m, "USD"));
        MonetaryValue.FromMinorUnits(1234, "JPY").ShouldBe(new MonetaryValue(1234m, "JPY"));
        MonetaryValue.FromMinorUnits(1234, Bhd).ShouldBe(new MonetaryValue(1.234m, Bhd));
        MonetaryValue.FromMinorUnits(-5, "USD").ShouldBe(new MonetaryValue(-0.05m, "USD"));
        MonetaryValue.FromMinorUnits(0, "USD").ShouldBe(new MonetaryValue(0m, "USD"));
        MonetaryValue.FromMinorUnits(long.MaxValue, "USD").Amount.ShouldBe(92233720368547758.07m);
        MonetaryValue.FromMinorUnits(long.MinValue, "USD").Amount.ShouldBe(-92233720368547758.08m);
    }

    [TestMethod]
    public void ToMinorUnits_WholeMinorUnits()
    {
        new MonetaryValue(12.34m, "USD").ToMinorUnits().ShouldBe(1234);
        new MonetaryValue(12.3m, "USD").ToMinorUnits().ShouldBe(1230);
        new MonetaryValue(12.3400m, "USD").ToMinorUnits().ShouldBe(1234);
        new MonetaryValue(-12.34m, "USD").ToMinorUnits().ShouldBe(-1234);
        new MonetaryValue(1234m, "JPY").ToMinorUnits().ShouldBe(1234);
        new MonetaryValue(1.234m, Bhd).ToMinorUnits().ShouldBe(1234);
        MonetaryValue.Default.ToMinorUnits().ShouldBe(0);
    }

    [TestMethod]
    public void ToMinorUnits_PartOfAMinorUnit_Throws()
    {
        Should.Throw<InvalidOperationException>(() => new MonetaryValue(12.345m, "USD").ToMinorUnits())
            .Message.ShouldBe("The amount 12.345 USD is not a whole number of minor units. Round it, for example with ToMinorUnits(MidpointRounding).");
        Should.Throw<InvalidOperationException>(() => new MonetaryValue(12.5m, "JPY").ToMinorUnits());
        Should.Throw<InvalidOperationException>(() => new MonetaryValue(-0.001m, "USD").ToMinorUnits());
    }

    [TestMethod]
    public void ToMinorUnits_WithRounding()
    {
        new MonetaryValue(12.345m, "USD").ToMinorUnits(MidpointRounding.ToEven).ShouldBe(1234);
        new MonetaryValue(12.345m, "USD").ToMinorUnits(MidpointRounding.AwayFromZero).ShouldBe(1235);
        new MonetaryValue(-12.345m, "USD").ToMinorUnits(MidpointRounding.AwayFromZero).ShouldBe(-1235);
        new MonetaryValue(12.5m, "JPY").ToMinorUnits(MidpointRounding.ToEven).ShouldBe(12);
        new MonetaryValue(12.34m, "USD").ToMinorUnits(MidpointRounding.ToEven).ShouldBe(1234);
        MonetaryValue.Default.ToMinorUnits(MidpointRounding.ToEven).ShouldBe(0);
    }

    [TestMethod]
    public void RoundingIncrement_DoesNotChangeTheMinorUnit()
    {
        // A currency rounded to 0.05 still counts minor units of 0.01, and ToMinorUnits(mode) rounds to 0.01.

        new MonetaryValue(0.03m, CashLike).ToMinorUnits().ShouldBe(3);
        new MonetaryValue(0.034m, CashLike).ToMinorUnits(MidpointRounding.ToEven).ShouldBe(3);
        MonetaryValue.FromMinorUnits(3, CashLike).Amount.ShouldBe(0.03m);
    }

    [TestMethod]
    public void Overflow_Throws()
    {
        Should.Throw<OverflowException>(() => new MonetaryValue(92233720368547758.08m, "USD").ToMinorUnits());
        Should.Throw<OverflowException>(() => new MonetaryValue(decimal.MaxValue, "USD").ToMinorUnits());
        Should.Throw<OverflowException>(() => new MonetaryValue(decimal.MaxValue, "USD").ToMinorUnits(MidpointRounding.ToEven));
        new MonetaryValue(92233720368547758.07m, "USD").ToMinorUnits().ShouldBe(long.MaxValue);
    }

    [TestMethod]
    public void RoundTrip()
    {
        foreach (long minorUnits in new long[] { 0, 1, -1, 1234, -987654321, long.MaxValue, long.MinValue })
        {
            foreach (var currency in new[] { Currency.GetCurrency("USD"), Currency.GetCurrency("JPY"), Bhd })
                MonetaryValue.FromMinorUnits(minorUnits, currency).ToMinorUnits().ShouldBe(minorUnits);
        }
    }
}
