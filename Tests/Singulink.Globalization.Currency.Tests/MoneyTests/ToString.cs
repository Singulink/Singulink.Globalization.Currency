using System.Globalization;
using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class ToString
{
    private const char Nbsp = '\u00A0';
    private static readonly CultureInfo EnUS = CultureInfo.GetCultureInfo("en-US");

    [TestMethod]
    public void ToString_DefaultFormat()
    {
        CultureInfo.CurrentCulture = EnUS;
        new Money(100m, "USD").ToString(null, null).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void ToString_CultureDependentInternational_NumberWithGroupSeparators_DecimalsWithBankerRounding()
    {
        new Money(100m, "USD").ToString("CN$", EnUS).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void ToString_InternationalCulture_DigitsWithNoSeparators_NoDecimals()
    {
        new Money(1000.001m, "USD").ToString("ID*", EnUS).ShouldBe($"USD{Nbsp}1000.001");
    }

    [TestMethod]
    public void ToString_Local_NumberWithGroupSeparators_AwayFromZero()
    {
        new Money(1000.005m, "CAD").ToString("LDF", EnUS).ShouldBe($"CAD{Nbsp}1000.01");
    }
}