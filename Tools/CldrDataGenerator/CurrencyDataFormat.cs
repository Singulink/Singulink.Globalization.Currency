using System.IO.Compression;
using System.Text;

namespace Singulink.Globalization.Tools;

/// <summary>
/// Writes the binary currency data embedded in the Cldr package. The format is private to the Cldr package and must stay in sync with the reader in
/// Source/Singulink.Globalization.Currency.Cldr/CldrDataReader.cs.
/// </summary>
/// <remarks>
/// <para>
/// Layout (strings are length-prefixed UTF-8 as written by <see cref="BinaryWriter.Write(string)"/>, ints are 7-bit encoded), wrapped in a gzip stream:
/// </para>
/// <code>
/// string  magic                    "SLCURDATA"
/// int     formatMajorVersion       1
/// int     formatMinorVersion       0
/// string  dataVersion              i.e. "48.0.0"
/// int     currencyCount
/// per currency (ordered by code):
///   string code
///   string englishName
///   byte   digits
///   int    rounding
///   byte   cashDigits
///   int    cashRounding
///   byte   status                  1 = current tender, 2 = current non-tender, 3 = historical
/// int     localeCount
/// per locale:
///   string name                    "" for the invariant locale (English symbols), otherwise the locale name, i.e. "fr-CA"
///   int    entryCount
///   per entry:
///     int    currencyIndex
///     byte   kind                  0 = name, 1 = symbol
///     string value
/// </code>
/// </remarks>
internal static class CurrencyDataFormat
{
    public const string Magic = "SLCURDATA";
    public const int FormatMajorVersion = 1;
    public const int FormatMinorVersion = 0;

    public static void Write(string path, string dataVersion, CldrData data)
    {
        using var file = File.Create(path);
        using var gzip = new GZipStream(file, CompressionLevel.SmallestSize);
        using var writer = new BinaryWriter(gzip, Encoding.UTF8);

        writer.Write(Magic);
        writer.Write7BitEncodedInt(FormatMajorVersion);
        writer.Write7BitEncodedInt(FormatMinorVersion);
        writer.Write(dataVersion);

        writer.Write7BitEncodedInt(data.Currencies.Count);

        foreach (var currency in data.Currencies)
        {
            writer.Write(currency.Code);
            writer.Write(currency.EnglishName);
            writer.Write(checked((byte)currency.Fraction.Digits));
            writer.Write7BitEncodedInt(currency.Fraction.Rounding);
            writer.Write(checked((byte)currency.Fraction.CashDigits));
            writer.Write7BitEncodedInt(currency.Fraction.CashRounding);
            writer.Write((byte)currency.Status);
        }

        writer.Write7BitEncodedInt(data.Locales.Count);

        foreach (var locale in data.Locales)
        {
            writer.Write(locale.Name);
            writer.Write7BitEncodedInt(locale.Entries.Count);

            foreach (var entry in locale.Entries)
            {
                writer.Write7BitEncodedInt(entry.CurrencyIndex);
                writer.Write((byte)entry.Kind);
                writer.Write(entry.Value);
            }
        }
    }
}
