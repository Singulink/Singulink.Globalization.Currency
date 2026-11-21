using System.Text;

namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class Utf8Formatting
{
    private static readonly string[] Cultures = ["en-US", "en-CA", "en-AU", "de-DE", "fr-FR", "fr-CA", "ar-SA", "tr-TR", "hi-IN", "ja-JP", "fa-IR"];

    private static readonly string[] Formats = [string.Empty, "G", "I", "R", "L", "C", "O", "D", "CB", "CA1", "LDB2", "V", "VB1"];

    private static Currency Get(string code) => CurrencyData.Load(CldrCurrencyData.Provider).Registry[code];

    [TestMethod]
    public void Utf8_EqualsEncodedChars_WithCldrSymbols()
    {
        // CLDR symbols include letters next to the number, non-ASCII symbols and spacing that system data does not have.

        MonetaryValue[] values = [
            new(1234.5m, Get("USD")), new(-1234.5m, Get("CAD")), new(1250m, Get("JPY")), new(-12.345m, Get("EUR")), new(99.99m, Get("INR")),
            new(5m, Get("SAR")), MonetaryValue.Default,
        ];

        byte[] buffer = new byte[256];

        foreach (string name in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(name);

            foreach (string format in Formats)
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
