using System.Globalization;
using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class ToString
{
    private const char Sp = '\u00A0'; // The space char that ToString uses (non-breaking space)

    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo FrFR = CultureInfo.GetCultureInfo("fe-FR");
    private static readonly CultureInfo KeaCV = CultureInfo.GetCultureInfo("kea-CV"); // currency symbol $ as decimal separator

    [TestMethod]
    public void DefaultFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(1000m, "USD").ToString().ShouldBe($"USD{Sp}1,000.00");
        Money.Create(-1000.1234m, "USD").ToString().ShouldBe($"USD{Sp}(1,000.1234)");
        Money.Create(1000m, "JPY").ToString(null, EnUS).ShouldBe($"JPY{Sp}1,000");

        Money.Create(1000m, "USD").ToString(null, FrFR).ShouldBe($"1,000.00{Sp}USD");
        Money.Create(-1000.1234m, "USD").ToString(null, FrFR).ShouldBe($"(1,000.1234){Sp}USD");

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(1000m, "USD").ToString().ShouldBe($"1{Sp}000$00{Sp}USD");
        Money.Create(-1000.1234m, "USD").ToString(null, KeaCV).ShouldBe($"(1{Sp}000$1234){Sp}USD");

        Money.Create(1000m, "USD").ToString(null, EnUS).ShouldBe($"USD{Sp}1,000.00");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsInternational()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(100m, "USD").ToString().ShouldBe($"USD{Sp}100.00");
        Money.Create(100m, "CAD").ToString().ShouldBe($"CAD{Sp}100.00");

        Money.Create(100m, "USD").ToString("I").ShouldBe($"USD{Sp}100.00");
        Money.Create(100m, "CAD").ToString("I").ShouldBe($"CAD{Sp}100.00");

        Money.Create(100m, "USD").ToString("C").ShouldBe($"USD{Sp}100.00");
        Money.Create(100m, "CAD").ToString("C").ShouldBe($"CAD{Sp}100.00");

        Money.Create(100m, "CAD").ToString("L").ShouldBe($"CAD{Sp}100.00");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsReverseInternational()
    {
        CultureInfo.CurrentCulture = FrFR;

        Money.Create(100m, "EUR").ToString().ShouldBe($"100.00{Sp}EUR");
        Money.Create(100m, "USD").ToString().ShouldBe($"100.00{Sp}USD");

        Money.Create(100m, "EUR").ToString("R").ShouldBe($"100.00{Sp}EUR");
        Money.Create(100m, "USD").ToString("R").ShouldBe($"100.00{Sp}USD");

        Money.Create(100m, "EUR").ToString("C").ShouldBe($"100.00{Sp}EUR");
        Money.Create(100m, "USD").ToString("C").ShouldBe($"100.00{Sp}USD");

        Money.Create(100m, "CAD").ToString("L").ShouldBe($"100.00{Sp}CAD");
    }

    [TestMethod]
    public void CurrencyFormat_ResultFormatIsSymbol()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(100m, "USD").ToString("S").ShouldBe("$100.00");
        Money.Create(100m, "USD").ToString("L").ShouldBe("$100.00");

        Money.Create(100m, "EUR").ToString("S", FrFR).ShouldBe("€100.00");
        Money.Create(100m, "EUR").ToString("L", FrFR).ShouldBe("€100.00");
    }

    [TestMethod]
    public void CurrencyFormat_Symbol_SymbolAsDecimalSeparatorCulture()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(1123.45m, "CVE").ToString("L", KeaCV).ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "CVE").ToString("S", KeaCV).ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "USD").ToString("S", KeaCV).ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "EUR").ToString("S", KeaCV).ShouldBe($"1{Sp}123€45");
        Money.Create(1123.45m, "GBP").ToString("S", KeaCV).ShouldBe($"1{Sp}123£45");

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(1123.45m, "CVE").ToString("L").ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "CVE").ToString("S").ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "USD").ToString("S").ShouldBe($"1{Sp}123$45");
        Money.Create(1123.45m, "EUR").ToString("S").ShouldBe($"1{Sp}123€45");
        Money.Create(1123.45m, "GBP").ToString("S").ShouldBe($"1{Sp}123£45");
    }

    [TestMethod]
    public void CurrencyFormat_Symbol_OverriddenSymbolAsDecimalSeparatorCulture()
    {
        var nfi = (NumberFormatInfo)KeaCV.NumberFormat.Clone();
        nfi.CurrencyDecimalSeparator = ",";

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(1123.45m, "CVE").ToString("L", nfi).ShouldBe($"$1{Sp}123,45");
        Money.Create(1123.45m, "CVE").ToString("S", nfi).ShouldBe($"$1{Sp}123,45");
        Money.Create(1123.45m, "USD").ToString("S", nfi).ShouldBe($"$1{Sp}123,45");
        Money.Create(1123.45m, "EUR").ToString("S", nfi).ShouldBe($"€1{Sp}123,45");
        Money.Create(1123.45m, "GBP").ToString("S", nfi).ShouldBe($"£1{Sp}123,45");
    }

    [TestMethod]
    public void NumberFormat_NumberWithGroupSeparators()
    {
        CultureInfo.CurrentCulture = FrFR;

        Money.Create(1000m, "CAD").ToString().ShouldBe($"1,000.00{Sp}CAD");
        Money.Create(1000m, "CAD").ToString("N").ShouldBe($"1,000.00{Sp}CAD");
    }

    [TestMethod]
    public void NumberFormat_DigitsWithNoGroupSeparators()
    {
        CultureInfo.CurrentCulture = FrFR;

        Money.Create(1000m, "CAD").ToString("D").ShouldBe($"1000.00{Sp}CAD");
        Money.Create(1000m, "CAD").ToString("D").ShouldBe($"1000.00{Sp}CAD");
    }

    [TestMethod]
    public void DecimalFormat_ShortestDecimalPlaces()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(100m, "USD").ToString("*").ShouldBe($"USD{Sp}100");
        Money.Create(100.4m, "USD").ToString("*").ShouldBe($"USD{Sp}100.40");
        Money.Create(100.4123m, "USD").ToString("*").ShouldBe($"USD{Sp}100.4123");
    }

    [TestMethod]
    public void DecimalFormat_CurrencyBankerRounding()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(100m, "USD").ToString("$").ShouldBe($"USD{Sp}100.00");
        Money.Create(100.005m, "USD").ToString("$").ShouldBe($"USD{Sp}100.00");
        Money.Create(1000.25m, "CAD").ToString("$1").ShouldBe($"CAD{Sp}1,000.2");
        Money.Create(1000.25m, "CAD").ToString("$4").ShouldBe($"CAD{Sp}1,000.2500");
    }

    [TestMethod]
    public void DecimalFormat_FixedAwayFromZeroRounding()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(1000m, "CAD").ToString("F").ShouldBe($"CAD{Sp}1,000.00");
        Money.Create(1000.005m, "CAD").ToString("F").ShouldBe($"CAD{Sp}1,000.01");
        Money.Create(1000.25m, "CAD").ToString("F1").ShouldBe($"CAD{Sp}1,000.3");
        Money.Create(1000.25m, "CAD").ToString("F4").ShouldBe($"CAD{Sp}1,000.2500");
    }

    [TestMethod]
    public void CombinedFormats()
    {
        CultureInfo.CurrentCulture = FrFR;

        Money.Create(123.456m, "CAD").ToString("SD$1").ShouldBe("$123.5");
        Money.Create(1234m, "USD").ToString("NF").ShouldBe($"1,234.00{Sp}USD");
        Money.Create(1234m, "EUR").ToString("I*").ShouldBe($"EUR{Sp}1,234");
    }

    [TestMethod]
    public void MaxLengthOutput()
    {
        CultureInfo.CurrentCulture = EnUS;

        var nfi = (NumberFormatInfo)EnUS.NumberFormat.Clone();
        nfi.CurrencyGroupSizes = [2];

        var currency = new Currency("Longest Possible Format Values", "$123456789123456789$", "$123456789123456789$", 28);
        var value = Money.Create(decimal.MinValue, currency);

        value.ToString("IN", nfi).ShouldBe($"$123456789123456789${Sp}(7,92,28,16,25,14,26,43,37,59,35,43,95,03,35.0000000000000000000000000000)");
    }
}