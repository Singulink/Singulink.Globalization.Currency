using System.Collections.Immutable;

namespace Singulink.Globalization;

/// <summary>
/// Provides currencies and currency registries built from Unicode Common Locale Data Repository (CLDR) data that is embedded in this package, including
/// localized names and symbols, rounding rules and cash rounding rules for every currency known to CLDR.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the system registry that <see cref="CurrencyRegistry.Default"/> provides out of the box, registries created from CLDR data do not depend on the
/// globalization data of the runtime or operating system, so they behave identically everywhere and include information that system data lacks, such as
/// cash rounding rules and currencies that are not the primary currency of any region.
/// </para>
/// <para>
/// Call <see cref="RegisterAsDefault(CldrCurrencyTypes)"/> at application startup to make <see cref="CurrencyRegistry.Default"/> return a CLDR-based
/// registry, or use <see cref="Registry"/> and <see cref="CreateRegistry(CldrCurrencyTypes)"/> directly.
/// </para>
/// </remarks>
public static partial class CldrCurrencyData
{
    private static readonly Lazy<Loaded> _loaded = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<CurrencyRegistry> _registry = new(() => CreateRegistry(CldrCurrencyTypes.CurrentTender), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets all the currencies known to CLDR, including historical currencies and codes that are not legal tender, ordered by currency code.
    /// </summary>
    public static ImmutableArray<Currency> AllCurrencies => _loaded.Value.Currencies;

    /// <summary>
    /// Gets a registry containing the currencies that are currently legal tender in at least one region. This is equivalent to calling
    /// <see cref="CreateRegistry(CldrCurrencyTypes)"/> with <see cref="CldrCurrencyTypes.CurrentTender"/>, but the registry is created once and cached.
    /// </summary>
    public static CurrencyRegistry Registry => _registry.Value;

    /// <summary>
    /// Creates a new registry containing the specified types of currencies.
    /// </summary>
    /// <param name="types">The types of currencies to include in the registry.</param>
    /// <exception cref="ArgumentException"><paramref name="types"/> does not include any currency types.</exception>
    public static CurrencyRegistry CreateRegistry(CldrCurrencyTypes types)
    {
        if ((types & CldrCurrencyTypes.All) is 0)
            throw new ArgumentException("At least one currency type must be specified.", nameof(types));

        var loaded = _loaded.Value;
        var currencies = new List<Currency>(loaded.Currencies.Length);

        for (int i = 0; i < loaded.Currencies.Length; i++)
        {
            if ((types & loaded.Types[i]) is not 0)
                currencies.Add(loaded.Currencies[i]);
        }

        return new CurrencyRegistry("CLDR", currencies);
    }

    /// <summary>
    /// Makes <see cref="CurrencyRegistry.Default"/> return a registry created from CLDR data containing the specified types of currencies. This method
    /// must be called at application startup before the default registry is first accessed.
    /// </summary>
    /// <param name="types">The types of currencies to include in the default registry.</param>
    /// <exception cref="InvalidOperationException">The default registry has already been created.</exception>
    public static void RegisterAsDefault(CldrCurrencyTypes types = CldrCurrencyTypes.CurrentTender)
    {
        CurrencyRegistry.SetDefault(types is CldrCurrencyTypes.CurrentTender ? static () => Registry : () => CreateRegistry(types));
    }

    /// <summary>
    /// Gets the type of the specified CLDR currency.
    /// </summary>
    /// <exception cref="ArgumentException">The currency was not created from CLDR data.</exception>
    public static CldrCurrencyTypes GetCurrencyType(Currency currency)
    {
        var loaded = _loaded.Value;

        if (!loaded.CurrencyIndexes.TryGetValue(currency, out int index))
            throw new ArgumentException("The currency was not created from CLDR data.", nameof(currency));

        return loaded.Types[index];
    }

    private static Loaded Load()
    {
        var data = CldrDataReader.Read();
        var currencies = ImmutableArray.CreateBuilder<Currency>(data.Currencies.Length);
        var types = new CldrCurrencyTypes[data.Currencies.Length];
        var currencyIndexes = new Dictionary<Currency, int>(data.Currencies.Length);

        // Locale lookups reference currency instances, which need the localizer, so the localizer is created around a lookup that is filled in after
        // the currencies are created.

        var localeLookup = new Dictionary<string, Dictionary<Currency, (string? Name, string? Symbol)>>(data.Locales.Count, StringComparer.OrdinalIgnoreCase);
        var localizer = new CldrCurrencyLocalizer(localeLookup);

        for (int i = 0; i < data.Currencies.Length; i++)
        {
            var info = data.Currencies[i];

            var currency = new Currency(info.Code, localizer) {
                RoundingPolicy = info.RoundingPolicy,
                CashRoundingPolicy = info.CashRoundingPolicy,
            };

            currencies.Add(currency);
            currencyIndexes.Add(currency, i);

            types[i] = info.Status switch {
                CldrCurrencyStatus.CurrentTender => CldrCurrencyTypes.CurrentTender,
                CldrCurrencyStatus.CurrentNonTender => CldrCurrencyTypes.CurrentNonTender,
                _ => CldrCurrencyTypes.Historical,
            };
        }

        foreach (var localeEntries in data.Locales)
        {
            var currencyLookup = new Dictionary<Currency, (string? Name, string? Symbol)>(localeEntries.Value.Length);

            foreach (var entry in localeEntries.Value)
            {
                var currency = currencies[entry.CurrencyIndex];
                currencyLookup.TryGetValue(currency, out var values);

                if (entry.Kind is CldrLocaleEntryKind.Name)
                    values.Name = entry.Value;
                else
                    values.Symbol = entry.Value;

                currencyLookup[currency] = values;
            }

            localeLookup.Add(localeEntries.Key, currencyLookup);
        }

        return new Loaded(currencies.MoveToImmutable(), types, currencyIndexes);
    }

    private sealed record Loaded(ImmutableArray<Currency> Currencies, CldrCurrencyTypes[] Types, Dictionary<Currency, int> CurrencyIndexes);
}
