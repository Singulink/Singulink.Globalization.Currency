namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class Localization
{
    private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en");
    private static readonly CultureInfo EnCA = CultureInfo.GetCultureInfo("en-CA");
    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr");
    private static readonly CultureInfo FrCA = CultureInfo.GetCultureInfo("fr-CA");
    private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de");
    private static readonly CultureInfo Ja = CultureInfo.GetCultureInfo("ja");

    private static Currency Get(string code) => CurrencyData.Load(CldrCurrencyData.Provider).Registry[code];

    [TestMethod]
    public void InvariantNamesAreEnglish()
    {
        Get("USD").Name.ShouldBe("US Dollar");
        Get("CAD").Name.ShouldBe("Canadian Dollar");
        Get("EUR").Name.ShouldBe("Euro");
        Get("JPY").Name.ShouldBe("Japanese Yen");
        Get("USD").GetLocalizedName(CultureInfo.InvariantCulture).ShouldBe("US Dollar");
    }

    [TestMethod]
    public void InvariantSymbolsAreEnglish()
    {
        Get("USD").Symbol.ShouldBe("$");
        Get("CAD").Symbol.ShouldBe("CA$");
        Get("EUR").Symbol.ShouldBe("€");
        Get("JPY").Symbol.ShouldBe("¥");
        Get("GBP").Symbol.ShouldBe("£");
    }

    [TestMethod]
    public void SymbolFallsBackToCode()
    {
        // CLDR does not define a symbol for many currencies, so the currency code is used.
        Get("KWD").Symbol.ShouldBe("KWD");
        Get("KWD").GetLocalizedSymbol(FrCA).ShouldBe("KWD");
    }

    [TestMethod]
    public void LocalizedNames()
    {
        Get("CAD").GetLocalizedName(Fr).ShouldBe("dollar canadien");
        Get("CAD").GetLocalizedName(FrCA).ShouldBe("dollar canadien");
        Get("USD").GetLocalizedName(De).ShouldBe("US-Dollar");
        Get("JPY").GetLocalizedName(Ja).ShouldBe("日本円");
    }

    [TestMethod]
    public void LocalizedSymbolsUseRegionalOverrides()
    {
        // In Canada the Canadian dollar is just "$" and the US dollar is "US$"; elsewhere in English it is the other way around.
        Get("CAD").GetLocalizedSymbol(EnCA).ShouldBe("$");
        Get("USD").GetLocalizedSymbol(EnCA).ShouldBe("US$");
        Get("CAD").GetLocalizedSymbol(EnUS).ShouldBe("CA$");
        Get("USD").GetLocalizedSymbol(EnUS).ShouldBe("$");
        Get("CAD").GetLocalizedSymbol(FrCA).ShouldBe("$");
        Get("CAD").GetLocalizedSymbol(Fr).ShouldBe("$CA");
    }

    [TestMethod]
    public void EscudoSymbolIsTheDollarDecimalSeparator()
    {
        // CLDR gives the Cape Verdean escudo a zero-width space as its symbol in the locales that write "$" as the decimal separator of an amount. The
        // symbol is "$" there, as it is with system data, so that an amount is written and parsed in the local form.

        const char Sp = '\u00A0';

        var registry = CurrencyData.Load(CldrCurrencyData.Provider).Registry;
        var cve = registry["CVE"];

        foreach (string name in new[] { "pt-CV", "kea-CV" })
        {
            var culture = CultureInfo.GetCultureInfo(name);
            string gs = culture.NumberFormat.CurrencyGroupSeparator;

            culture.NumberFormat.CurrencyDecimalSeparator.ShouldBe("$", name);
            cve.GetLocalizedSymbol(culture).ShouldBe("$", name);

            new MonetaryValue(1234.56m, cve).ToString("C", culture).ShouldBe($"1{gs}234$56", name);
            new MonetaryValue(1234.56m, cve).ToString("L", culture).ShouldBe($"1{gs}234$56", name);
            new MonetaryValue(-5m, cve).ToString("C", culture).ShouldBe("-5$00", name);
            new MonetaryValue(5m, cve).ToString("C*", culture).ShouldBe("5$00", name);
            new MonetaryValue(1234.56m, cve).ToString("G", culture).ShouldBe($"1{gs}234,56{Sp}CVE", name);

            registry.ParseMonetaryValue("1234$56", MonetaryStyles.LocalSymbol, culture).ShouldBe(new MonetaryValue(1234.56m, cve), name);
            registry.ParseMonetaryValue($"1{gs}234$56", MonetaryStyles.Any, culture).ShouldBe(new MonetaryValue(1234.56m, cve), name);
            registry.ParseMonetaryValue("-5$00", MonetaryStyles.Any, culture).ShouldBe(new MonetaryValue(-5m, cve), name);

            // No other currency has that symbol with CLDR data, so only the escudo is written this way.

            registry.TryGetCurrenciesBySymbol("$", culture, out var currencies).ShouldBeTrue(name);
            currencies.ShouldBe([cve], name);
            new MonetaryValue(5m, registry["USD"]).ToString("C", culture).ShouldBe($"5,00{Sp}US$", name);
        }

        cve.GetLocalizedSymbol(CultureInfo.GetCultureInfo("kea")).ShouldBe("$");
        cve.Symbol.ShouldBe("CVE");
    }

    [TestMethod]
    public void UnknownCultureFallsBackThroughChain()
    {
        // Regional variants that CLDR does not have data for inherit from the language.
        var frXX = CultureInfo.GetCultureInfo("fr-BE");
        Get("CAD").GetLocalizedName(frXX).ShouldBe("dollar canadien");

        // Cultures that do not exist in CLDR at all fall back to English.
        Get("CAD").GetLocalizedName(CultureInfo.InvariantCulture).ShouldBe("Canadian Dollar");
    }

    [TestMethod]
    public void CurrencyToString()
    {
        Get("USD").ToString("L", En).ShouldBe("US Dollar (USD)");
        Get("USD").ToString("S", Fr).ShouldBe("dollar des États-Unis");
    }

    [TestMethod]
    public void MonetaryValueFormatting()
    {
        const char Sp = ' ';

        new MonetaryValue(1234.5m, Get("CAD")).ToString("C", EnCA).ShouldBe("$1,234.50");
        new MonetaryValue(1234.5m, Get("CAD")).ToString("C", EnUS).ShouldBe("CA$1,234.50");
        new MonetaryValue(1234.5m, Get("USD")).ToString(null, EnUS).ShouldBe($"USD{Sp}1,234.50");
    }
}
