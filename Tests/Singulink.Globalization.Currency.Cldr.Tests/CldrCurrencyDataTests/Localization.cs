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
