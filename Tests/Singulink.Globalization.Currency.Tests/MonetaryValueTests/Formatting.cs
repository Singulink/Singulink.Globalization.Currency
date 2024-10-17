using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Formatting
{
    private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

    private static readonly CultureInfo EnUS = WithNegativeCurrencyPattern(CultureInfo.GetCultureInfo("en-US"), 0);
    private static readonly CultureInfo FrFR = WithNegativeCurrencyPattern(CultureInfo.GetCultureInfo("fe-FR"), 0);
    private static readonly CultureInfo KeaCV = CultureInfo.GetCultureInfo("kea-CV"); // $ as decimal separator, no actual currency symbol

    private static CultureInfo WithNegativeCurrencyPattern(CultureInfo culture, int pattern)
    {
        var c = (CultureInfo)culture.Clone();
        c.NumberFormat.CurrencyNegativePattern = pattern;
        return c;
    }

    [TestMethod]
    public void DefaultFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(1000m, "USD").ToString().ShouldBe($"USD{Sp}1,000.00");
        MonetaryValue.Create(-1000.1234m, "USD").ToString().ShouldBe($"USD{Sp}(1,000.1234)");
        MonetaryValue.Create(1000m, "JPY").ToString(null, EnUS).ShouldBe($"JPY{Sp}1,000");

        MonetaryValue.Create(1000m, "USD").ToString(null, FrFR).ShouldBe($"1,000.00{Sp}USD");
        MonetaryValue.Create(-1000.1234m, "USD").ToString(null, FrFR).ShouldBe($"(1,000.1234){Sp}USD");

        CultureInfo.CurrentCulture = KeaCV;

        MonetaryValue.Create(1000m, "USD").ToString().ShouldBe($"1{Sp}000,00{Sp}USD");
        MonetaryValue.Create(-1000.1234m, "USD").ToString(null, KeaCV).ShouldBe($"-1{Sp}000,1234{Sp}USD");

        MonetaryValue.Create(1000m, "USD").ToString(null, EnUS).ShouldBe($"USD{Sp}1,000.00");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsInternational()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(100m, "USD").ToString().ShouldBe($"USD{Sp}100.00");
        MonetaryValue.Create(100m, "CAD").ToString().ShouldBe($"CAD{Sp}100.00");

        MonetaryValue.Create(100m, "USD").ToString("I").ShouldBe($"USD{Sp}100.00");
        MonetaryValue.Create(100m, "CAD").ToString("I").ShouldBe($"CAD{Sp}100.00");

        MonetaryValue.Create(100m, "USD").ToString("G").ShouldBe($"USD{Sp}100.00");
        MonetaryValue.Create(100m, "CAD").ToString("G").ShouldBe($"CAD{Sp}100.00");

        MonetaryValue.Create(100m, "CAD").ToString("L").ShouldBe($"CAD{Sp}100.00");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsReverseInternational()
    {
        CultureInfo.CurrentCulture = FrFR;

        MonetaryValue.Create(100m, "EUR").ToString().ShouldBe($"100.00{Sp}EUR");
        MonetaryValue.Create(100m, "USD").ToString().ShouldBe($"100.00{Sp}USD");

        MonetaryValue.Create(100m, "EUR").ToString("R").ShouldBe($"100.00{Sp}EUR");
        MonetaryValue.Create(100m, "USD").ToString("R").ShouldBe($"100.00{Sp}USD");

        MonetaryValue.Create(100m, "EUR").ToString("G").ShouldBe($"100.00{Sp}EUR");
        MonetaryValue.Create(100m, "USD").ToString("G").ShouldBe($"100.00{Sp}USD");

        MonetaryValue.Create(100m, "CAD").ToString("L").ShouldBe($"100.00{Sp}CAD");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsSymbol()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(100m, "USD").ToString("C").ShouldBe("$100.00");
        MonetaryValue.Create(100m, "USD").ToString("L").ShouldBe("$100.00");

        MonetaryValue.Create(100m, "EUR").ToString("C", FrFR).ShouldBe("€100.00");
        MonetaryValue.Create(100m, "EUR").ToString("L", FrFR).ShouldBe("€100.00");
    }

    [TestMethod]
    public void CurrencyFormat_Symbol_SymbolAsDecimalSeparatorCulture()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(1123.45m, "CVE").ToString("L", KeaCV).ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "CVE").ToString("C", KeaCV).ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "USD").ToString("C", KeaCV).ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "EUR").ToString("C", KeaCV).ShouldBe($"1{Sp}123,45{Sp}€");
        MonetaryValue.Create(1123.45m, "GBP").ToString("C", KeaCV).ShouldBe($"1{Sp}123,45{Sp}£");

        CultureInfo.CurrentCulture = KeaCV;

        MonetaryValue.Create(1123.45m, "CVE").ToString("L").ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "CVE").ToString("C").ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "USD").ToString("C").ShouldBe($"1{Sp}123$45");
        MonetaryValue.Create(1123.45m, "EUR").ToString("C").ShouldBe($"1{Sp}123,45{Sp}€");
        MonetaryValue.Create(1123.45m, "GBP").ToString("C").ShouldBe($"1{Sp}123,45{Sp}£");
    }

    [TestMethod]
    public void CurrencyFormat_Symbol_OverriddenSymbolAsDecimalSeparatorCulture()
    {
        var nfi = (NumberFormatInfo)KeaCV.NumberFormat.Clone();
        nfi.CurrencyDecimalSeparator = ",";

        CultureInfo.CurrentCulture = KeaCV;

        MonetaryValue.Create(1123.45m, "CVE").ToString("L", nfi).ShouldBe($"1{Sp}123,45{Sp}$");
        MonetaryValue.Create(1123.45m, "CVE").ToString("C", nfi).ShouldBe($"1{Sp}123,45{Sp}$");
        MonetaryValue.Create(1123.45m, "USD").ToString("C", nfi).ShouldBe($"1{Sp}123,45{Sp}$");
        MonetaryValue.Create(1123.45m, "EUR").ToString("C", nfi).ShouldBe($"1{Sp}123,45{Sp}€");
        MonetaryValue.Create(1123.45m, "GBP").ToString("C", nfi).ShouldBe($"1{Sp}123,45{Sp}£");
    }

    [TestMethod]
    public void NumberFormat_NumberWithGroupSeparators()
    {
        CultureInfo.CurrentCulture = FrFR;

        MonetaryValue.Create(1000m, "CAD").ToString().ShouldBe($"1,000.00{Sp}CAD");
        MonetaryValue.Create(1000m, "CAD").ToString("N").ShouldBe($"1,000.00{Sp}CAD");
    }

    [TestMethod]
    public void NumberFormat_DigitsWithNoGroupSeparators()
    {
        CultureInfo.CurrentCulture = FrFR;

        MonetaryValue.Create(1000m, "CAD").ToString("D").ShouldBe($"1000.00{Sp}CAD");
        MonetaryValue.Create(1000m, "CAD").ToString("D").ShouldBe($"1000.00{Sp}CAD");
    }

    [TestMethod]
    public void DecimalFormat_ShortestDecimalPlaces()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(100m, "USD").ToString("*").ShouldBe($"USD{Sp}100");
        MonetaryValue.Create(100.4m, "USD").ToString("*").ShouldBe($"USD{Sp}100.40");
        MonetaryValue.Create(100.4123m, "USD").ToString("*").ShouldBe($"USD{Sp}100.4123");
    }

    [TestMethod]
    public void DecimalFormat_CurrencyBankerRounding()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(100m, "USD").ToString("B").ShouldBe($"USD{Sp}100.00");
        MonetaryValue.Create(100.005m, "USD").ToString("B").ShouldBe($"USD{Sp}100.00");
        MonetaryValue.Create(1000.25m, "CAD").ToString("B1").ShouldBe($"CAD{Sp}1,000.2");
        MonetaryValue.Create(1000.25m, "CAD").ToString("B4").ShouldBe($"CAD{Sp}1,000.2500");
    }

    [TestMethod]
    public void DecimalFormat_AwayFromZeroRounding()
    {
        CultureInfo.CurrentCulture = EnUS;

        MonetaryValue.Create(1000m, "CAD").ToString("A").ShouldBe($"CAD{Sp}1,000.00");
        MonetaryValue.Create(1000.005m, "CAD").ToString("A").ShouldBe($"CAD{Sp}1,000.01");
        MonetaryValue.Create(1000.25m, "CAD").ToString("A1").ShouldBe($"CAD{Sp}1,000.3");
        MonetaryValue.Create(1000.25m, "CAD").ToString("A4").ShouldBe($"CAD{Sp}1,000.2500");
    }

    [TestMethod]
    public void CombinedFormats()
    {
        CultureInfo.CurrentCulture = FrFR;
#if NET
        MonetaryValue.Create(123.456m, "CAD").ToString("CDB1").ShouldBe("CA$123.5");
#else
        MonetaryValue.Create(123.456m, "CAD").ToString("CDB1").ShouldBe("$123.5");
#endif
        MonetaryValue.Create(1234m, "USD").ToString("RA").ShouldBe($"1,234.00{Sp}USD");
        MonetaryValue.Create(1234m, "EUR").ToString("I*").ShouldBe($"EUR{Sp}1,234");
    }

    [TestMethod]
    public void LargeLengthOutput()
    {
        CultureInfo.CurrentCulture = EnUS;

        var nfi = (NumberFormatInfo)EnUS.NumberFormat.Clone();
        nfi.CurrencyGroupSizes = [2];

        var currency = new Currency("ABCDEABCDEABCDEABCDEABCDEABCDEABCDEABCDE", 28, "Long Currency", "ABCDEABCDEABCDEABCDEABCDEABCDEABCDEABCDE");
        var value = MonetaryValue.Create(decimal.MinValue, currency);

        value.ToString("IN", nfi).ShouldBe($"ABCDEABCDEABCDEABCDEABCDEABCDEABCDEABCDE{Sp}(7,92,28,16,25,14,26,43,37,59,35,43,95,03,35.0000000000000000000000000000)");
    }
}