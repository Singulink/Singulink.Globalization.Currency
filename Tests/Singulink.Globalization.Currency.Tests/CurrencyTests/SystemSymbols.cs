using System.Globalization;

namespace Singulink.Globalization.Tests.CurrencyTests;

/// <summary>
/// The symbols that the system registry, which is the default registry in this test project, takes from the operating system's culture data. The data
/// differs between systems, so these tests pin the results that must be the same everywhere.
/// </summary>
[PrefixTestClass]
public class SystemSymbols
{
    [TestMethod]
    [DataRow("EUR", "€")]
    [DataRow("RUB", "₽")]
    [DataRow("NGN", "₦")]
    [DataRow("PLN", "zł")]
    [DataRow("UAH", "₴")]
    public void InvariantSymbol_IsTheSymbolWhenOneCultureReportsTheCode(string code, string symbol)
    {
        // For each of these currencies, some version of the system's data has a culture that reports the currency code as the symbol. One such culture must
        // not replace the symbol that the other cultures report: on Linux, the euro's symbol would be "EUR" because of one culture (vec-IT).

        var currency = CurrencyRegistry.Default[code];

        currency.Symbol.ShouldBe(symbol);
        currency.GetLocalizedSymbol(CultureInfo.InvariantCulture).ShouldBe(symbol);

        // A culture with no data of its own for the currency gets the invariant symbol.
        currency.GetLocalizedSymbol(CultureInfo.GetCultureInfo("ja-JP")).ShouldBe(symbol);
    }

    [TestMethod]
    [DataRow("IRR")]
    [DataRow("SDG")]
    [DataRow("MAD")]
    public void InvariantSymbol_IsNeverAWordInAnotherScript(string code)
    {
        // The symbols that cultures report for these currencies are words in Arabic or Tifinagh script, which do not replace the code as the
        // invariant symbol. The symbol is the code, or a currency sign or Latin letters where a system's data has one: the test goes by the
        // rule and not by one system's data.

        string symbol = CurrencyRegistry.Default[code].Symbol;

        symbol.ShouldNotBeEmpty();

        foreach (char c in symbol)
            (c <= '\u024F' || char.GetUnicodeCategory(c) is UnicodeCategory.CurrencySymbol).ShouldBeTrue($"'{symbol}' for {code}");
    }
}
