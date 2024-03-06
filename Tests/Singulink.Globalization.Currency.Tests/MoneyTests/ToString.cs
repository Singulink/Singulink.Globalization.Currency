using System.Globalization;
using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class ToString
{
    private const char Nbsp = '\u00A0';

    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo FrFR = CultureInfo.GetCultureInfo("fe-FR");
    private static readonly CultureInfo KeaCV = CultureInfo.GetCultureInfo("kea-CV"); // currency symbol $ as decimal separator

    [TestMethod]
    public void DefaultFormat()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(100m, "USD").ToString(null, EnUS).ShouldBe($"USD{Nbsp}100.00");
        Money.Create(-100m, "USD").ToString(null, EnUS).ShouldBe($"USD{Nbsp}(100.00)");
        Money.Create(100m, "JPY").ToString(null, EnUS).ShouldBe($"JPY{Nbsp}100");

        Money.Create(100m, "USD").ToString(null, FrFR).ShouldBe($"100.00{Nbsp}USD");
        Money.Create(-100m, "USD").ToString(null, FrFR).ShouldBe($"(100.00){Nbsp}USD");

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(100m, "USD").ToString().ShouldBe($"100$00{Nbsp}USD");
        Money.Create(100m, "USD").ToString(null, KeaCV).ShouldBe($"100$00{Nbsp}USD");

        Money.Create(100m, "USD").ToString(null, EnUS).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void CultureDependentInternational_NumberWithGroupSeparators_DecimalsWithBankerRounding()
    {
        Money.Create(100m, "USD").ToString("CN$", EnUS).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void InternationalCulture_DigitsWithNoSeparators_NoDecimals()
    {
        Money.Create(1000.001m, "USD").ToString("ID*", EnUS).ShouldBe($"USD{Nbsp}1000.001");
    }

    [TestMethod]
    public void Local_NumberWithGroupSeparators_AwayFromZero()
    {
        Money.Create(1000.005m, "CAD").ToString("LDF", EnUS).ShouldBe($"CAD{Nbsp}1000.01");
    }

    [TestMethod]
    public void SymbolAsDecimalSeparator()
    {
        CultureInfo.CurrentCulture = EnUS;

        Money.Create(123.45m, "CVE").ToString("LN", KeaCV).ShouldBe("123$45");
        Money.Create(123.45m, "CVE").ToString("SN", KeaCV).ShouldBe("123$45");
        Money.Create(123.45m, "USD").ToString("SN", KeaCV).ShouldBe("123$45");
        Money.Create(123.45m, "EUR").ToString("SN", KeaCV).ShouldBe("123€45");
        Money.Create(123.45m, "GBP").ToString("SN", KeaCV).ShouldBe("123£45");

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(123.45m, "CVE").ToString("LN").ShouldBe("123$45");
        Money.Create(123.45m, "CVE").ToString("SN").ShouldBe("123$45");
        Money.Create(123.45m, "USD").ToString("SN").ShouldBe("123$45");
        Money.Create(123.45m, "EUR").ToString("SN").ShouldBe("123€45");
        Money.Create(123.45m, "GBP").ToString("SN").ShouldBe("123£45");
    }

    [TestMethod]
    public void SymbolAsDecimalSeparator_SeperatorOverriden()
    {
        var nfi = (NumberFormatInfo)EnUS.NumberFormat.Clone();
        nfi.CurrencyDecimalSeparator = ".";

        CultureInfo.CurrentCulture = KeaCV;

        Money.Create(123.45m, "CVE").ToString("LN", nfi).ShouldBe("$123.45");
        Money.Create(123.45m, "CVE").ToString("SN", nfi).ShouldBe("$123.45");
        Money.Create(123.45m, "USD").ToString("SN", nfi).ShouldBe("$123.45");
        Money.Create(123.45m, "EUR").ToString("SN", nfi).ShouldBe("€123.45");
        Money.Create(123.45m, "GBP").ToString("SN", nfi).ShouldBe("£123.45");
    }

    [TestMethod]
    public void MaxLength()
    {
        CultureInfo.CurrentCulture = EnUS;
        var nfi = (NumberFormatInfo)EnUS.NumberFormat.Clone();
        nfi.CurrencyGroupSizes = [2];

        var currency = new Currency("Longest Possible Format Values", "$123456789123456789$", "$123456789123456789$", 28);
        var value = Money.Create(decimal.MinValue, currency);

        value.ToString("IN", nfi).ShouldBe($"$123456789123456789${Nbsp}(7,92,28,16,25,14,26,43,37,59,35,43,95,03,35.0000000000000000000000000000)");
    }
}