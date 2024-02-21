using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Globalization.Tests.MoneyTests;

[TestClass]
public class ToStringTests
{
    private static readonly CultureInfo _enUS = CultureInfo.GetCultureInfo("en-US");

    [TestMethod]
    public void ToString_DefaultFormat()
    {
        new Money(100m, "USD").ToString(null, null).ShouldBe("USD\u00A0100.00");
    }

    [TestMethod]
    public void ToString_CultureDependentInternational_NumberWithGroupSeparators_DecimalsWithBankerRounding()
    {
        new Money(100m, "USD").ToString("CN$", _enUS).ShouldBe("USD\u00A0100.00");
    }

    [TestMethod]
    public void ToString_InternationalCulture_DigitsWithNoSeparators_NoDecimals()
    {
        new Money(1000.001m, "USD").ToString("ID*", _enUS).ShouldBe("USD\u00A01000.001");
    }

    [TestMethod]
    public void ToString_Local_NumberWithGroupSeparators_AwayFromZero()
    {
        new Money(1000.005m, "CAD").ToString("LDF", _enUS).ShouldBe("CAD\u00A01000.01");
    }
}