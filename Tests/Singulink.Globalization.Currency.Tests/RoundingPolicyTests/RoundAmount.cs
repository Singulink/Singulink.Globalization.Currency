namespace Singulink.Globalization.Tests.RoundingPolicyTests;

[PrefixTestClass]
public class RoundAmount
{
    [TestMethod]
    public void DecimalDigitsOnly()
    {
        var policy = new RoundingPolicy(2);

        policy.RoundAmount(10.004m).ShouldBe(10.00m);
        policy.RoundAmount(10.005m).ShouldBe(10.00m);
        policy.RoundAmount(10.005m, MidpointRounding.AwayFromZero).ShouldBe(10.01m);
        policy.RoundAmount(10.006m).ShouldBe(10.01m);
        policy.RoundAmount(-10.006m).ShouldBe(-10.01m);
    }

    [TestMethod]
    public void ZeroDecimalDigits()
    {
        var policy = new RoundingPolicy(0);

        policy.RoundAmount(6.2m).ShouldBe(6m);
        policy.RoundAmount(6.5m).ShouldBe(6m);
        policy.RoundAmount(6.5m, MidpointRounding.AwayFromZero).ShouldBe(7m);
        policy.RoundAmount(6.7m).ShouldBe(7m);
    }

    [TestMethod]
    public void NearestFiveHundredths()
    {
        var policy = new RoundingPolicy(2, 5);

        policy.RoundAmount(1.02m).ShouldBe(1.00m);
        policy.RoundAmount(1.03m).ShouldBe(1.05m);
        policy.RoundAmount(1.025m).ShouldBe(1.00m); // Rounds to 1.02 first (to even), then to nearest 0.05
        policy.RoundAmount(1.075m, MidpointRounding.AwayFromZero).ShouldBe(1.10m);
        policy.RoundAmount(1.125m).ShouldBe(1.10m); // 1.12 (to even) then 1.10
        policy.RoundAmount(1.15m).ShouldBe(1.15m);
        policy.RoundAmount(-1.03m).ShouldBe(-1.05m);
    }

    [TestMethod]
    public void NearestHalfUnit()
    {
        var policy = new RoundingPolicy(2, 50);

        policy.RoundAmount(1.24m).ShouldBe(1.00m);
        policy.RoundAmount(1.26m).ShouldBe(1.50m);
        policy.RoundAmount(1.75m).ShouldBe(2.00m);
        policy.RoundAmount(1.25m).ShouldBe(1.00m); // Midpoint to even
        policy.RoundAmount(1.25m, MidpointRounding.AwayFromZero).ShouldBe(1.50m);
    }

    [TestMethod]
    public void NearestFiveWholeUnits()
    {
        var policy = new RoundingPolicy(0, 5);

        policy.RoundAmount(102m).ShouldBe(100m);
        policy.RoundAmount(103m).ShouldBe(105m);
        policy.RoundAmount(102.5m).ShouldBe(100m); // 102 (to even) then 100
    }

    [TestMethod]
    public void SmallestUnitAmount()
    {
        new RoundingPolicy(2).SmallestUnitAmount.ShouldBe(0.01m);
        new RoundingPolicy(2, 5).SmallestUnitAmount.ShouldBe(0.05m);
        new RoundingPolicy(2, 50).SmallestUnitAmount.ShouldBe(0.50m);
        new RoundingPolicy(0).SmallestUnitAmount.ShouldBe(1m);
        new RoundingPolicy(0, 5).SmallestUnitAmount.ShouldBe(5m);
        new RoundingPolicy(3).SmallestUnitAmount.ShouldBe(0.001m);
    }

    [TestMethod]
    public void ValidationAndCoercion()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new RoundingPolicy(-1));
        Should.Throw<ArgumentOutOfRangeException>(() => new RoundingPolicy(29));
        Should.Throw<ArgumentOutOfRangeException>(() => new RoundingPolicy(2, -1));

        new RoundingPolicy(2, 1).DecimalUnits.ShouldBe(0);
        new RoundingPolicy(2, 1).ShouldBe(new RoundingPolicy(2));
    }

    [TestMethod]
    public void Equality()
    {
        new RoundingPolicy(2, 5).ShouldBe(new RoundingPolicy(2, 5));
        new RoundingPolicy(2, 5).GetHashCode().ShouldBe(new RoundingPolicy(2, 5).GetHashCode());
        new RoundingPolicy(2, 5).ShouldNotBe(new RoundingPolicy(2));
        new RoundingPolicy(2).ShouldNotBe(new RoundingPolicy(3));
        RoundingPolicy.Default.ShouldBe(new RoundingPolicy(2));
    }

    [TestMethod]
    public void ToStringOutput()
    {
        new RoundingPolicy(2).ToString().ShouldBe("0.01");
        new RoundingPolicy(2, 5).ToString().ShouldBe("0.05");
        new RoundingPolicy(0).ToString().ShouldBe("1");
    }
}
