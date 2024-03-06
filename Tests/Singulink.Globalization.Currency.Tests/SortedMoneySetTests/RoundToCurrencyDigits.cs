using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class RoundToCurrencyDigits
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    private static readonly ImmutableSortedMoneySet RoundDownResults = [new(10.000m, "USD"), new(6.0m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundDownValues  = [new(10.004m, "USD"), new(6.2m, "JPY")];
    private static readonly ImmutableSortedMoneySet MidpointValues   = [new(10.005m, "USD"), new(6.5m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundUpValues    = [new(10.006m, "USD"), new(6.7m, "JPY")];
    private static readonly ImmutableSortedMoneySet RoundUpResults   = [new(10.010m, "USD"), new(7.0m, "JPY")];
#pragma warning restore SA1025

    private readonly SortedMoneySet _roundDownResults = RoundDownResults.ToSet();
    private readonly SortedMoneySet _roundDownValues = RoundDownValues.ToSet();
    private readonly SortedMoneySet _midpointValues = MidpointValues.ToSet();
    private readonly SortedMoneySet _roundUpValues = RoundUpValues.ToSet();
    private readonly SortedMoneySet _roundUpResults = RoundUpResults.ToSet();

    [TestMethod]
    public void ToEven()
    {
        const MidpointRounding mode = MidpointRounding.ToEven;

        _roundDownResults.RoundToCurrencyDigits(mode);
        _roundDownValues.RoundToCurrencyDigits(mode);
        _midpointValues.RoundToCurrencyDigits(mode);
        _roundUpValues.RoundToCurrencyDigits(mode);
        _roundUpResults.RoundToCurrencyDigits(mode);

        _roundDownResults.ShouldBe(RoundDownResults);
        _roundDownValues.ShouldBe(RoundDownResults);
        _midpointValues.ShouldBe(RoundDownResults);
        _roundUpValues.ShouldBe(RoundUpResults);
        _roundUpResults.ShouldBe(RoundUpResults);
    }

    [TestMethod]
    public void AwayFromZero()
    {
        const MidpointRounding mode = MidpointRounding.AwayFromZero;

        _roundDownResults.RoundToCurrencyDigits(mode);
        _roundDownValues.RoundToCurrencyDigits(mode);
        _midpointValues.RoundToCurrencyDigits(mode);
        _roundUpValues.RoundToCurrencyDigits(mode);
        _roundUpResults.RoundToCurrencyDigits(mode);

        _roundDownResults.ShouldBe(RoundDownResults);
        _roundDownValues.ShouldBe(RoundDownResults);
        _midpointValues.ShouldBe(RoundUpResults);
        _roundUpValues.ShouldBe(RoundUpResults);
        _roundUpResults.ShouldBe(RoundUpResults);
    }

    [TestMethod]
    public void Default()
    {
        RoundDownResults.RoundToCurrencyDigits().ShouldBe(RoundDownResults);
    }
}