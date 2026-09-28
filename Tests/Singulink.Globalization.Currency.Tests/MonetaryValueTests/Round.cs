namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Round
{
    private static readonly MonetaryValue RoundDownResult = new(10, "USD");
    private static readonly MonetaryValue RoundDownValue = new(10.004m, "USD");
    private static readonly MonetaryValue MidpointValue = new(10.005m, "USD");
    private static readonly MonetaryValue RoundUpValue = new(10.006m, "USD");
    private static readonly MonetaryValue RoundUpResult = new(10.01m, "USD");

    [TestMethod]
    public void ToEven()
    {
        const MidpointRounding mode = MidpointRounding.ToEven;

        RoundDownResult.Round(mode).ShouldBe(RoundDownResult);
        RoundDownValue.Round(mode).ShouldBe(RoundDownResult);
        MidpointValue.Round(mode).ShouldBe(RoundDownResult);
        RoundUpValue.Round(mode).ShouldBe(RoundUpResult);
        RoundUpResult.Round(mode).ShouldBe(RoundUpResult);
        MonetaryValue.Default.Round(mode).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void AwayFromZero()
    {
        const MidpointRounding mode = MidpointRounding.AwayFromZero;

        RoundDownResult.Round(mode).ShouldBe(RoundDownResult);
        RoundDownValue.Round(mode).ShouldBe(RoundDownResult);
        MidpointValue.Round(mode).ShouldBe(RoundUpResult);
        RoundUpValue.Round(mode).ShouldBe(RoundUpResult);
        RoundUpResult.Round(mode).ShouldBe(RoundUpResult);
        MonetaryValue.Default.Round(mode).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Default()
    {
        MidpointValue.Round().ShouldBe(RoundDownResult);
    }
}
