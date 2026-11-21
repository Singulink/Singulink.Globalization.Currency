using System.Globalization;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class RoundTripFormat
{
    private static readonly string[] ThreadCultures = ["en-US", "de-DE", "fr-FR", "ar-SA", "tr-TR", "hi-IN", "kea-CV"];

    private static readonly Currency Dot = new("AB.", "Dot code");
    private static readonly Currency LeadingDot = new(".AB", "Leading dot code");
    private static readonly Currency Dollars = new("$$$", "Dollar code");
    private static readonly Currency Lower = new("btc", "Lower case code") { RoundingPolicy = new RoundingPolicy(8) };
    private static readonly Currency Long = new("LOYALTY", "Long code") { RoundingPolicy = new RoundingPolicy(0) };
    private static readonly CurrencyRegistry Custom = new("Custom", [Dot, LeadingDot, Dollars, Lower, Long]);

    private static IEnumerable<(MonetaryValue Value, CurrencyRegistry Registry, string Text)> Cases()
    {
        var registry = CurrencyRegistry.Default;
        var bhd = registry["BHD"];

        yield return (new(12.30m, "USD"), registry, "USD 12.30");
        yield return (new(12.3m, "USD"), registry, "USD 12.30");
        yield return (new(12m, "USD"), registry, "USD 12.00");
        yield return (new(12.3000m, "USD"), registry, "USD 12.30");
        yield return (new(12.345m, "USD"), registry, "USD 12.345");
        yield return (new(-1234.5678m, "USD"), registry, "USD -1234.5678");
        yield return (new(-0.50m, "USD"), registry, "USD -0.50");
        yield return (new(0m, "USD"), registry, "USD 0.00");
        yield return (new(0.00m, "USD"), registry, "USD 0.00");
        yield return (new(new decimal(0, 0, 0, true, 2), "USD"), registry, "USD 0.00");
        yield return (new(decimal.MaxValue, "USD"), registry, "USD 79228162514264337593543950335.00");
        yield return (new(decimal.MinValue, "USD"), registry, "USD -79228162514264337593543950335.00");
        yield return (new(0.0000000000000000000000000001m, "USD"), registry, "USD 0.0000000000000000000000000001");
        yield return (new(7.9228162514264337593543950335m, "USD"), registry, "USD 7.9228162514264337593543950335");
        yield return (new(79228162514264337593543950.335m, "USD"), registry, "USD 79228162514264337593543950.335");
        yield return (new(1250m, "JPY"), registry, "JPY 1250");
        yield return (new(1250.00m, "JPY"), registry, "JPY 1250");
        yield return (new(12.5m, "JPY"), registry, "JPY 12.5");
        yield return (new(1.234m, bhd), registry, "BHD 1.234");
        yield return (new(1.2m, bhd), registry, "BHD 1.200");
        yield return (new(1.2345m, bhd), registry, "BHD 1.2345");
        yield return (MonetaryValue.Default, registry, "0");
        yield return (new(-12.5m, Dot), Custom, "AB. -12.50");
        yield return (new(12.5m, LeadingDot), Custom, ".AB 12.50");
        yield return (new(-12.5m, Dollars), Custom, "$$$ -12.50");
        yield return (new(0.00000001m, Lower), Custom, "btc 0.00000001");
        yield return (new(1500m, Long), Custom, "LOYALTY 1500");
    }

    [TestMethod]
    public void Format_WritesRoundTripText_InEveryThreadCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        Span<char> buffer = stackalloc char[100];

        try
        {
            foreach (string name in ThreadCultures)
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

                foreach (var (value, _, expected) in Cases())
                {
                    value.ToString("O").ShouldBe(expected, name);
                    value.ToString("o", CultureInfo.GetCultureInfo("de-DE")).ShouldBe(expected, name);
                    value.ToString("O", CultureInfo.GetCultureInfo(name)).ShouldBe(expected, name);

                    value.TryFormat(buffer, out int written, "O".AsSpan(), CultureInfo.GetCultureInfo("fr-FR")).ShouldBeTrue();
                    buffer[..written].ToString().ShouldBe(expected, name);
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void Text_ReadsBackAsEqualValue_InEveryThreadCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        var invariant = CultureInfo.InvariantCulture;

        try
        {
            foreach (string name in ThreadCultures)
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

                foreach (var (value, registry, _) in Cases())
                {
                    string text = value.ToString("O");

                    ShouldBeEqual(registry.ParseMonetaryValue(text, MonetaryStyles.CurrencyCode, invariant), value, text);
                    ShouldBeEqual(registry.ParseMonetaryValue(text.AsSpan(), MonetaryStyles.CurrencyCode, invariant), value, text);

                    registry.TryParseMonetaryValue(text, MonetaryStyles.CurrencyCode, invariant, out var parsed).ShouldBeTrue(text);
                    ShouldBeEqual(parsed, value, text);

                    registry.TryParseMonetaryValue(text.AsSpan(), MonetaryStyles.CurrencyCode, invariant, out parsed, out string error).ShouldBeTrue(error);
                    ShouldBeEqual(parsed, value, text);

                    if (registry == CurrencyRegistry.Default)
                    {
                        ShouldBeEqual(MonetaryValue.Parse(text, invariant), value, text);
                        ShouldBeEqual(MonetaryValue.Parse(text.AsSpan(), invariant), value, text);
                        MonetaryValue.TryParse(text, MonetaryStyles.CurrencyCode, invariant, out parsed).ShouldBeTrue(text);
                        ShouldBeEqual(parsed, value, text);
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void EqualValues_WriteIdenticalText()
    {
        var equalGroups = new MonetaryValue[][] {
            [new(12.3m, "USD"), new(12.30m, "USD"), new(12.3000m, "USD")],
            [new(0m, "USD"), new(0.00m, "USD"), new(0.0000m, "USD"), new(new decimal(0, 0, 0, true, 3), "USD")],
            [new(1250m, "JPY"), new(1250.0m, "JPY"), new(1250.000m, "JPY")],
            [new(1.2m, "BHD"), new(1.200m, "BHD"), new(1.20000m, "BHD")],
            [new(-0.5m, "USD"), new(-0.50m, "USD"), new(-0.5000m, "USD")],
        };

        foreach (var group in equalGroups)
        {
            string first = group[0].ToString("O");

            foreach (var value in group)
                value.ToString("O").ShouldBe(first);
        }
    }

    [TestMethod]
    public void Decimals_AreTheCurrencysOrMoreWhenNeeded()
    {
        new MonetaryValue(5m, "JPY").ToString("O").ShouldBe("JPY 5");        // 0 digits
        new MonetaryValue(5.25m, "JPY").ToString("O").ShouldBe("JPY 5.25");  // more when the amount needs them
        new MonetaryValue(5m, "BHD").ToString("O").ShouldBe("BHD 5.000");    // 3 digits
        new MonetaryValue(5.0001m, "BHD").ToString("O").ShouldBe("BHD 5.0001");
        new MonetaryValue(5m, "USD").ToString("O").ShouldBe("USD 5.00");
        new MonetaryValue(5.125m, "USD").ToString("O").ShouldBe("USD 5.125");
    }

    [TestMethod]
    [DataRow("USD 12.3")]
    [DataRow("USD 12.30")]
    [DataRow("USD 12.3000")]
    [DataRow("USD 12.300000000")]
    public void Reading_AcceptsAnyNumberOfDecimals(string text)
    {
        MonetaryValue.Parse(text, CultureInfo.InvariantCulture).ShouldBe(new MonetaryValue(12.3m, "USD"));
    }

    [TestMethod]
    [DataRow("O2")]
    [DataRow("ON")]
    [DataRow("OD")]
    [DataRow("OB")]
    [DataRow("OA2")]
    [DataRow("O*")]
    [DataRow("GO")]
    [DataRow("CO")]
    [DataRow("OO")]
    public void CombinedSpecifiers_Throw(string format)
    {
        Should.Throw<FormatException>(() => new MonetaryValue(1m, "USD").ToString(format));
        Should.Throw<FormatException>(() => MonetaryValue.Default.ToString(format));
    }

    [TestMethod]
    public void SmallBuffer_ReturnsFalse()
    {
        var value = new MonetaryValue(12.3000m, "USD");

        value.TryFormat(new char[8], out _, "O".AsSpan(), null).ShouldBeFalse();
        value.TryFormat(new char[9], out int written, "O".AsSpan(), null).ShouldBeTrue();
        written.ShouldBe(9);
        MonetaryValue.Default.TryFormat(Span<char>.Empty, out _, "O".AsSpan(), null).ShouldBeFalse();
    }

    [TestMethod]
    public void ZeroWithoutCurrency_ParsesAsDefault()
    {
        // A value without a currency can only be the default value, so a zero amount without a currency reads as the default value.

        foreach (string text in new[] { "0", "0.00", "-0", "(0)", " 0 " })
        {
            MonetaryValue.Parse(text, CultureInfo.InvariantCulture).ShouldBe(MonetaryValue.Default, text);
            MonetaryValue.TryParse(text, MonetaryStyles.CurrencyCode, CultureInfo.InvariantCulture, out var parsed).ShouldBeTrue(text);
            parsed.IsDefault.ShouldBeTrue();
        }

        MonetaryValue.Parse("0", MonetaryStyles.Any, CultureInfo.GetCultureInfo("en-US")).IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void NonZeroWithoutCurrency_StillThrows()
    {
        Should.Throw<FormatException>(() => MonetaryValue.Parse("10", CultureInfo.InvariantCulture))
            .Message.ShouldBe("The input string '10' does not contain a currency code.");
        Should.Throw<FormatException>(() => MonetaryValue.Parse("-0.01", CultureInfo.InvariantCulture));
        Should.Throw<FormatException>(() => MonetaryValue.Parse(string.Empty, CultureInfo.InvariantCulture));
        MonetaryValue.TryParse(string.Empty, out _).ShouldBeFalse();
    }

    [TestMethod]
    public void Bags_WriteEachValueInRoundTripForm()
    {
        var bag = new SortedMoneyBag { new(12.3m, "USD"), new(-2m, "EUR") };
        bag.ToString("O", CultureInfo.GetCultureInfo("de-DE")).ShouldBe("EUR -2.00, USD 12.30");
    }

    private static void ShouldBeEqual(MonetaryValue actual, MonetaryValue expected, string text)
    {
        actual.ShouldBe(expected, text);
        actual.CurrencyOrDefault.ShouldBeSameAs(expected.CurrencyOrDefault, text);
    }
}
