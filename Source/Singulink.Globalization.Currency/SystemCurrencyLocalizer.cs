using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Localizes currency names and symbols using system globalization data, with a final fallback to the currency code itself.
/// </summary>
internal sealed partial class SystemCurrencyLocalizer : ICurrencyLocalizer
{
    private static FrozenDictionary<(Currency Currency, string CultureName), string> _nameLookup =
        FrozenDictionary<(Currency Currency, string CultureName), string>.Empty;
    private static FrozenDictionary<(Currency Currency, string CultureName), string> _symbolLookup =
        FrozenDictionary<(Currency Currency, string CultureName), string>.Empty;

    private readonly ConditionalWeakTable<string, Cache> _cacheLookup = new();
    private volatile Cache? _lastCache;

    private SystemCurrencyLocalizer() { }

    /// <summary>
    /// Gets the singleton instance of the <see cref="SystemCurrencyLocalizer"/> class.
    /// </summary>
    public static SystemCurrencyLocalizer Instance { get; } = new SystemCurrencyLocalizer();

    /// <inheritdoc cref="ICurrencyLocalizer.GetName(Currency, CultureInfo)"/>
    public string GetName(Currency currency, CultureInfo culture)
    {
#if DEBUG
        if (_nameLookup.Count is 0)
            return currency.CurrencyCode;
#endif

        var cache = GetCache(culture);

        if (!cache.TryGetValue(currency, out var info))
            info = GetAndAddInfo(currency, culture, cache);

        return info.Name;
    }

    /// <inheritdoc cref="ICurrencyLocalizer.GetSymbol(Currency, CultureInfo)"/>
    public string GetSymbol(Currency currency, CultureInfo culture)
    {
#if DEBUG
        if (_symbolLookup.Count is 0)
            return currency.CurrencyCode;
#endif
        var cache = GetCache(culture);

        if (!cache.TryGetValue(currency, out var info))
            info = GetAndAddInfo(currency, culture, cache);

        return info.Symbol;
    }

    internal static void InitLookups(
        FrozenDictionary<(Currency Currency, string CultureName), string> nameLookup,
        FrozenDictionary<(Currency Currency, string CultureName), string> symbolLookup)
    {
        Debug.Assert(_nameLookup.Count is 0, "Already initialized");

        _nameLookup = nameLookup;
        _symbolLookup = symbolLookup;
    }

    private static (string Name, string Symbol) GetAndAddInfo(Currency currency, CultureInfo culture, Cache cache)
    {
        string name = GetValueFromLookup(currency, culture, _nameLookup);
        string symbol = GetValueFromLookup(currency, culture, _symbolLookup);
        var info = (name, symbol);

        cache.TryAdd(currency, info);
        return info;
    }

    private static string GetValueFromLookup(Currency currency, CultureInfo culture, FrozenDictionary<(Currency Currency, string CultureName), string> lookup)
    {
        while (true)
        {
            var key = (currency, culture.Name);

            if (lookup.TryGetValue(key, out string result))
                return result;

            if (culture == culture.Parent)
                return currency.CurrencyCode; // Fallback to currency code if no localized name or symbol is found.

            culture = culture.Parent;
        }
    }

    private Cache GetCache(CultureInfo culture)
    {
        var cache = _lastCache;

        if (cache is null || cache.CultureName != culture.Name)
        {
            _lastCache = cache = _cacheLookup.GetValue(culture.Name, static ccn => new Cache(ccn));
        }

        return cache;
    }

    private sealed partial class Cache(string culture) : ConcurrentDictionary<Currency, (string Name, string Symbol)>
    {
        public string CultureName { get; } = culture;
    }
}

