using System.Collections.Immutable;
using System.Runtime.CompilerServices;

#if NETSTANDARD
using Singulink.Globalization.Polyfills;
#endif

namespace Singulink.Globalization;

/// <summary>
/// Provides currencies and currency registries loaded from an <see cref="ICurrencyDataProvider"/>, such as the Unicode Common Locale Data Repository (CLDR)
/// data provided by the <c>Singulink.Globalization.Currency.Cldr</c> package.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the system registry that <see cref="CurrencyRegistry.Default"/> provides out of the box, registries created from a data provider do not depend on
/// the globalization data of the runtime or operating system, so they behave identically everywhere and can include information that system data lacks,
/// such as cash rounding rules, localized names and symbols for every locale, and currencies that are not the primary currency of any region.
/// </para>
/// <para>
/// Data is loaded once per data provider instance and cached, so the same <see cref="Currency"/> instances are returned every time a data provider is loaded.
/// Call <see cref="CurrencyRegistry.SetDefault(ICurrencyDataProvider, CurrencyTypes)"/> at application startup to make <see cref="CurrencyRegistry.Default"/>
/// return a registry built from a data provider.
/// </para>
/// </remarks>
public sealed class CurrencyData
{
    private static readonly ConditionalWeakTable<ICurrencyDataProvider, CurrencyData> _cache = new();

    private readonly CurrencyTypes[] _types;
    private readonly Dictionary<Currency, int> _currencyIndexes;
    private CurrencyRegistry? _registry;

    private CurrencyData(ICurrencyDataProvider provider, ImmutableArray<Currency> currencies, CurrencyTypes[] types, Dictionary<Currency, int> currencyIndexes)
    {
        Provider = provider;
        AllCurrencies = currencies;
        _types = types;
        _currencyIndexes = currencyIndexes;
    }

    /// <summary>
    /// Gets the data provider that this data was loaded from.
    /// </summary>
    public ICurrencyDataProvider Provider { get; }

    /// <summary>
    /// Gets all the currencies provided by the data provider, including historical currencies and codes that are not legal tender, ordered by currency code.
    /// </summary>
    public ImmutableArray<Currency> AllCurrencies { get; }

    /// <summary>
    /// Gets a registry containing the currencies that are currently legal tender in at least one region. This is equivalent to calling
    /// <see cref="CreateRegistry(CurrencyTypes)"/> with <see cref="CurrencyTypes.CurrentTender"/>, but the registry is created once and cached.
    /// </summary>
    public CurrencyRegistry Registry => _registry ??= CreateRegistry(CurrencyTypes.CurrentTender);

    /// <summary>
    /// Loads currency data from the specified data provider, or returns previously loaded data if the data provider has already been loaded.
    /// </summary>
    /// <exception cref="InvalidDataException">The data provider returned invalid data, i.e. duplicate currency codes, localizations for unknown currencies
    /// or invalid rounding information.</exception>
    public static CurrencyData Load(ICurrencyDataProvider provider)
    {
        if (_cache.TryGetValue(provider, out var data))
            return data;

        lock (_cache)
        {
            if (_cache.TryGetValue(provider, out data))
                return data;

            data = LoadCore(provider);
            _cache.Add(provider, data);
            return data;
        }
    }

    /// <summary>
    /// Creates a new registry containing the specified types of currencies. The registry is named after the data provider.
    /// </summary>
    /// <param name="types">The types of currencies to include in the registry.</param>
    /// <exception cref="ArgumentException"><paramref name="types"/> does not include any currency types.</exception>
    public CurrencyRegistry CreateRegistry(CurrencyTypes types)
    {
        if ((types & CurrencyTypes.All) is 0)
            throw new ArgumentException("At least one currency type must be specified.", nameof(types));

        var currencies = new List<Currency>(AllCurrencies.Length);

        for (int i = 0; i < AllCurrencies.Length; i++)
        {
            if ((types & _types[i]) is not 0)
                currencies.Add(AllCurrencies[i]);
        }

        return new CurrencyRegistry(Provider.Name, currencies);
    }

