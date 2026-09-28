using System.Globalization;

namespace Singulink.Globalization.Tests.CurrencyRegistryTests;

[PrefixTestClass]
public class ParseMonetaryValue
{
    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");

    [TestMethod]
    public void ParseStringAndSpan()
    {
        var registry = CurrencyRegistry.Default;
        var expected = new MonetaryValue(1234.56m, "USD");

        registry.ParseMonetaryValue("USD 1,234.56", provider: EnUS).ShouldBe(expected);
        registry.ParseMonetaryValue("USD 1,234.56".AsSpan(), provider: EnUS).ShouldBe(expected);
        registry.ParseMonetaryValue("$1,234.56", MonetaryStyles.LocalSymbol, EnUS).ShouldBe(expected);
    }

    [TestMethod]
    public void ParseInvalidThrowsWithMessage()
    {
        var ex = Should.Throw<FormatException>(() => CurrencyRegistry.Default.ParseMonetaryValue("not money", provider: EnUS));
        ex.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void TryParseStringOverloads()
    {
        var registry = CurrencyRegistry.Default;
        var expected = new MonetaryValue(1234.56m, "USD");

        registry.TryParseMonetaryValue("USD 1,234.56", MonetaryStyles.CurrencyCode, EnUS, out var result).ShouldBeTrue();
        result.ShouldBe(expected);

        registry.TryParseMonetaryValue("USD 1,234.56", MonetaryStyles.CurrencyCode, EnUS, out result, out string error).ShouldBeTrue();
        result.ShouldBe(expected);
        error.ShouldBe(string.Empty);

        registry.TryParseMonetaryValue("not money", MonetaryStyles.CurrencyCode, EnUS, out result, out error).ShouldBeFalse();
        result.ShouldBe(MonetaryValue.Default);
        error.ShouldNotBeNullOrWhiteSpace();

        registry.TryParseMonetaryValue(null, MonetaryStyles.CurrencyCode, EnUS, out result).ShouldBeFalse();
    }
}
