using System.Globalization;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[TestClass]
public class ToStringTests
{
    private const char Nbsp = '\u00A0';
    private static readonly CultureInfo _enUS = CultureInfo.GetCultureInfo("en-US");

    [TestMethod]
    public void ToString_DefaultFormat()
    {
        CultureInfo.CurrentCulture = _enUS;
        new Money(100m, "USD").ToString(null, null).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void ToString_CultureDependentInternational_NumberWithGroupSeparators_DecimalsWithBankerRounding()
    {
        new Money(100m, "USD").ToString("CN$", _enUS).ShouldBe($"USD{Nbsp}100.00");
    }

    [TestMethod]
    public void ToString_InternationalCulture_DigitsWithNoSeparators_NoDecimals()
    {
        new Money(1000.001m, "USD").ToString("ID*", _enUS).ShouldBe($"USD{Nbsp}1000.001");
    }

    [TestMethod]
    public void ToString_Local_NumberWithGroupSeparators_AwayFromZero()
    {
        new Money(1000.005m, "CAD").ToString("LDF", _enUS).ShouldBe($"CAD{Nbsp}1000.01");
    }
}