using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class RoundToCurrencyDigits
{
    private static readonly Money RoundDownResult = new(10, "USD");
    private static readonly Money RoundDownValue = new(10.004m, "USD");
    private static readonly Money MidpointValue = new(10.005m, "USD");
    private static readonly Money RoundUpValue = new(10.006m, "USD");
    private static readonly Money RoundUpResult = new(10.01m, "USD");

    [TestMethod]
    public void ToEven()
    {
        const MidpointRounding mode = MidpointRounding.ToEven;

        RoundDownResult.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        RoundDownValue.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        MidpointValue.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResult);
        RoundUpValue.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        RoundUpResult.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResult);
        Money.Default.RoundToCurrencyDigits(mode).ShouldBe(Money.Default);
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
        Money.Default.RoundToCurrencyDigits(mode).ShouldBe(Money.Default);
    }

    [TestMethod]
    public void Default()
    {
        MidpointValue.RoundToCurrencyDigits().ShouldBe(RoundDownResult);
    }
}