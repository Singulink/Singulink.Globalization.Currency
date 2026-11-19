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

    [TestMethod]
    public void CodeClashingWithLocalSymbol_ParsesAsCode()
    {
        var registry = CreateClashRegistry();

        registry["USD"].GetLocalizedSymbol(EnUS).ShouldBe("CAD");  // en-US's local currency is USD, and its symbol there is spelled like the CAD code

        foreach (var style in new[] { MonetaryStyles.Any, MonetaryStyles.CurrencyCodeOrLocalSymbol })
        {
            registry.ParseMonetaryValue("CAD 10.00", style, EnUS).Currency.ShouldBeSameAs(registry["CAD"]);
            registry.ParseMonetaryValue("CAD 10.00".AsSpan(), style, EnUS).Currency.ShouldBeSameAs(registry["CAD"]);
            registry.ParseMonetaryValue("10.00 CAD", style, EnUS).Currency.ShouldBeSameAs(registry["CAD"]);

            registry.TryParseMonetaryValue("CAD 10.00", style, EnUS, out var result).ShouldBeTrue();
            result.Currency.ShouldBeSameAs(registry["CAD"]);

            registry.TryParseMonetaryValue("CAD 10.00".AsSpan(), style, EnUS, out result, out string error).ShouldBeTrue();
            result.Currency.ShouldBeSameAs(registry["CAD"]);
            error.ShouldBe(string.Empty);
        }

        // Only the local symbol is allowed, so the text is the local symbol and the value is in USD.
        registry.ParseMonetaryValue("CAD10.00", MonetaryStyles.LocalSymbol, EnUS).Currency.ShouldBeSameAs(registry["USD"]);
    }

    [TestMethod]
    public void DefaultFormat_RoundTripsWhereCodeClashesWithLocalSymbol()
    {
        var registry = CreateClashRegistry();

        foreach (string code in new[] { "USD", "CAD" })
        {
            var value = new MonetaryValue(1234.5m, registry[code]);
            string text = value.ToString(null, EnUS);

            text.ShouldStartWith(code);   // the default format writes the code
            registry.ParseMonetaryValue(text, MonetaryStyles.Any, EnUS).ShouldBe(value);
            registry.ParseMonetaryValue(text, MonetaryStyles.CurrencyCodeOrLocalSymbol, EnUS).ShouldBe(value);
        }
    }

    [TestMethod]
    public void FailedLookups_AreListedInTheOrderTried()
    {
        var registry = CreateClashRegistry();

        registry.TryParseMonetaryValue("ZZZ 10", MonetaryStyles.Any, EnUS, out _, out string error).ShouldBeFalse();
        error.ShouldEndWith("Currency code: CodeNotFound, Local currency symbol: SymbolDoesNotMatch, Unambiguous currency symbol: CodeNotFound");
    }

    [TestMethod]
    public void UnparsableLocalSymbols_ThrowEvenForCode()
    {
        // Allowing the local symbol still checks the culture's symbols first, as it did when the local symbol was tried before the code.

        var registry = CreateClashRegistry("U S");

        Should.Throw<InvalidOperationException>(() => registry.ParseMonetaryValue("CAD 10", MonetaryStyles.CurrencyCodeOrLocalSymbol, EnUS))
            .Message.ShouldStartWith("This currency registry cannot parse currency symbols for culture 'en-US'");

        registry.ParseMonetaryValue("CAD 10", MonetaryStyles.CurrencyCode, EnUS).Currency.ShouldBeSameAs(registry["CAD"]);
    }

    private static CurrencyRegistry CreateClashRegistry(string usdSymbolInEnUS = "CAD")
    {
        var provider = new TestProvider(
            [new CurrencyDataEntry("USD", "US Dollar") { Symbol = "$" }, new CurrencyDataEntry("CAD", "Canadian Dollar") { Symbol = "C$" }],
            [new CurrencyLocalizationEntry("en-US", "USD") { Symbol = usdSymbolInEnUS }]);

        return CurrencyData.Load(provider).Registry;
    }

    private sealed class TestProvider(IEnumerable<CurrencyDataEntry> currencies, IEnumerable<CurrencyLocalizationEntry> localizations) : ICurrencyDataProvider
    {
        public string Name => "Clash";

        public string Version => "1.0";

        public DateTime? ReleaseDate => null;

        public IEnumerable<CurrencyDataEntry> GetCurrencies() => currencies;

        public IEnumerable<CurrencyLocalizationEntry> GetLocalizations() => localizations;
    }
}
