using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

/// <summary>
/// The <c>V</c> currency specifier, which writes the amount with no currency code and no symbol, as the culture writes amounts of money.
/// </summary>
[PrefixTestClass]
public class ValueOnlyFormat
{
    private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

    private static readonly CultureInfo EnUS = WithNegativeCurrencyPattern(CultureInfo.GetCultureInfo("en-US"), 1);
    private static readonly CultureInfo DeDE = CultureInfo.GetCultureInfo("de-DE");
    private static readonly CultureInfo KeaCV = CultureInfo.GetCultureInfo("kea-CV"); // "$" is the decimal separator of its own currency

    private static readonly Currency NoDecimals = new("AAA", "No decimals") { RoundingPolicy = new RoundingPolicy(0) };
    private static readonly Currency ThreeDecimals = new("BBB", "Three decimals") { RoundingPolicy = new RoundingPolicy(3) };

    private static CultureInfo WithNegativeCurrencyPattern(CultureInfo culture, int pattern)
    {
        var c = (CultureInfo)culture.Clone();
        c.NumberFormat.CurrencyNegativePattern = pattern;
        return c;
    }

    [TestMethod]
    public void WritesTheAmountAlone()
    {
        MonetaryValue.Create(1234.5m, "USD").ToString("V", EnUS).ShouldBe("1,234.50");
        MonetaryValue.Create(1234.5m, "USD").ToString("v", EnUS).ShouldBe("1,234.50");
        MonetaryValue.Create(1234.5m, "CAD").ToString("V", EnUS).ShouldBe("1,234.50");
        MonetaryValue.Create(0m, "USD").ToString("V", EnUS).ShouldBe("0.00");

        MonetaryValue.Create(1234.5m, "USD").ToString("V", DeDE).ShouldBe("1.234,50");

        CultureInfo.CurrentCulture = DeDE;
        MonetaryValue.Create(1234.5m, "USD").ToString("V").ShouldBe("1.234,50");
    }

    [TestMethod]
    public void DecimalsComeFromTheCurrency()
    {
        new MonetaryValue(1234m, NoDecimals).ToString("V", EnUS).ShouldBe("1,234");
        new MonetaryValue(1234.5m, ThreeDecimals).ToString("V", EnUS).ShouldBe("1,234.500");

        // More decimals than the currency has are kept, as with every other currency specifier.

        MonetaryValue.Create(1.005m, "USD").ToString("V", EnUS).ShouldBe("1.005");
        new MonetaryValue(1234.5m, NoDecimals).ToString("V", EnUS).ShouldBe("1,234.5");
    }

    [TestMethod]
    public void CombinesWithTheNumberAndDecimalsSpecifiers()
    {
        MonetaryValue.Create(1234.5m, "USD").ToString("VD", EnUS).ShouldBe("1234.50");
        MonetaryValue.Create(1234.5m, "USD").ToString("VN", EnUS).ShouldBe("1,234.50");

        MonetaryValue.Create(1000m, "USD").ToString("V*", EnUS).ShouldBe("1,000");
        MonetaryValue.Create(1000.5m, "USD").ToString("V*", EnUS).ShouldBe("1,000.50");

        MonetaryValue.Create(1234.565m, "USD").ToString("VB", EnUS).ShouldBe("1,234.56");
        MonetaryValue.Create(1234.565m, "USD").ToString("VA", EnUS).ShouldBe("1,234.57");
        MonetaryValue.Create(1234.5m, "USD").ToString("VB0", EnUS).ShouldBe("1,234");
        MonetaryValue.Create(1234.5m, "USD").ToString("VA0", EnUS).ShouldBe("1,235");
        MonetaryValue.Create(1234.5m, "USD").ToString("VDB3", EnUS).ShouldBe("1234.500");
    }

    [TestMethod]
    public void NegativeAmount_FollowsTheCurrencyPatternOfTheCulture()
    {
        // The form of each of .NET's 17 currency negative patterns with the symbol and its space taken out.

        string[] expected = [
            "(1.50)", "-1.50", "-1.50", "1.50-", "(1.50)", "-1.50", "1.50-", "1.50-", "-1.50",
            "-1.50", "1.50-", "1.50-", "-1.50", "1.50-", "(1.50)", "(1.50)", "-1.50",
        ];

#if NET
        int count = expected.Length;
#else
        int count = 16; // .NET Framework does not have the last pattern
#endif

        for (int pattern = 0; pattern < count; pattern++)
        {
            var culture = WithNegativeCurrencyPattern(EnUS, pattern);

            MonetaryValue.Create(-1.5m, "USD").ToString("V", culture).ShouldBe(expected[pattern], $"pattern {pattern}");
            MonetaryValue.Create(1.5m, "USD").ToString("V", culture).ShouldBe("1.50", $"pattern {pattern}");
        }
    }

    [TestMethod]
    public void UsesTheCurrencySeparatorsOfTheCulture()
    {
        // A culture whose currency format has other separators than its number format, as de-CH and others have.

        var nfi = (NumberFormatInfo)EnUS.NumberFormat.Clone();
        nfi.CurrencyDecimalSeparator = ",";
        nfi.CurrencyGroupSeparator = ".";

        var value = MonetaryValue.Create(1234.5m, "USD");

        value.ToString("V", nfi).ShouldBe("1.234,50");
        value.Amount.ToString("N2", nfi).ShouldBe("1,234.50");
    }

    [TestMethod]
    public void CultureWithTheDollarDecimalSeparator_WritesItsNumberSeparators()
    {
        // The "$" between the escudos and the centavos is the sign of the currency, so a value without a currency does not have it.

        MonetaryValue.Create(1123.45m, "CVE").ToString("V", KeaCV).ShouldBe($"1{Sp}123,45");
        MonetaryValue.Create(1123.45m, "CVE").ToString("V*", KeaCV).ShouldBe($"1{Sp}123,45");
        MonetaryValue.Create(5m, "CVE").ToString("V*", KeaCV).ShouldBe("5");
        MonetaryValue.Create(1123.45m, "USD").ToString("V", KeaCV).ShouldBe($"1{Sp}123,45");
    }

    [TestMethod]
    public void DefaultValue_IsWrittenAsWithEveryOtherSpecifier()
    {
        MonetaryValue.Default.ToString("V", EnUS).ShouldBe("0");
        MonetaryValue.Default.ToString("VB2", EnUS).ShouldBe("0.00");
    }

    [TestMethod]
    public void DoesNotCombineWithAnotherCurrencySpecifier()
    {
        var value = MonetaryValue.Create(1m, "USD");

        Should.Throw<FormatException>(() => value.ToString("VC", EnUS));
        Should.Throw<FormatException>(() => value.ToString("CV", EnUS));
        Should.Throw<FormatException>(() => value.ToString("VO", EnUS));
        Should.Throw<FormatException>(() => value.ToString("OV", EnUS));
    }

    [TestMethod]
    public void Text_DoesNotParseBack()
    {
        // The text has no currency, so it is not a monetary value.

        string text = MonetaryValue.Create(1234.5m, "USD").ToString("V", EnUS);

        MonetaryValue.TryParse(text, MonetaryStyles.Any, EnUS, out _).ShouldBeFalse();
        decimal.Parse(text, NumberStyles.Currency, EnUS).ShouldBe(1234.5m);
    }
}
