using System.IO.Compression;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Reads the embedded binary currency data into data provider entries. The layout must stay in sync with the writer in
/// Tools/CldrDataGenerator/CurrencyDataFormat.cs, which documents it. The format is private to this package.
/// </summary>
internal static class CldrDataReader
{
    private const string ResourceName = "Singulink.Globalization.Currency.Cldr.CurrencyData.bin";
    private const string Magic = "SLCURDATA";
    private const int FormatMajorVersion = 1;

    public static (List<CurrencyDataEntry> Currencies, List<CurrencyLocalizationEntry> Localizations) Read()
    {
        using var resource = typeof(CldrDataReader).Assembly.GetManifestResourceStream(ResourceName) ??
            throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");

        using var gzip = new GZipStream(resource, CompressionMode.Decompress);
        using var reader = new BinaryReader(gzip, Encoding.UTF8);

        if (reader.ReadString() != Magic)
            throw new InvalidDataException("Embedded CLDR currency data is not in the expected format.");

        int formatMajorVersion = Read7BitEncodedInt(reader);
        _ = Read7BitEncodedInt(reader); // minor version

        if (formatMajorVersion != FormatMajorVersion)
            throw new InvalidDataException($"Embedded CLDR currency data uses unsupported format version {formatMajorVersion}.");

        string dataVersion = reader.ReadString();

        if (dataVersion != CldrCurrencyData.CldrVersion)
            throw new InvalidDataException($"Embedded CLDR currency data version '{dataVersion}' does not match the expected version '{CldrCurrencyData.CldrVersion}'.");

        int currencyCount = Read7BitEncodedInt(reader);
        var codes = new string[currencyCount];
        var names = new string[currencyCount];
        var digits = new int[currencyCount];
        var rounding = new int[currencyCount];
        var cashDigits = new int[currencyCount];
        var cashRounding = new int[currencyCount];
        var types = new CurrencyTypes[currencyCount];
        var symbols = new string?[currencyCount];

        for (int i = 0; i < currencyCount; i++)
        {
            codes[i] = reader.ReadString();
            names[i] = reader.ReadString();
            digits[i] = reader.ReadByte();
            rounding[i] = Read7BitEncodedInt(reader);
            cashDigits[i] = reader.ReadByte();
            cashRounding[i] = Read7BitEncodedInt(reader);

            types[i] = reader.ReadByte() switch {
                1 => CurrencyTypes.CurrentTender,
                2 => CurrencyTypes.CurrentNonTender,
                3 => CurrencyTypes.Historical,
                var status => throw new InvalidDataException($"Embedded CLDR currency data has an invalid status '{status}' for currency '{codes[i]}'."),
            };
        }

        int localeCount = Read7BitEncodedInt(reader);
        var localizations = new List<CurrencyLocalizationEntry>();

        for (int i = 0; i < localeCount; i++)
        {
            string locale = reader.ReadString();
            int entryCount = Read7BitEncodedInt(reader);
            bool isInvariant = locale.Length is 0;

            for (int j = 0; j < entryCount; j++)
            {
                int currencyIndex = Read7BitEncodedInt(reader);
                byte kind = reader.ReadByte();
                string value = reader.ReadString();

                if (currencyIndex < 0 || currencyIndex >= currencyCount)
                    throw new InvalidDataException($"Embedded CLDR currency data is corrupt (invalid currency index in locale '{locale}').");

                if (isInvariant)
                {
                    // Invariant names are already stored with the currency; invariant symbols become the currency entry's symbol.
                    if (kind is 1)
                        symbols[currencyIndex] = value;
                }
                else
                {
                    localizations.Add(new CurrencyLocalizationEntry(locale, codes[currencyIndex]) {
                        Name = kind is 0 ? value : null,
                        Symbol = kind is 1 ? value : null,
                    });
                }
            }
        }

        var currencies = new List<CurrencyDataEntry>(currencyCount);

        for (int i = 0; i < currencyCount; i++)
        {
            currencies.Add(new CurrencyDataEntry(codes[i], names[i]) {
                Symbol = symbols[i],
                DecimalDigits = digits[i],
                DecimalUnits = rounding[i],
                CashDecimalDigits = cashDigits[i],
                CashDecimalUnits = cashRounding[i],
                Type = types[i],
            });
        }

        return (currencies, localizations);
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
