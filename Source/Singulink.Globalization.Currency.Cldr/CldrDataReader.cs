using System.IO.Compression;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Reads the embedded binary currency data file. The layout must stay in sync with the writer in Tools/CldrDataGenerator/CldrDataFormat.cs.
/// </summary>
internal static class CldrDataReader
{
    private const string Magic = "SLCLDR1";
    private const string ResourceName = "Singulink.Globalization.Currency.Cldr.CurrencyData.bin";

    public static CldrData Read()
    {
        using var resource = typeof(CldrDataReader).Assembly.GetManifestResourceStream(ResourceName) ??
            throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

        using var gzip = new GZipStream(resource, CompressionMode.Decompress);
        using var reader = new BinaryReader(gzip, Encoding.UTF8);

        if (reader.ReadString() != Magic)
            throw new InvalidDataException("Embedded CLDR currency data is not in the expected format.");

        string version = reader.ReadString();

        if (version != CldrCurrencyData.CldrVersion)
            throw new InvalidDataException($"Embedded CLDR currency data version '{version}' does not match the expected version '{CldrCurrencyData.CldrVersion}'.");

        int currencyCount = Read7BitEncodedInt(reader);
        var currencies = new CldrCurrencyInfo[currencyCount];

        for (int i = 0; i < currencyCount; i++)
        {
            string code = reader.ReadString();
            string englishName = reader.ReadString();
            int digits = reader.ReadByte();
            int rounding = Read7BitEncodedInt(reader);
            int cashDigits = reader.ReadByte();
            int cashRounding = Read7BitEncodedInt(reader);
            var status = (CldrCurrencyStatus)reader.ReadByte();

            currencies[i] = new CldrCurrencyInfo(code, englishName, new RoundingPolicy(digits, rounding), new RoundingPolicy(cashDigits, cashRounding), status);
        }

        int localeCount = Read7BitEncodedInt(reader);
        var locales = new Dictionary<string, CldrLocaleEntry[]>(localeCount, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < localeCount; i++)
        {
            string locale = reader.ReadString();
            int entryCount = Read7BitEncodedInt(reader);
            var entries = new CldrLocaleEntry[entryCount];

            for (int j = 0; j < entryCount; j++)
            {
                int currencyIndex = Read7BitEncodedInt(reader);
                var kind = (CldrLocaleEntryKind)reader.ReadByte();
                string value = reader.ReadString();

                entries[j] = new CldrLocaleEntry(currencyIndex, kind, value);
            }

            locales.Add(locale, entries);
        }

        return new CldrData(currencies, locales);
    }

    private static int Read7BitEncodedInt(BinaryReader reader)
    {
        // BinaryReader.Read7BitEncodedInt is protected on .NET Standard 2.0 so it is reimplemented here.

        int result = 0;
        int shift = 0;

        while (true)
        {
            byte b = reader.ReadByte();
            result |= (b & 0x7F) << shift;

            if ((b & 0x80) is 0)
                return result;

            shift += 7;

            if (shift > 28)
                throw new InvalidDataException("Invalid 7-bit encoded integer.");
        }
    }
}

internal sealed record CldrData(CldrCurrencyInfo[] Currencies, Dictionary<string, CldrLocaleEntry[]> Locales);

internal sealed record CldrCurrencyInfo(string Code, string EnglishName, RoundingPolicy RoundingPolicy, RoundingPolicy CashRoundingPolicy, CldrCurrencyStatus Status);

internal readonly record struct CldrLocaleEntry(int CurrencyIndex, CldrLocaleEntryKind Kind, string Value);

// Values are shared with the generator tool and must stay in sync.

internal enum CldrCurrencyStatus : byte
{
    CurrentTender = 1,
    CurrentNonTender = 2,
    Historical = 3,
}

internal enum CldrLocaleEntryKind : byte
{
    Name = 0,
    Symbol = 1,
}
