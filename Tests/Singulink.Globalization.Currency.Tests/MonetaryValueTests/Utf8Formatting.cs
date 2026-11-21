using System.Globalization;
using System.Text;
using System.Text.Unicode;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class Utf8Formatting
{
    public static readonly string[] Cultures = ["en-US", "de-DE", "fr-FR", "ar-SA", "tr-TR", "hi-IN", "kea-CV", "fa-IR", "ja-JP", "en-AU"];

    public static readonly string[] Formats = [string.Empty, "G", "I", "R", "L", "C", "O", "N", "D", "GD", "IN", "CB", "CA1", "R*", "LDB2", "CDA0", "*", "B3", "V", "VD", "V*", "VB1"];

    [TestMethod]
    public void Utf8_EqualsEncodedChars_ForEveryFormatAndCulture()
    {
        MonetaryValue[] values = [
            new(1234.5m, "USD"), new(-1234.5678m, "EUR"), new(1250m, "JPY"), new(-0.5m, "CAD"), new(9876543.21m, "INR"), new(12.345m, "BHD"),
            MonetaryValue.Default,
        ];

        AssertUtf8MatchesChars(values, Cultures, Formats);
    }

    [TestMethod]
    public void DestinationOneByteTooSmall_ReturnsFalseAndWritesNothing()
    {
        var value = new MonetaryValue(-1234.5m, "EUR");

        foreach (string culture in new[] { "fr-FR", "de-DE", "ar-SA" })
        {
            var provider = CultureInfo.GetCultureInfo(culture);
            byte[] expected = Encoding.UTF8.GetBytes(value.ToString("C", provider));

            byte[] tooSmall = new byte[expected.Length - 1];
            Array.Fill(tooSmall, (byte)0xAA);

            value.TryFormat(tooSmall, out int bytesWritten, "C", provider).ShouldBeFalse(culture);
            bytesWritten.ShouldBe(0);
            tooSmall.ShouldAllBe(b => b == 0xAA);

            byte[] exact = new byte[expected.Length];
            value.TryFormat(exact, out bytesWritten, "C", provider).ShouldBeTrue(culture);
            bytesWritten.ShouldBe(expected.Length);
            exact.ShouldBe(expected);
        }
    }

    [TestMethod]
    public void DefaultValue()
    {
        Span<byte> buffer = stackalloc byte[8];

        MonetaryValue.Default.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture).ShouldBeTrue();
        Encoding.UTF8.GetString(buffer[..bytesWritten]).ShouldBe("0");

        MonetaryValue.Default.TryFormat(buffer, out bytesWritten, "B2", CultureInfo.GetCultureInfo("de-DE")).ShouldBeTrue();
        Encoding.UTF8.GetString(buffer[..bytesWritten]).ShouldBe("0,00");

        MonetaryValue.Default.TryFormat(Span<byte>.Empty, out bytesWritten, default, CultureInfo.InvariantCulture).ShouldBeFalse();
        bytesWritten.ShouldBe(0);

        MonetaryValue.Default.TryFormat(buffer, out bytesWritten, "O", null).ShouldBeTrue();
        Encoding.UTF8.GetString(buffer[..bytesWritten]).ShouldBe("0");

        MonetaryValue.Default.TryFormat(Span<byte>.Empty, out bytesWritten, "O", null).ShouldBeFalse();
        bytesWritten.ShouldBe(0);
    }

    [TestMethod]
    public void Utf8TryWrite_WritesInterpolatedValue()
    {
        var previous = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var value = new MonetaryValue(1234.5m, "EUR");

            Span<byte> buffer = stackalloc byte[64];
            Utf8.TryWrite(buffer, $"Total: {value}", out int written).ShouldBeTrue();
            Encoding.UTF8.GetString(buffer[..written]).ShouldBe($"Total: {value}");

            Utf8.TryWrite(buffer, CultureInfo.GetCultureInfo("de-DE"), $"{value:C}!", out written).ShouldBeTrue();
            Encoding.UTF8.GetString(buffer[..written]).ShouldBe(value.ToString("C", CultureInfo.GetCultureInfo("de-DE")) + "!");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public void LongCode_FallsBackAndStillMatches()
    {
        var currency = new Currency(new string('X', 120), "Long code");
        var value = new MonetaryValue(1m, currency);

        byte[] buffer = new byte[300];
        value.TryFormat(buffer, out int written, "I", CultureInfo.InvariantCulture).ShouldBeTrue();
        Encoding.UTF8.GetString(buffer, 0, written).ShouldBe(value.ToString("I", CultureInfo.InvariantCulture));
    }

    internal static void AssertUtf8MatchesChars(IEnumerable<MonetaryValue> values, IEnumerable<string> cultures, IEnumerable<string> formats)
    {
        byte[] buffer = new byte[256];

        foreach (string name in cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);

            foreach (string format in formats)
            {
                foreach (var value in values)
                {
                    string text = value.ToString(format, culture);

                    value.TryFormat(buffer, out int bytesWritten, format, culture).ShouldBeTrue($"{name} '{format}'");
                    buffer.AsSpan(0, bytesWritten).ToArray().ShouldBe(Encoding.UTF8.GetBytes(text), $"{name} '{format}' {text}");
                }
            }
        }
    }
}
