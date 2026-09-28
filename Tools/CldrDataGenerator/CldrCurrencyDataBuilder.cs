using System.Text.Json;

namespace Singulink.Globalization.Tools;

/// <summary>
/// Reads the CLDR JSON distribution and produces the deduplicated in-memory model that gets written to the binary data file.
/// </summary>
internal static class CldrCurrencyDataBuilder
{
    // Locales are stored under the invariant name ("") when they are the root of the fallback chain. The English locale is used as the invariant data
    // since CLDR's "root" locale does not contain any currency names or symbols.
    private const string InvariantLocale = "";
    private const string EnglishLocale = "en";

    public static CldrData Build(string coreDir, string numbersDir)
    {
        var supplemental = LoadJson(Path.Combine(coreDir, "supplemental", "currencyData.json"))
            .RootElement.GetProperty("supplemental").GetProperty("currencyData");

        var localeData = LoadLocales(Path.Combine(numbersDir, "main"));

        if (!localeData.ContainsKey(EnglishLocale))
            throw new InvalidOperationException("CLDR data does not contain the 'en' locale.");

        var english = localeData[EnglishLocale];
        var statuses = GetStatuses(supplemental.GetProperty("region"));
        var fractions = supplemental.GetProperty("fractions");
        var defaultFraction = ReadFraction(fractions.GetProperty("DEFAULT"), null);

        // Currencies: every currency that has an English display name, ordered by code.

        var currencies = new List<CldrCurrency>();

        foreach (string code in english.Keys.Order(StringComparer.Ordinal))
        {
            if (code.Length is not 3 || !code.All(c => c is >= 'A' and <= 'Z'))
                throw new InvalidOperationException($"Unexpected currency code '{code}' in CLDR data.");

            var fraction = fractions.TryGetProperty(code, out var fractionElement) ? ReadFraction(fractionElement, defaultFraction) : defaultFraction;

            if (!statuses.TryGetValue(code, out var status))
                status = CldrCurrencyStatus.Historical;

            currencies.Add(new CldrCurrency(code, english[code].Name!, fraction, status));
        }

        var currencyIndexes = currencies.Select((c, i) => (c.Code, Index: i)).ToDictionary(x => x.Code, x => x.Index, StringComparer.Ordinal);

        // Locales: store each locale's names and symbols, omitting values that match the value inherited from the parent locale in the fallback chain
        // that the runtime uses (truncating the last subtag until the invariant locale is reached). Symbols that are missing in CLDR fall back to the
        // currency code at runtime, so they are not stored.

        var resolved = new Dictionary<string, Dictionary<string, CldrLocaleValues>>(StringComparer.Ordinal);
        resolved[InvariantLocale] = english;

        foreach (var (locale, values) in localeData)
        {
            if (locale is EnglishLocale or "root")
                continue;

            resolved[locale] = values;
        }

        var locales = new List<CldrLocale>();
        int entryCount = 0;

        foreach (string locale in resolved.Keys.Order(StringComparer.Ordinal))
        {
            var entries = new List<CldrLocaleEntry>();
            string? parent = locale == InvariantLocale ? null : GetParentLocale(locale);

            foreach (var (code, values) in resolved[locale])
            {
                if (!currencyIndexes.TryGetValue(code, out int index))
                    throw new InvalidOperationException($"Locale '{locale}' has currency '{code}' which is not in the English locale.");

                if (values.Name is not null && values.Name != Resolve(parent, code, static v => v.Name))
                    entries.Add(new CldrLocaleEntry(index, CldrLocaleEntryKind.Name, values.Name));

                if (values.Symbol is not null && values.Symbol != Resolve(parent, code, static v => v.Symbol))
                    entries.Add(new CldrLocaleEntry(index, CldrLocaleEntryKind.Symbol, values.Symbol));
            }

            if (entries.Count is 0)
                continue;

            entries.Sort((a, b) => a.CurrencyIndex != b.CurrencyIndex ? a.CurrencyIndex.CompareTo(b.CurrencyIndex) : a.Kind.CompareTo(b.Kind));
            locales.Add(new CldrLocale(locale, entries));
            entryCount += entries.Count;
        }

        return new CldrData(currencies, locales, entryCount);

        string? Resolve(string? locale, string code, Func<CldrLocaleValues, string?> selector)
        {
            while (locale is not null)
            {
                if (resolved.TryGetValue(locale, out var values) && values.TryGetValue(code, out var v) && selector(v) is { } result)
                    return result;

                locale = locale == InvariantLocale ? null : GetParentLocale(locale);
            }

            return null;
        }
    }