    /// <summary>
    /// Gets the type of the specified currency.
    /// </summary>
    /// <exception cref="ArgumentException">The currency does not belong to this data.</exception>
    public CurrencyTypes GetCurrencyType(Currency currency)
    {
        if (!_currencyIndexes.TryGetValue(currency, out int index))
            throw new ArgumentException("The currency does not belong to this currency data.", nameof(currency));

        return _types[index];
    }

    private static CurrencyData LoadCore(ICurrencyDataProvider provider)
    {
        string description = $"{provider.Name} {provider.Version}";

        var entries = provider.GetCurrencies().OrderBy(e => e.CurrencyCode, StringComparer.OrdinalIgnoreCase).ToList();
        var currencies = ImmutableArray.CreateBuilder<Currency>(entries.Count);
        var types = new CurrencyTypes[entries.Count];
        var currencyIndexes = new Dictionary<Currency, int>(entries.Count);
        var currencyLookup = new Dictionary<string, Currency>(entries.Count, StringComparer.OrdinalIgnoreCase);

        // Locale lookups reference currency instances, which need the localizer, so the localizer is created around a lookup that is filled in after
        // the currencies are created. The invariant locale ("") holds the invariant name and symbol from the currency entries.

        var localeLookup = new Dictionary<string, Dictionary<Currency, (string? Name, string? Symbol)>>(StringComparer.OrdinalIgnoreCase);
        var localizer = new DataCurrencyLocalizer(localeLookup);
        var invariantLookup = new Dictionary<Currency, (string? Name, string? Symbol)>(entries.Count);
        localeLookup.Add(string.Empty, invariantLookup);

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (entry.Type is not (CurrencyTypes.CurrentTender or CurrencyTypes.CurrentNonTender or CurrencyTypes.Historical))
                throw new InvalidDataException($"Currency data from '{description}' specifies an invalid type for currency '{entry.CurrencyCode}'.");

            RoundingPolicy roundingPolicy;
            RoundingPolicy cashRoundingPolicy;

            try
            {
                roundingPolicy = new RoundingPolicy(entry.DecimalDigits, entry.DecimalUnits);
                cashRoundingPolicy = new RoundingPolicy(entry.CashDecimalDigits ?? entry.DecimalDigits, entry.CashDecimalUnits ?? entry.DecimalUnits);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new InvalidDataException($"Currency data from '{description}' specifies invalid rounding information for currency '{entry.CurrencyCode}'.", ex);
            }

            var currency = new Currency(entry.CurrencyCode, localizer) {
                RoundingPolicy = roundingPolicy,
                CashRoundingPolicy = cashRoundingPolicy,
            };

            if (!currencyLookup.TryAdd(entry.CurrencyCode, currency))
                throw new InvalidDataException($"Currency data from '{description}' contains multiple currencies with currency code '{entry.CurrencyCode}'.");

            currencies.Add(currency);
            currencyIndexes.Add(currency, i);
            invariantLookup.Add(currency, (entry.Name, entry.Symbol));
            types[i] = entry.Type;
        }

        foreach (var entry in provider.GetLocalizations())
        {
            if (!currencyLookup.TryGetValue(entry.CurrencyCode, out var currency))
                throw new InvalidDataException($"Currency data from '{description}' contains a localization for unknown currency '{entry.CurrencyCode}'.");

            if (entry.Name is null && entry.Symbol is null)
                continue;

            if (!localeLookup.TryGetValue(entry.Locale, out var currencyLookupForLocale))
                localeLookup.Add(entry.Locale, currencyLookupForLocale = []);

            currencyLookupForLocale.TryGetValue(currency, out var values);

            if (entry.Name is not null)
                values.Name = entry.Name;

            if (entry.Symbol is not null)
                values.Symbol = entry.Symbol;

            currencyLookupForLocale[currency] = values;
        }

        return new CurrencyData(provider, currencies.MoveToImmutable(), types, currencyIndexes);
    }
}
