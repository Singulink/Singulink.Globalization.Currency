using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class RoundToDecimals
{
    private const int AwayFromZero = (int)MidpointRounding.AwayFromZero;
    private const int ToEven = (int)MidpointRounding.ToEven;

    [TestMethod]
    [DataRow("1.23456789", 4, ToEven, "1.2346")]
    [DataRow("1.23455", 4, ToEven, "1.2346")]
    [DataRow("1.23445", 4, ToEven, "1.2344")]
    [DataRow("1.23445", 4, AwayFromZero, "1.2345")]
    [DataRow("-1.23445", 4, ToEven, "-1.2344")]
    [DataRow("-1.23445", 4, AwayFromZero, "-1.2345")]
    [DataRow("-1.23456789", 6, ToEven, "-1.234568")]
    [DataRow("2.5", 0, ToEven, "2")]
    [DataRow("2.5", 0, AwayFromZero, "3")]
    [DataRow("-2.5", 0, ToEven, "-2")]
    [DataRow("-2.5", 0, AwayFromZero, "-3")]
    [DataRow("0.125", 2, ToEven, "0.12")]
    [DataRow("0.125", 2, AwayFromZero, "0.13")]
    [DataRow("1.5", 4, ToEven, "1.5")]
    [DataRow("1.5", 28, ToEven, "1.5")]
    [DataRow("123", 2, ToEven, "123")]
    [DataRow("0.0000000000000000000000000001", 28, ToEven, "0.0000000000000000000000000001")]
    [DataRow("0.0000000000000000000000000001", 27, ToEven, "0")]
    [DataRow("0.0000000000000000000000000005", 27, AwayFromZero, "0.000000000000000000000000001")]
    [DataRow("3.3333333333333333333333333333", 28, ToEven, "3.3333333333333333333333333333")]
    [DataRow("3.3333333333333333333333333333", 27, ToEven, "3.333333333333333333333333333")]
    public void RoundsAmount(string amount, int decimals, int mode, string expected)
    {
        var value = new MonetaryValue(Parse(amount), "USD");
        var result = value.Round(decimals, (MidpointRounding)mode);

        result.Amount.ShouldBe(Parse(expected));
        result.Currency.ShouldBeSameAs(value.Currency);
    }

    [TestMethod]
    public void MoreDecimalsThanTheAmountHas_IsUnchanged()
    {
        var value = new MonetaryValue(12.30m, "USD");

        for (int decimals = 2; decimals <= 28; decimals++)
        {
            var result = value.Round(decimals);
            result.ShouldBe(value);
            result.Amount.ToString(CultureInfo.InvariantCulture).ShouldBe("12.30");
        }
    }

    [TestMethod]
    public void IgnoresTheCurrencysDecimals()
    {
        new MonetaryValue(1234.5678m, "JPY").Round(2).ShouldBe(new MonetaryValue(1234.57m, "JPY"));
        new MonetaryValue(0.123456m, "USD").Round(4).ShouldBe(new MonetaryValue(0.1235m, "USD"));
        new MonetaryValue(1.23456m, "KWD").Round(1).ShouldBe(new MonetaryValue(1.2m, "KWD"));
    }

    [TestMethod]
    public void EveryDecimalsInRange_Works()
    {
        var value = new MonetaryValue(10m / 3, "USD");

        for (int decimals = 0; decimals <= 28; decimals++)
            value.Round(decimals).Amount.ShouldBe(Math.Round(10m / 3, decimals));
    }

    [TestMethod]
    public void DefaultValue_StaysDefault()
    {
        MonetaryValue.Default.Round(0).ShouldBe(MonetaryValue.Default);
        MonetaryValue.Default.Round(4, MidpointRounding.AwayFromZero).ShouldBe(MonetaryValue.Default);
        MonetaryValue.Default.Round(28).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(29)]
    [DataRow(int.MinValue)]
    [DataRow(int.MaxValue)]
    public void OutOfRange_Throws(int decimals)
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => new MonetaryValue(1m, "USD").Round(decimals));
        ex.ParamName.ShouldBe("decimals");

        Should.Throw<ArgumentOutOfRangeException>(() => MonetaryValue.Default.Round(decimals)).ParamName.ShouldBe("decimals");
    }