    private static string GetParentLocale(string locale)
    {
        int i = locale.LastIndexOf('-');
        return i < 0 ? InvariantLocale : locale[..i];
    }

    private static Dictionary<string, Dictionary<string, CldrLocaleValues>> LoadLocales(string mainDir)
    {
        var result = new Dictionary<string, Dictionary<string, CldrLocaleValues>>(StringComparer.Ordinal);

        foreach (string localeDir in Directory.EnumerateDirectories(mainDir))
        {
            string locale = Path.GetFileName(localeDir);
            string path = Path.Combine(localeDir, "currencies.json");

            if (!File.Exists(path))
                continue;

            var currencies = LoadJson(path).RootElement.GetProperty("main").GetProperty(locale).GetProperty("numbers").GetProperty("currencies");
            var values = new Dictionary<string, CldrLocaleValues>(StringComparer.Ordinal);

            foreach (var currency in currencies.EnumerateObject())
            {
                string? name = currency.Value.TryGetProperty("displayName", out var n) ? n.GetString() : null;
                string? symbol = currency.Value.TryGetProperty("symbol", out var s) ? s.GetString() : null;

                if (name is null && symbol is null)
                    continue;

                values[currency.Name] = new CldrLocaleValues(name, symbol);
            }

            result[locale] = values;
        }

        return result;
    }

    private static Dictionary<string, CldrCurrencyStatus> GetStatuses(JsonElement regions)
    {
        var statuses = new Dictionary<string, CldrCurrencyStatus>(StringComparer.Ordinal);

        foreach (var region in regions.EnumerateObject())
        {
            foreach (var tenure in region.Value.EnumerateArray())
            {
                foreach (var currency in tenure.EnumerateObject())
                {
                    bool isCurrent = !currency.Value.TryGetProperty("_to", out _);
                    bool isTender = !currency.Value.TryGetProperty("_tender", out var tender) || tender.GetString() != "false";

                    var status = !isCurrent ? CldrCurrencyStatus.Historical : isTender ? CldrCurrencyStatus.CurrentTender : CldrCurrencyStatus.CurrentNonTender;

                    // Best status wins: a currency that is current legal tender anywhere is considered current legal tender.
                    if (!statuses.TryGetValue(currency.Name, out var existing) || status < existing)
                        statuses[currency.Name] = status;
                }
            }
        }

        return statuses;
    }

    private static CldrFraction ReadFraction(JsonElement element, CldrFraction? defaults)
    {
        int digits = ReadInt(element, "_digits") ?? defaults?.Digits ?? throw new InvalidOperationException("DEFAULT fraction has no _digits.");
        int rounding = ReadInt(element, "_rounding") ?? defaults?.Rounding ?? 0;
        int cashDigits = ReadInt(element, "_cashDigits") ?? digits;
        int cashRounding = ReadInt(element, "_cashRounding") ?? rounding;

        return new CldrFraction(digits, rounding, cashDigits, cashRounding);

        static int? ReadInt(JsonElement element, string name)
            => element.TryGetProperty(name, out var value) ? int.Parse(value.GetString()!, System.Globalization.CultureInfo.InvariantCulture) : null;
    }

    private static JsonDocument LoadJson(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonDocument.Parse(stream);
    }
}

internal sealed record CldrLocaleValues(string? Name, string? Symbol);

internal sealed record CldrFraction(int Digits, int Rounding, int CashDigits, int CashRounding);

internal sealed record CldrCurrency(string Code, string EnglishName, CldrFraction Fraction, CldrCurrencyStatus Status);

internal sealed record CldrLocaleEntry(int CurrencyIndex, CldrLocaleEntryKind Kind, string Value);

internal sealed record CldrLocale(string Name, List<CldrLocaleEntry> Entries);

internal sealed record CldrData(List<CldrCurrency> Currencies, List<CldrLocale> Locales, int LocaleEntryCount);

// Values are shared with the runtime reader in the Cldr package and must stay in sync.

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
