using System.ComponentModel;
using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

/// <summary>
/// Parsing in kea-CV and pt-CV, the cultures whose currency decimal separator is <c>$</c>, the Cape Verdean escudo symbol written inside the amount.
/// </summary>
[PrefixTestClass]
public class ParseDollarDecimalSeparator
{
    private static readonly string[] Cultures = ["kea-CV", "pt-CV"];

    private static readonly TypeConverter ValueConverter = TypeDescriptor.GetConverter(typeof(MonetaryValue));

    [TestMethod]
    public void BothCultures_HaveTheDollarDecimalSeparator()
    {
        foreach (string name in Cultures)
            CultureInfo.GetCultureInfo(name).NumberFormat.CurrencyDecimalSeparator.ShouldBe("$");
    }

    [TestMethod]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE 1,00", "CVE", "1.00")]
    [DataRow(MonetaryStyles.CurrencyCode, "1,00 CVE", "CVE", "1.00")]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE -1,00", "CVE", "-1.00")]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE 1 000,50", "CVE", "1000.50")]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE 0", "CVE", "0")]
    [DataRow(MonetaryStyles.CurrencyCode, "0 CVE", "CVE", "0")]
    [DataRow(MonetaryStyles.CurrencyCode, "USD 1,00", "USD", "1.00")]
    [DataRow(MonetaryStyles.CurrencyCode, "EUR 2,50", "EUR", "2.50")]
    [DataRow(MonetaryStyles.CurrencyCode, "1,00 EUR", "EUR", "1.00")]
    [DataRow(MonetaryStyles.CurrencyCode, " CVE 1,00 ", "CVE", "1.00")]
    [DataRow(MonetaryStyles.LocalSymbol, "1$00", "CVE", "1.00")]
    [DataRow(MonetaryStyles.LocalSymbol, "-1$00", "CVE", "-1.00")]
    [DataRow(MonetaryStyles.LocalSymbol, "1 000$50", "CVE", "1000.50")]
    [DataRow(MonetaryStyles.LocalSymbol, "0$00", "CVE", "0.00")]
    [DataRow(MonetaryStyles.LocalSymbol, "1$", "CVE", "1")]
    [DataRow(MonetaryStyles.LocalSymbol, "$00", "CVE", "0")]
    [DataRow(MonetaryStyles.Any, "CVE 1,00", "CVE", "1.00")]
    [DataRow(MonetaryStyles.Any, "1,00 CVE", "CVE", "1.00")]
    [DataRow(MonetaryStyles.Any, "CVE -1,00", "CVE", "-1.00")]
    [DataRow(MonetaryStyles.Any, "CVE 1 000,50", "CVE", "1000.50")]
    [DataRow(MonetaryStyles.Any, "CVE 0", "CVE", "0")]
    [DataRow(MonetaryStyles.Any, "0 CVE", "CVE", "0")]
    [DataRow(MonetaryStyles.Any, "1$00", "CVE", "1.00")]
    [DataRow(MonetaryStyles.Any, "-1$00", "CVE", "-1.00")]
    [DataRow(MonetaryStyles.Any, "1 000$50", "CVE", "1000.50")]
    [DataRow(MonetaryStyles.Any, "0$00", "CVE", "0.00")]
    [DataRow(MonetaryStyles.Any, "1$", "CVE", "1")]
    [DataRow(MonetaryStyles.Any, "$00", "CVE", "0")]
    [DataRow(MonetaryStyles.Any, "USD 1,00", "USD", "1.00")]
    [DataRow(MonetaryStyles.Any, "EUR 2,50", "EUR", "2.50")]
    [DataRow(MonetaryStyles.Any, "1,00 EUR", "EUR", "1.00")]
    [DataRow(MonetaryStyles.Any, " CVE 1,00 ", "CVE", "1.00")]
    public void TextWithACurrency_ParsesAsBefore(MonetaryStyles style, string text, string code, string amount)
    {
        var expected = new MonetaryValue(decimal.Parse(amount, CultureInfo.InvariantCulture), code);

        foreach (string name in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);
            var result = MonetaryValue.Parse(text, style, culture);

            result.ShouldBe(expected, name);
            result.Amount.ToString(CultureInfo.InvariantCulture).ShouldBe(amount, name);
        }
    }

    [TestMethod]
    [DataRow(MonetaryStyles.CurrencyCode, "-CVE 1,00", "The input string '-CVE 1,00' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "(CVE 1,00)", "The input string '(CVE 1,00)' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE 1.000,50", "The input string 'CVE 1.000,50' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "1$00", "The input string '1$00' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "-1$00", "The input string '-1$00' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "1$00 CVE", "The input string '1$00 CVE' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "CVE 1$00", "The input string 'CVE 1$00' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "1 000$50", "The input string '1 000$50' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "1.000$50", "The input string '1.000$50' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "0$00", "The input string '0$00' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode")]
    [DataRow(MonetaryStyles.CurrencyCode, "1$", "The input string '1$' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: CodeNotFound")]
    [DataRow(MonetaryStyles.CurrencyCode, "$00", "The input string '$00' had a currency indicator '$' that could not be resolved. The following lookups were attempted but failed: Currency code: CodeNotFound")]
    [DataRow(MonetaryStyles.CurrencyCode, "$", "The input string '$' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "1$00$", "The input string '1$00$' was not in a correct format.")]
    [DataRow(MonetaryStyles.CurrencyCode, "USD 1.00", "The input string 'USD 1.00' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE 1,00", "The input string 'CVE 1,00' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "1,00 CVE", "The input string '1,00 CVE' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE -1,00", "The input string 'CVE -1,00' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "-CVE 1,00", "The input string '-CVE 1,00' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "(CVE 1,00)", "The input string '(CVE 1,00)' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE 1.000,50", "The input string 'CVE 1.000,50' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE 1 000,50", "The input string 'CVE 1 000,50' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE 0", "The input string 'CVE 0' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "0 CVE", "The input string '0 CVE' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "1$00 CVE", "The input string '1$00 CVE' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "CVE 1$00", "The input string 'CVE 1$00' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "1.000$50", "The input string '1.000$50' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "$", "The input string '$' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "1$00$", "The input string '1$00$' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "USD 1,00", "The input string 'USD 1,00' had a currency indicator 'USD' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "USD 1.00", "The input string 'USD 1.00' was not in a correct format.")]
    [DataRow(MonetaryStyles.LocalSymbol, "EUR 2,50", "The input string 'EUR 2,50' had a currency indicator 'EUR' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, "1,00 EUR", "The input string '1,00 EUR' had a currency indicator 'EUR' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.LocalSymbol, " CVE 1,00 ", "The input string ' CVE 1,00 ' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Local currency symbol: SymbolDoesNotMatch")]
    [DataRow(MonetaryStyles.Any, "-CVE 1,00", "The input string '-CVE 1,00' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode, Local currency symbol: SymbolDoesNotMatch, Unambiguous currency symbol: CodeNotFound")]
    [DataRow(MonetaryStyles.Any, "(CVE 1,00)", "The input string '(CVE 1,00)' had a currency indicator 'CVE' that could not be resolved. The following lookups were attempted but failed: Currency code: IndicatorIsNotCode, Local currency symbol: SymbolDoesNotMatch, Unambiguous currency symbol: CodeNotFound")]
    [DataRow(MonetaryStyles.Any, "CVE 1.000,50", "The input string 'CVE 1.000,50' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "1$00 CVE", "The input string '1$00 CVE' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "CVE 1$00", "The input string 'CVE 1$00' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "1.000$50", "The input string '1.000$50' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "$", "The input string '$' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "1$00$", "The input string '1$00$' was not in a correct format.")]
    [DataRow(MonetaryStyles.Any, "USD 1.00", "The input string 'USD 1.00' was not in a correct format.")]
    public void InvalidText_FailsAsBefore(MonetaryStyles style, string text, string message)
    {
        foreach (string name in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);

            Should.Throw<FormatException>(() => MonetaryValue.Parse(text, style, culture)).Message.ShouldBe(message, name);
            MonetaryValue.TryParse(text, style, culture, out _).ShouldBeFalse(name);
        }
    }

    [TestMethod]
    [DataRow("0")]
    [DataRow("0,00")]
    [DataRow("-0")]
    [DataRow(" 0 ")]
    [DataRow("(0,00)")]
    public void ZeroWithoutACurrency_IsTheDefaultValue(string text)
    {
        foreach (string name in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);

            MonetaryValue.Parse(text, culture).ShouldBe(MonetaryValue.Default, name);
            MonetaryValue.Parse(text, MonetaryStyles.Any, culture).ShouldBe(MonetaryValue.Default, name);
            MonetaryValue.TryParse(text, culture, out var result).ShouldBeTrue(name);
            result.ShouldBe(MonetaryValue.Default, name);
            ValueConverter.ConvertFrom(null, culture, text).ShouldBe(MonetaryValue.Default, name);
        }
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("1,00")]
    [DataRow("-1,00")]
    [DataRow("(1,00)")]
    public void NonZeroWithoutACurrency_FailsAsInOtherCultures(string text)
    {
        // de-DE also has a comma as its decimal separator, so the same text fails there for the same reason.

        var deDE = CultureInfo.GetCultureInfo("de-DE");
        string expected = Should.Throw<FormatException>(() => MonetaryValue.Parse(text, deDE)).Message;
        expected.ShouldBe($"The input string '{text}' does not contain a currency code.");

        var expectedConverter = Should.Throw<ArgumentException>(() => ValueConverter.ConvertFrom(null, deDE, text));

        foreach (string name in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);

            Should.Throw<FormatException>(() => MonetaryValue.Parse(text, culture)).Message.ShouldBe(expected, name);
            Should.Throw<FormatException>(() => MonetaryValue.Parse(text, MonetaryStyles.Any, culture)).Message.ShouldBe(expected, name);
            MonetaryValue.TryParse(text, culture, out _).ShouldBeFalse(name);

            var converterException = Should.Throw<ArgumentException>(() => ValueConverter.ConvertFrom(null, culture, text));
            converterException.Message.ShouldBe(expectedConverter.Message, name);
            converterException.InnerException.ShouldBeOfType<FormatException>().Message.ShouldBe(expected, name);
        }
    }
}
