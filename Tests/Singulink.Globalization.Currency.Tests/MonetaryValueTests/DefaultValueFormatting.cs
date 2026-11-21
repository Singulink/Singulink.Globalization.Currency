using System.Globalization;
#if NET
using System.Runtime.CompilerServices;
using System.Text;
#endif

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class DefaultValueFormatting
{
    private static readonly string[] CurrencyParts = [string.Empty, "G", "I", "R", "C", "L"];
    private static readonly string[] NumberParts = [string.Empty, "N", "D"];
    private static readonly (string Part, int Decimals)[] DecimalsParts = [(string.Empty, 0), ("*", 0), ("B", 0), ("A", 0), ("B2", 2), ("A3", 3), ("A0", 0), ("A28", 28)];

    [TestMethod]
    [DataRow("en-US")]
    [DataRow("de-DE")]
    [DataRow("fr-FR")]
    [DataRow("ar-SA")]
    [DataRow("tr-TR")]
    [DataRow("hi-IN")]
    [DataRow("kea-CV")]
    [DataRow("pt-CV")]
    public void EveryFormat_WritesZeroThatParsesBackAsDefault(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        string separator = GetDecimalSeparator(culture);

        foreach (string currencyPart in CurrencyParts)
        {
            foreach (string numberPart in NumberParts)
            {
                foreach (var (decimalsPart, decimals) in DecimalsParts)
                {
                    string format = currencyPart + numberPart + decimalsPart;
                    string expected = decimals is 0 ? "0" : "0" + separator + new string('0', decimals);

                    AssertFormats(format, culture, expected);
                }
            }
        }
    }

    [TestMethod]
    public void DecimalSeparator_IsTheCultures()
    {
        var enUS = CultureInfo.GetCultureInfo("en-US");
        var deDE = CultureInfo.GetCultureInfo("de-DE");

        MonetaryValue.Default.ToString(null, enUS).ShouldBe("0");
        MonetaryValue.Default.ToString("B2", enUS).ShouldBe("0.00");
        MonetaryValue.Default.ToString("B2", deDE).ShouldBe("0,00");
        MonetaryValue.Default.ToString("CA3", deDE).ShouldBe("0,000");
        MonetaryValue.Default.ToString("O", deDE).ShouldBe("0");
    }

    [TestMethod]
    public void NoFormatAndNoProvider_UsesTheCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            MonetaryValue.Default.ToString().ShouldBe("0");
            MonetaryValue.Default.ToString("B2").ShouldBe("0,00");
            MonetaryValue.Default.ToString((string?)null, null).ShouldBe("0");
            $"{MonetaryValue.Default}".ShouldBe("0");
            $"{MonetaryValue.Default:IB2}".ShouldBe("0,00");
            MonetaryValue.Parse(MonetaryValue.Default.ToString("B2"), null).ShouldBe(MonetaryValue.Default);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void RoundTripFormat_IsUnchanged()
    {
        foreach (string name in new[] { "en-US", "de-DE", "kea-CV" })
            MonetaryValue.Default.ToString("O", CultureInfo.GetCultureInfo(name)).ShouldBe("0");
    }

    [TestMethod]
    public void TooSmallDestination_ReturnsFalse()
    {
        var deDE = CultureInfo.GetCultureInfo("de-DE");

        MonetaryValue.Default.TryFormat(Span<char>.Empty, out int charsWritten, default, deDE).ShouldBeFalse();
        charsWritten.ShouldBe(0);

        Span<char> chars = stackalloc char[3];
        MonetaryValue.Default.TryFormat(chars, out charsWritten, "B2", deDE).ShouldBeFalse();
        charsWritten.ShouldBe(0);

#if NET
        Span<byte> bytes = stackalloc byte[3];
        bytes.Fill((byte)'x');
        MonetaryValue.Default.TryFormat(bytes, out int bytesWritten, "B2", deDE).ShouldBeFalse();
        bytesWritten.ShouldBe(0);
        bytes.ToArray().ShouldAllBe(b => b == (byte)'x');

        MonetaryValue.Default.TryFormat(Span<byte>.Empty, out bytesWritten, default, deDE).ShouldBeFalse();
        bytesWritten.ShouldBe(0);
#endif
    }

    private static void AssertFormats(string format, CultureInfo culture, string expected)
    {
        var value = MonetaryValue.Default;
        string because = $"format '{format}', culture {culture.Name}";

        value.ToString(format, culture).ShouldBe(expected, because);
        string.Format(culture, "{0:" + format + "}", value).ShouldBe(expected, because);

        Span<char> chars = stackalloc char[64];
        value.TryFormat(chars, out int charsWritten, format.AsSpan(), culture).ShouldBeTrue(because);
        chars[..charsWritten].ToString().ShouldBe(expected, because);

        value.TryFormat(chars[..expected.Length], out charsWritten, format.AsSpan(), culture).ShouldBeTrue(because);
        charsWritten.ShouldBe(expected.Length, because);

        value.TryFormat(chars[..(expected.Length - 1)], out charsWritten, format.AsSpan(), culture).ShouldBeFalse(because);
        charsWritten.ShouldBe(0, because);

#if NET
        Span<byte> bytes = stackalloc byte[128];
        value.TryFormat(bytes, out int bytesWritten, format, culture).ShouldBeTrue(because);
        Encoding.UTF8.GetString(bytes[..bytesWritten]).ShouldBe(expected, because);

        var handler = new DefaultInterpolatedStringHandler(0, 1, culture);
        handler.AppendFormatted(value, format);
        handler.ToStringAndClear().ShouldBe(expected, because);
#endif

        MonetaryValue.Parse(expected, culture).ShouldBe(MonetaryValue.Default, because);
        MonetaryValue.TryParse(expected, MonetaryStyles.CurrencyCode, culture, out var parsed).ShouldBeTrue(because);
        parsed.ShouldBe(MonetaryValue.Default, because);
    }

    private static string GetDecimalSeparator(CultureInfo culture)
    {
        // A culture whose currency decimal separator is "$" (kea-CV, pt-CV) writes numbers next to a currency code with its number decimal separator.
        var info = culture.NumberFormat;
        return info.CurrencyDecimalSeparator is "$" ? info.NumberDecimalSeparator : info.CurrencyDecimalSeparator;
    }
}
