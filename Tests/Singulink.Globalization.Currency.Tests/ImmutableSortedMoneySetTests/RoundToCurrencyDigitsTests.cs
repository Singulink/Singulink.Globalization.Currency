using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class RoundToCurrencyDigitsTests
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    private static readonly ImmutableSortedMoneySet RoundDownResults = [new(10.000m, "USD"), new(6.0m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundDownValues  = [new(10.004m, "USD"), new(6.2m, "JPY")];
    private static readonly ImmutableSortedMoneySet MidpointValues   = [new(10.005m, "USD"), new(6.5m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundUpValues    = [new(10.006m, "USD"), new(6.7m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundUpResults   = [new(10.010m, "USD"), new(7.0m, "JPY")];
#pragma warning restore SA1025

    [TestMethod]
    public void ToEven()
    {
        const MidpointRounding mode = MidpointRounding.ToEven;

        RoundDownResults.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults);
        RoundDownValues.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults);
        MidpointValues.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults);
        RoundUpValues.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults);
        RoundUpResults.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults);
    }

    [TestMethod]
    public void AwayFromZero()
    {
        const MidpointRounding mode = MidpointRounding.AwayFromZero;

        RoundDownResults.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults);
        RoundDownValues.RoundToCurrencyDigits(mode).ShouldBe(RoundDownResults);
        MidpointValues.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults);
        RoundUpValues.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults);
        RoundUpResults.RoundToCurrencyDigits(mode).ShouldBe(RoundUpResults);
    }

    [TestMethod]
    public void Default()
    {
        RoundDownResults.RoundToCurrencyDigits().ShouldBe(RoundDownResults);
    }
}