namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class RoundToCurrencyDigits
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

        RoundDownResult.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        RoundDownValue.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        MidpointValue.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        RoundUpValue.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        RoundUpResult.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        MonetaryValue.Default.RoundToCurrencyDigits(mode).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void AwayFromZero()
    {
        const MidpointRounding mode = MidpointRounding.AwayFromZero;

        RoundDownResult.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        RoundDownValue.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        MidpointValue.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        RoundUpValue.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        RoundUpResult.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        MonetaryValue.Default.RoundToCurrencyDigits(mode).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void Default()
    {
        MidpointValue.RoundToCurrencyDigits().ShouldBe(RoundDownResult);
    }
}