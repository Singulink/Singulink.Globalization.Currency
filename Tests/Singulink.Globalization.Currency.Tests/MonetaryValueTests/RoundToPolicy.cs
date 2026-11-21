namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class RoundToPolicy
{
    private static readonly RoundingPolicy Nickel = new(2, 5);
    private static readonly RoundingPolicy Quarter = new(2, 25);
    private static readonly RoundingPolicy FiveUnits = new(0, 5);

    [TestMethod]
    public void IncrementPolicy_RoundsToTheIncrement()
    {
        new MonetaryValue(10.03m, "USD").Round(Nickel).ShouldBe(new MonetaryValue(10.05m, "USD"));
        new MonetaryValue(10.02m, "USD").Round(Nickel).ShouldBe(new MonetaryValue(10.00m, "USD"));
        new MonetaryValue(-10.03m, "USD").Round(Nickel).ShouldBe(new MonetaryValue(-10.05m, "USD"));
        new MonetaryValue(10.13m, "USD").Round(Quarter).ShouldBe(new MonetaryValue(10.25m, "USD"));
        new MonetaryValue(1234m, "JPY").Round(FiveUnits).ShouldBe(new MonetaryValue(1235m, "JPY"));
    }

    [TestMethod]
    public void IncrementPolicy_Midpoints()
    {
        // 10.025 is halfway between 10.00 and 10.05: 200.5 nickels, so to even gives 200 and away from zero gives 201.

        var value = new MonetaryValue(10.025m, "USD");

        value.Round(Nickel).ShouldBe(new MonetaryValue(10.00m, "USD"));
        value.Round(Nickel, MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(10.05m, "USD"));
        (-value).Round(Nickel).ShouldBe(new MonetaryValue(-10.00m, "USD"));
        (-value).Round(Nickel, MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(-10.05m, "USD"));

        // 10.075 is 201.5 nickels, so to even rounds up.
        new MonetaryValue(10.075m, "USD").Round(Nickel).ShouldBe(new MonetaryValue(10.10m, "USD"));
    }

#if NET
    [TestMethod]
    public void IncrementPolicy_DirectedModes()
    {
        var value = new MonetaryValue(10.01m, "USD");

        value.Round(Nickel, MidpointRounding.ToZero).ShouldBe(new MonetaryValue(10.00m, "USD"));
        value.Round(Nickel, MidpointRounding.ToNegativeInfinity).ShouldBe(new MonetaryValue(10.00m, "USD"));
        value.Round(Nickel, MidpointRounding.ToPositiveInfinity).ShouldBe(new MonetaryValue(10.05m, "USD"));
        (-value).Round(Nickel, MidpointRounding.ToZero).ShouldBe(new MonetaryValue(-10.00m, "USD"));
        (-value).Round(Nickel, MidpointRounding.ToNegativeInfinity).ShouldBe(new MonetaryValue(-10.05m, "USD"));
        (-value).Round(Nickel, MidpointRounding.ToPositiveInfinity).ShouldBe(new MonetaryValue(-10.00m, "USD"));
    }
#endif

    [TestMethod]
    public void PolicyWithoutIncrement_MatchesRoundToDecimals()
    {
        var value = new MonetaryValue(10m / 3, "USD");

        for (int decimals = 0; decimals <= 28; decimals++)
        {
            value.Round(new RoundingPolicy(decimals)).ShouldBe(value.Round(decimals));
            value.Round(new RoundingPolicy(decimals), MidpointRounding.AwayFromZero).ShouldBe(value.Round(decimals, MidpointRounding.AwayFromZero));
        }
    }

    [TestMethod]
    [DataRow("USD")]
    [DataRow("JPY")]
    [DataRow("KWD")]
    public void CurrencysPolicy_MatchesRound(string code)
    {
        foreach (decimal amount in new[] { 10.005m, 10.0005m, 10.5m, -10.005m, -10.5m, 1.23456789m })
        {
            var value = new MonetaryValue(amount, code);
            var policy = value.Currency.RoundingPolicy;

            value.Round(policy).ShouldBe(value.Round());
            value.Round(policy, MidpointRounding.ToEven).ShouldBe(value.Round(MidpointRounding.ToEven));
            value.Round(policy, MidpointRounding.AwayFromZero).ShouldBe(value.Round(MidpointRounding.AwayFromZero));
        }
    }

    [TestMethod]
    public void KeepsTheCurrency()
    {
        var value = new MonetaryValue(1234.5678m, "JPY");
        value.Round(new RoundingPolicy(2)).ShouldBe(new MonetaryValue(1234.57m, "JPY"));
        value.Round(new RoundingPolicy(2)).Currency.ShouldBeSameAs(value.Currency);
    }

    [TestMethod]
    public void DefaultValue_StaysDefault()
    {
        MonetaryValue.Default.Round(Nickel).ShouldBe(MonetaryValue.Default);
        MonetaryValue.Default.Round(RoundingPolicy.Default, MidpointRounding.AwayFromZero).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void NullPolicy_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new MonetaryValue(1m, "USD").Round((RoundingPolicy)null!)).ParamName.ShouldBe("policy");
        Should.Throw<ArgumentNullException>(() => MonetaryValue.Default.Round(null!)).ParamName.ShouldBe("policy");
    }
}
