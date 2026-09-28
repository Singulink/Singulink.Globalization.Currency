namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Allocate
{
    private static readonly Currency Cad = new("CAD", "Canadian Dollar", "$") { CashRoundingPolicy = new RoundingPolicy(2, 5) };

    private static MonetaryValue Usd(decimal amount) => new(amount, "USD");

    private static void ShouldSumTo(ImmutableArray<MonetaryValue> parts, MonetaryValue total)
    {
        var sum = MonetaryValue.CreateOrDefault(0, total.CurrencyOrDefault);

        foreach (var part in parts)
            sum += part;

        sum.ShouldBe(total);
    }

    [TestMethod]
    public void EqualParts()
    {
        var parts = Usd(100m).Allocate(3);

        parts.ShouldBe([Usd(33.34m), Usd(33.33m), Usd(33.33m)]);
        ShouldSumTo(parts, Usd(100m));

        Usd(100m).Allocate(1).ShouldBe([Usd(100m)]);
        Usd(100m).Allocate(4).ShouldBe([Usd(25m), Usd(25m), Usd(25m), Usd(25m)]);
        Usd(0.05m).Allocate(3).ShouldBe([Usd(0.02m), Usd(0.02m), Usd(0.01m)]);
        Usd(0.01m).Allocate(3).ShouldBe([Usd(0.01m), Usd(0m), Usd(0m)]);
        Usd(0m).Allocate(3).ShouldBe([Usd(0m), Usd(0m), Usd(0m)]);
    }

    [TestMethod]
    public void EqualPartsZeroDecimalCurrency()
    {
        var jpy = new MonetaryValue(100m, "JPY");
        var parts = jpy.Allocate(3);

        parts.ShouldBe([new(34m, "JPY"), new(33m, "JPY"), new(33m, "JPY")]);
        ShouldSumTo(parts, jpy);
    }

    [TestMethod]
    public void Ratios()
    {
        var parts = Usd(100m).Allocate(50, 30, 20);
        parts.ShouldBe([Usd(50m), Usd(30m), Usd(20m)]);

        // 100 / 3 by ratio 1:1:1 is the same as equal parts
        Usd(100m).Allocate(1, 1, 1).ShouldBe([Usd(33.34m), Usd(33.33m), Usd(33.33m)]);

        // Largest remainder wins: shares are 33.33.., 66.66.. so the second part gets the extra cent
        Usd(100m).Allocate(1, 2).ShouldBe([Usd(33.33m), Usd(66.67m)]);

        // Ratios do not need to sum to 1 or 100
        Usd(10m).Allocate(0.5m, 0.25m, 0.25m).ShouldBe([Usd(5m), Usd(2.5m), Usd(2.5m)]);
        Usd(10m).Allocate(2, 1, 1).ShouldBe([Usd(5m), Usd(2.5m), Usd(2.5m)]);
    }

    [TestMethod]
    public void RatiosLargestRemainderTieBreaksToEarlierPart()
    {
        // Shares are 0.005, 0.005, 0.005 => floors 0, 0, 0 with 1 unit left over, so the first part gets it
        Usd(0.01m).Allocate(1, 1, 1).ShouldBe([Usd(0.01m), Usd(0m), Usd(0m)]);

        // Shares: 1.5, 1.5 units (0.015 each) => floors 1, 1 with 1 left over => first part
        Usd(0.03m).Allocate(1, 1).ShouldBe([Usd(0.02m), Usd(0.01m)]);
    }

    [TestMethod]
    public void ZeroRatiosGetNothing()
    {
        var parts = Usd(100m).Allocate(1, 0, 1);
        parts.ShouldBe([Usd(50m), Usd(0m), Usd(50m)]);

        // Remainder never goes to a zero-ratio part
        Usd(0.01m).Allocate(0, 1, 1).ShouldBe([Usd(0m), Usd(0.01m), Usd(0m)]);
    }

    [TestMethod]
    public void NegativeAmounts()
    {
        var parts = Usd(-100m).Allocate(3);

        parts.ShouldBe([Usd(-33.34m), Usd(-33.33m), Usd(-33.33m)]);
        ShouldSumTo(parts, Usd(-100m));

        Usd(-100m).Allocate(1, 2).ShouldBe([Usd(-33.33m), Usd(-66.67m)]);
    }

    [TestMethod]
    public void TotalIsRoundedToPolicyFirst()
    {
        // 100.005 rounds to 100.00 (to even) before allocation
        var parts = Usd(100.005m).Allocate(2);
        parts.ShouldBe([Usd(50m), Usd(50m)]);

        Usd(100.015m).Allocate(2).ShouldBe([Usd(50.01m), Usd(50.01m)]);
    }

    [TestMethod]
    public void ExplicitPolicy()
    {
        var wholeDollars = new RoundingPolicy(0);

        Usd(100m).Allocate(3, wholeDollars).ShouldBe([Usd(34m), Usd(33m), Usd(33m)]);
        Usd(100.49m).Allocate(3, wholeDollars).ShouldBe([Usd(34m), Usd(33m), Usd(33m)]);
        Usd(100m).Allocate([1, 3], wholeDollars).ShouldBe([Usd(25m), Usd(75m)]);

        var quarters = new RoundingPolicy(2, 25);
        Usd(1m).Allocate(3, quarters).ShouldBe([Usd(0.50m), Usd(0.25m), Usd(0.25m)]);
    }

    [TestMethod]
    public void ToCash()
    {
        var total = new MonetaryValue(100m, Cad);

        total.Allocate(3).ShouldBe([new(33.34m, Cad), new(33.33m, Cad), new(33.33m, Cad)]);

        var cash = total.AllocateToCash(3);
        cash.ShouldBe([new(33.35m, Cad), new(33.35m, Cad), new(33.30m, Cad)]);
        ShouldSumTo(cash, total);

        total.AllocateToCash(1, 1).ShouldBe([new(50m, Cad), new(50m, Cad)]);
        new MonetaryValue(0.07m, Cad).AllocateToCash(2).ShouldBe([new(0.05m, Cad), new(0m, Cad)]); // 0.07 rounds to 0.05 (to even: 1.4 units => 1)
    }

    [TestMethod]
    public void ToCashThrowsWithoutPolicy()
    {
        Should.Throw<NotSupportedException>(() => Usd(100m).AllocateToCash(3));
        Should.Throw<NotSupportedException>(() => Usd(100m).AllocateToCash(1, 1));
    }

    [TestMethod]
    public void DefaultValue()
    {
        MonetaryValue.Default.Allocate(3).ShouldBe([default, default, default]);
        MonetaryValue.Default.Allocate(1, 2).ShouldBe([default, default]);
        MonetaryValue.Default.AllocateToCash(2).ShouldBe([default, default]);
    }

    [TestMethod]
    public void Validation()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Usd(100m).Allocate(0));
        Should.Throw<ArgumentOutOfRangeException>(() => Usd(100m).Allocate(-1));
        Should.Throw<ArgumentOutOfRangeException>(() => MonetaryValue.Default.Allocate(0));
        Should.Throw<ArgumentException>(() => Usd(100m).Allocate(ReadOnlySpan<decimal>.Empty));
        Should.Throw<ArgumentException>(() => Usd(100m).Allocate(1, -1));
        Should.Throw<ArgumentException>(() => Usd(100m).Allocate(0, 0));
        Should.Throw<ArgumentException>(() => MonetaryValue.Default.Allocate(0, 0));
    }

    [TestMethod]
    public void LargeValues()
    {
        var large = Usd(79228162514264337593543950335m / 100); // Near decimal.MaxValue with 2 decimals
        var parts = large.Allocate(7);

        parts.Length.ShouldBe(7);
        ShouldSumTo(parts, large.Round());
    }
}
