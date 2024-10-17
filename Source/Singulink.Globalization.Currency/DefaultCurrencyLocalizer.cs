using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NETSTANDARD
using Singulink.Globalization.Polyfills;
#endif

namespace Singulink.Globalization;

internal sealed partial class DefaultCurrencyLocalizer : ICurrencyLocalizer
{
    private static FrozenDictionary<(Currency Currency, string CultureName), string> _nameLookup =
        FrozenDictionary<(Currency Currency, string CultureName), string>.Empty;
    private static FrozenDictionary<(Currency Currency, string CultureName), string> _symbolLookup =
        FrozenDictionary<(Currency Currency, string CultureName), string>.Empty;

    private static readonly ConditionalWeakTable<string, Cache> _cacheLookup = new();
    private static volatile Cache? _lastCache;

    private DefaultCurrencyLocalizer() { }

    public static DefaultCurrencyLocalizer Instance { get; } = new DefaultCurrencyLocalizer();

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

            culture = culture.Parent;
        }
    }

    private static Cache GetCache(CultureInfo culture)
    {
        if (culture.Name == "en")
            culture = CultureInfo.InvariantCulture;

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