#if NET
    [TestMethod]
    [DataRow("1.23456", "1.2345", "1.2345", "1.2346")]
    [DataRow("-1.23456", "-1.2345", "-1.2346", "-1.2345")]
    [DataRow("1.2345", "1.2345", "1.2345", "1.2345")]
    public void DirectedModes(string amount, string toZero, string toNegativeInfinity, string toPositiveInfinity)
    {
        var value = new MonetaryValue(Parse(amount), "USD");

        value.Round(4, MidpointRounding.ToZero).Amount.ShouldBe(Parse(toZero));
        value.Round(4, MidpointRounding.ToNegativeInfinity).Amount.ShouldBe(Parse(toNegativeInfinity));
        value.Round(4, MidpointRounding.ToPositiveInfinity).Amount.ShouldBe(Parse(toPositiveInfinity));
    }

    [TestMethod]
    public void DirectedModes_MatchTheCurrencyRounding()
    {
        var value = new MonetaryValue(-10.005m, "USD");

        foreach (var mode in Enum.GetValues<MidpointRounding>())
        {
            value.Round(2, mode).ShouldBe(value.Round(mode));
            value.Round(value.Currency.RoundingPolicy, mode).ShouldBe(value.Round(mode));
        }
    }
#else
    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void DirectedModes_ThrowLikeTheCurrencyRounding(int mode)
    {
        // .NET Framework's Math.Round only knows ToEven and AwayFromZero; the existing Round(mode) throws the same way.

        var value = new MonetaryValue(1.23456m, "USD");

        Should.Throw<ArgumentException>(() => value.Round((MidpointRounding)mode));
        Should.Throw<ArgumentException>(() => value.Round(4, (MidpointRounding)mode));
        Should.Throw<ArgumentException>(() => value.Round(new RoundingPolicy(4), (MidpointRounding)mode));
    }
#endif

    [TestMethod]
    public void CallForms_BindToTheIntendedOverload()
    {
        // 10.125 rounds differently to the currency (2 decimals), to 0 decimals and to the 0.05 policy, so each result shows which overload ran.

        var value = new MonetaryValue(10.125m, "USD");

        value.Round().ShouldBe(new MonetaryValue(10.12m, "USD"));                                       // Round(MidpointRounding)
        value.Round(MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(10.13m, "USD"));          // Round(MidpointRounding)
        value.Round(mode: MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(10.13m, "USD"));    // Round(MidpointRounding)
        value.Round(2).ShouldBe(new MonetaryValue(10.12m, "USD"));                                      // Round(int, MidpointRounding)
        value.Round(0).ShouldBe(new MonetaryValue(10m, "USD"));                                         // Round(int, MidpointRounding), not the enum
        value.Round(1).ShouldBe(new MonetaryValue(10.1m, "USD"));                                       // Round(int, MidpointRounding), not the enum
        value.Round(2, MidpointRounding.AwayFromZero).ShouldBe(new MonetaryValue(10.13m, "USD"));       // Round(int, MidpointRounding)
        value.Round(new RoundingPolicy(2, 5)).ShouldBe(new MonetaryValue(10.10m, "USD"));               // Round(RoundingPolicy, MidpointRounding)

        const MidpointRounding mode = MidpointRounding.AwayFromZero;
        value.Round(mode).ShouldBe(new MonetaryValue(10.13m, "USD"));

        MidpointRounding variable = MidpointRounding.ToEven;
        value.Round(variable).ShouldBe(new MonetaryValue(10.12m, "USD"));

        Func<MidpointRounding, MonetaryValue> byMode = value.Round;
        Func<int, MidpointRounding, MonetaryValue> byDecimals = value.Round;
        Func<RoundingPolicy, MidpointRounding, MonetaryValue> byPolicy = value.Round;

        byMode(MidpointRounding.ToEven).ShouldBe(new MonetaryValue(10.12m, "USD"));
        byDecimals(0, MidpointRounding.ToEven).ShouldBe(new MonetaryValue(10m, "USD"));
        byPolicy(new RoundingPolicy(2, 5), MidpointRounding.ToEven).ShouldBe(new MonetaryValue(10.10m, "USD"));
    }

    private static decimal Parse(string amount) => decimal.Parse(amount, NumberStyles.Float, CultureInfo.InvariantCulture);
}
