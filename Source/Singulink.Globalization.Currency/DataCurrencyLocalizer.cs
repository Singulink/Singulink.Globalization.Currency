using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Localizes currency names and symbols using data from a currency data provider, walking up the locale fallback chain (i.e. <c>fr-CA</c>, then <c>fr</c>,
/// then the invariant locale, which contains English data) and falling back to the currency code for symbols that the data does not define.
/// </summary>
internal sealed partial class DataCurrencyLocalizer : ICurrencyLocalizer
{
    private const string InvariantLocale = "";

    // Locale name => currency => (name, symbol). Values are null when the locale does not override the parent locale's value.
    private readonly Dictionary<string, Dictionary<Currency, (string? Name, string? Symbol)>> _localeLookup;

    // Culture name => resolved cache. Cultures are a small finite set so this does not grow unbounded.
    private readonly ConcurrentDictionary<string, Cache> _cacheLookup = new(StringComparer.OrdinalIgnoreCase);
    private volatile Cache? _lastCache;

    public DataCurrencyLocalizer(Dictionary<string, Dictionary<Currency, (string? Name, string? Symbol)>> localeLookup)
    {
        _localeLookup = localeLookup;
    }

    public string GetName(Currency currency, CultureInfo culture)
    {
        var cache = GetCache(culture);

        if (!cache.TryGetValue(currency, out var info))
            info = GetAndAddInfo(currency, cache);

        return info.Name;
    }

    public string GetSymbol(Currency currency, CultureInfo culture)
    {
        var cache = GetCache(culture);

        if (!cache.TryGetValue(currency, out var info))
            info = GetAndAddInfo(currency, cache);

        return info.Symbol;
    }

    private (string Name, string Symbol) GetAndAddInfo(Currency currency, Cache cache)
    {
        string? name = null;
        string? symbol = null;
        string locale = cache.CultureName;

        while (true)
        {
            if (_localeLookup.TryGetValue(locale, out var currencyLookup) && currencyLookup.TryGetValue(currency, out var values))
            {
                name ??= values.Name;
                symbol ??= values.Symbol;

                if (name is not null && symbol is not null)
                    break;
            }

            if (locale.Length is 0)
                break;

            int i = locale.LastIndexOf('-');
            locale = i < 0 ? InvariantLocale : locale.Substring(0, i);
        }

        var info = (name ?? currency.CurrencyCode, symbol ?? currency.CurrencyCode);
        cache.TryAdd(currency, info);
        return info;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Cache GetCache(CultureInfo culture)
    {
        var cache = _lastCache;

        if (cache is null || !string.Equals(cache.CultureName, culture.Name, StringComparison.OrdinalIgnoreCase))
            _lastCache = cache = _cacheLookup.GetOrAdd(culture.Name, static name => new Cache(name));

        return cache;
    }

    private sealed partial class Cache(string cultureName) : ConcurrentDictionary<Currency, (string Name, string Symbol)>
    {
        public string CultureName { get; } = cultureName;
    }
}
