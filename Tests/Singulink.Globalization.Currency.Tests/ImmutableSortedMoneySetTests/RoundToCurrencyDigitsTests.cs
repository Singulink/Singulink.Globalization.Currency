using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[TestClass]
public class RoundToCurrencyDigitsTests
{
    private static readonly ImmutableSortedMoneySet _roundDownResult = [new Money(10, "USD")];
    private static readonly ImmutableSortedMoneySet _roundDownValue = [new Money(10.004m, "USD")];
    private static readonly ImmutableSortedMoneySet _midpointValue = [new Money(10.005m, "USD")];
    private static readonly ImmutableSortedMoneySet _roundUpValue = [new Money(10.006m, "USD")];
    private static readonly ImmutableSortedMoneySet _roundUpResult = [new Money(10.01m, "USD")];

    [TestMethod]
    public void ToEven()
    {
        const MidpointRounding mode = MidpointRounding.ToEven;
        _roundDownResult.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
        _roundDownValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
        _midpointValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
        _roundUpValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
        _roundUpResult.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
    }

    [TestMethod]
    public void AwayFromZero()
    {
        const MidpointRounding mode = MidpointRounding.AwayFromZero;
        _roundDownResult.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
        _roundDownValue.RoundToCurrencyDigits(mode).ShouldBe(_roundDownResult);
        _midpointValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
        _roundUpValue.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
        _roundUpResult.RoundToCurrencyDigits(mode).ShouldBe(_roundUpResult);
    }

    [TestMethod]
    public void Default()
    {
        _roundDownResult.RoundToCurrencyDigits().ShouldBe(_roundDownResult);
    }
}