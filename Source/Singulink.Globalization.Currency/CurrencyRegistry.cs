using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Singulink.Collections;

#if NETSTANDARD
using Singulink.Globalization.Polyfills;
#endif

namespace Singulink.Globalization;

/// <summary>
/// Represents a collection of currencies.
/// </summary>
public sealed partial class CurrencyRegistry : IReadOnlySet<Currency>, ISet<Currency>
{
    private static CurrencyRegistry? _default;

    private readonly string _name;
    private readonly HashSet<Currency> _currencies = [];
    private ImmutableArray<Currency> _orderedCurrencies;

    private readonly StringKeyDictionaryLookupSwitcher<Currency> _currencyLookup = new(new(StringComparer.OrdinalIgnoreCase));
    private readonly ConcurrentDictionary<string, (StringKeyListDictionaryLookupSwitcher<Currency> Lookup, string? ParseError)> _localizedSymbolLookups = [];

    private readonly string? _parseCurrencyCodesError;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyRegistry"/> class with the specified name and set of currencies.
    /// </summary>
    /// <exception cref="ArgumentException">The <paramref name="currencies"/> argument had multiple currencies with the same currency code.</exception>
    public CurrencyRegistry(string name, IEnumerable<Currency> currencies)
    {
        _name = name.Trim();

        if (_name.Length is 0)
            throw new ArgumentException("Name is required.", nameof(name));

        foreach (var currency in currencies)
        {
            if (_parseCurrencyCodesError is null)
                Currency.IsSymbolOrCodeParsable(currency.CurrencyCode, out _parseCurrencyCodesError);

            if (_currencies.Add(currency) && !_currencyLookup.Dictionary.TryAdd(currency.CurrencyCode, currency))
                throw new ArgumentException($"Multiple currencies with currency code '{currency.CurrencyCode}'.", nameof(currencies));
        }
    }

    /// <summary>
    /// Gets the default registry built from system globalization data.
    /// </summary>
    public static CurrencyRegistry Default => _default ??= BuildDefaultRegistry();

    /// <summary>
    /// Gets a currency from this registry with the specified currency code.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid or unknown currency code.</exception>
    public Currency this[TargetDependentStringKey currencyCode]
    {
        get {
            if (!TryGetCurrency(currencyCode, out var currency))
                Throw(currencyCode);

            return currency;

            [DoesNotReturn]
            static void Throw(TargetDependentStringKey currencyCode) => throw new ArgumentException($"Currency code '{currencyCode}' not found in the registry.", nameof(currencyCode));
        }
    }

    /// <summary>
    /// Gets the name of this currency registry.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the number of currencies in this registry.
    /// </summary>
    public int Count => _currencies.Count;

    /// <summary>
    /// Gets a currency from this registry with the specified currency code.
    /// </summary>
    public bool TryGetCurrency(TargetDependentStringKey currencyCode, [MaybeNullWhen(false)] out Currency currency)
    {
        return _currencyLookup.TargetDependent.TryGetValue(currencyCode, out currency);
    }

    /// <inheritdoc cref="TryGetCurrenciesBySymbol(TargetDependentStringKey, CultureInfo?, out IReadOnlyList{Currency})"/>
    public bool TryGetCurrenciesBySymbol(TargetDependentStringKey currencySymbol, out IReadOnlyList<Currency> currencies)
        => TryGetCurrenciesBySymbol(currencySymbol, null, out currencies);

    /// <summary>
    /// Gets the currencies in this registry that use the specified currency symbol, ordered by currency code.
    /// </summary>
    /// <param name="currencySymbol">The symbol of the currency to find.</param>
    /// <param name="culture">The culture to use for the localized symbols.</param>
    /// <param name="currencies">When the method returns, contains the currencies if any were found; otherwise an empty collection.</param>
    /// <returns><see langword="true"/> if currencies were found; otherwise <see langword="false"/>.</returns>
    public bool TryGetCurrenciesBySymbol(TargetDependentStringKey currencySymbol, CultureInfo? culture, out IReadOnlyList<Currency> currencies)
    {
        culture ??= CultureInfo.CurrentCulture;
        var (lookup, _) = GetOrBuildCurrenciesBySymbolLookup(culture);

        if (!lookup.TargetDependent.TryGetValues(currencySymbol, out var currencyList))
        {
            currencies = [];
            return false;
        }

        currencies = currencyList.AsTransientReadOnly();
        return true;
    }

    /// <summary>
    /// Gets the local currency for the current culture. Culture must be region-specific for this method to succeed.
    /// </summary>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the culture was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetLocalCurrency([MaybeNullWhen(false)] out Currency currency) => TryGetLocalCurrency(CultureInfo.CurrentCulture, out currency);

    /// <summary>
    /// Gets the local currency for the specified culture. Culture must be region-specific for this method to succeed.
    /// </summary>
    /// <param name="culture">The culture to get a local currency for.</param>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the culture was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetLocalCurrency(CultureInfo culture, [MaybeNullWhen(false)] out Currency currency)
    {
        var region = culture.GetRegionInfo();

        if (region is null)
        {
            currency = null;
            return false;
        }

        return TryGetLocalCurrency(region, out currency);
    }

    /// <summary>
    /// Gets the local currency for the specified region.
    /// </summary>
    /// <param name="region">The region to get a local currency for.</param>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the region was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetLocalCurrency(RegionInfo region, [MaybeNullWhen(false)] out Currency currency) => TryGetCurrency(region.ISOCurrencySymbol, out currency);

    /// <summary>
    /// Gets a value indicating whether this registry contains a currency with the specified currency code.
    /// </summary>
    public bool Contains(TargetDependentStringKey currencyCode) => _currencyLookup.TargetDependent.ContainsKey(currencyCode);

    /// <summary>
    /// Gets a value indicating whether this registry contains the specified currency.
    /// </summary>
    public bool Contains(Currency currency) => _currencies.Contains(currency);

    /// <summary>
    /// Returns an enumerator that iterates through the currencies in this registry, ordered by currency code.
    /// </summary>
    public Enumerator GetEnumerator()
    {
        if (_orderedCurrencies.IsDefault)
        {
            var orderedCurrencies = _currencies.OrderBy(c => c.CurrencyCode, StringComparer.OrdinalIgnoreCase).ToArray();
            _orderedCurrencies = Unsafe.As<Currency[], ImmutableArray<Currency>>(ref orderedCurrencies);
        }

        return new(_orderedCurrencies);
    }

    /// <summary>
    /// Determines whether this registry is a proper (strict) subset of the specified currency collection.
    /// </summary>
    public bool IsProperSubsetOf(IEnumerable<Currency> other) => _currencies.IsProperSubsetOf(other);

    /// <summary>
    /// Determines whether this registry is a proper superset of the specified currency collection.
    /// </summary>
    public bool IsProperSupersetOf(IEnumerable<Currency> other) => _currencies.IsProperSupersetOf(other);

    /// <summary>
    /// Determines whether this registry is a subset of the specified currency collection.
    /// </summary>
    public bool IsSubsetOf(IEnumerable<Currency> other) => _currencies.IsSubsetOf(other);

    /// <summary>
    /// Determines whether this registry is a superset of the specified currency collection.
    /// </summary>
    public bool IsSupersetOf(IEnumerable<Currency> other) => _currencies.IsSupersetOf(other);

    /// <summary>
    /// Determines whether this registry and the specified currency collection share common elements.
    /// </summary>
    public bool Overlaps(IEnumerable<Currency> other) => _currencies.Overlaps(other);

    /// <summary>
    /// Determines whether this registry and the specified currency collection contain the same elements.
    /// </summary>
    public bool SetEquals(IEnumerable<Currency> other) => _currencies.SetEquals(other);

    private static CurrencyRegistry BuildDefaultRegistry()
    {
        var sourceCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        var currencyLookup = new Dictionary<string, Currency>(sourceCultures.Length);
        var nameInfoLookup = new Dictionary<(Currency Currency, CultureInfo Culture), (CultureInfo SourceCulture, string Value)>(sourceCultures.Length);
        var symbolInfoLookup = new Dictionary<(Currency Currency, CultureInfo Culture), (CultureInfo SourceCulture, string Value)>(sourceCultures.Length);

        foreach (var sourceCulture in sourceCultures)
        {
            if (!sourceCulture.Parent.IsNeutralCulture)
                continue;

            var region = new RegionInfo(sourceCulture.Name);

            // Skip regions that don't have a valid ISO currency.
            // New ICU data contains at least a few such regions, "World", "Europe", "Latin America", etc.
            if (region.ISOCurrencySymbol.Length is not 3)
                continue;

            string currencyCode = region.ISOCurrencySymbol;
            string englishName = region.CurrencyEnglishName;
            string symbol = Currency.GetSystemSymbol(sourceCulture, region);
            bool isSymbolRightToLeft = IsRightToLeft(symbol);

            if (!currencyLookup.TryGetValue(currencyCode, out var currency))
            {
                currency = new Currency(currencyCode, sourceCulture.NumberFormat.CurrencyDecimalDigits, DefaultCurrencyLocalizer.Instance);

                nameInfoLookup.TryAdd((currency, CultureInfo.InvariantCulture), (CultureInfo.InvariantCulture, englishName));
                currencyLookup[currencyCode] = currency;
            }

            string newNativeName = region.CurrencyNativeName;
            var currentCulture = sourceCulture;

            while (true)
            {
                bool setName = currentCulture != CultureInfo.InvariantCulture;
                bool setSymbol = currentCulture != CultureInfo.InvariantCulture || !isSymbolRightToLeft;

                if (setName && nameInfoLookup.TryGetValue((currency, currentCulture), out var existingNativeNameInfo))
                {
                    if (existingNativeNameInfo.Value == newNativeName)
                    {
                        setName = false;
                    }
                    else
                    {
                        // Tie-breaker rules:
                        // - Less specific culture wins
                        // - Latin culture wins
                        // - All ASCII wins

                        setName =
                            ShouldOverrideIfLessSpecific(sourceCulture, existingNativeNameInfo.SourceCulture) ??
                            ShouldOverrideIfLatin(sourceCulture, existingNativeNameInfo.SourceCulture) ??
                            ShouldOverrideIfAscii(newNativeName, existingNativeNameInfo.Value) ??
                            false;
                    }
                }

                if (setSymbol && symbolInfoLookup.TryGetValue((currency, currentCulture), out var existingSymbolInfo))
                {
                    if (existingSymbolInfo.Value == symbol)
                    {
                        setSymbol = false;
                    }
                    else
                    {
                        // Tie-breaker rules:
                        // - Less specific culture wins
                        // - Latin culture wins
                        // - All ASCII wins
                        // - Shorter symbol wins

                        setSymbol =
                            ShouldOverrideIfLessSpecific(sourceCulture, existingSymbolInfo.SourceCulture) ??
                            ShouldOverrideIfLatin(sourceCulture, existingSymbolInfo.SourceCulture) ??
                            ShouldOverrideIfAscii(symbol, existingSymbolInfo.Value) ??
                            ShouldOverrideIfShorter(symbol, existingSymbolInfo.Value) ??
                            false;
                    }
                }

                if (setName)
                    nameInfoLookup[(currency, currentCulture)] = (sourceCulture, newNativeName);

                if (setSymbol)
                    symbolInfoLookup[(currency, currentCulture)] = (sourceCulture, symbol);

                if (currentCulture == CultureInfo.InvariantCulture)
                    break;

                currentCulture = currentCulture.Parent;
            }
        }

        // Add any missing invariant symbols that did not get filled in because the only available symbols were right-to-left using currency code.

        foreach (var c in currencyLookup.Values)
            symbolInfoLookup.TryAdd((c, CultureInfo.InvariantCulture), (CultureInfo.InvariantCulture, c.CurrencyCode));

        RemoveRedundantEntries(nameInfoLookup);
        RemoveRedundantEntries(symbolInfoLookup);

        var nameLookup = nameInfoLookup.ToFrozenDictionary(kvp => (kvp.Key.Currency, CultureName: kvp.Key.Culture.Name), kvp => kvp.Value.Value);
        var symbolLookup = symbolInfoLookup.ToFrozenDictionary(kvp => (kvp.Key.Currency, CultureName: kvp.Key.Culture.Name), kvp => kvp.Value.Value);

        DefaultCurrencyLocalizer.InitLookups(nameLookup, symbolLookup);

        // Clear cached data so we don't needlessly keep unneeded culture data in memory

        CultureInfo.InvariantCulture.ClearCachedData(); // Clears all cached data, not just the culture it is called on

        return new CurrencyRegistry("System", currencyLookup.Values);

        static bool IsRightToLeft(string value) => value.Any(c => c is '\u200E' or '\u200F'); // Check for left-to-right mark

        static bool? ShouldOverrideIfLatin(CultureInfo newCulture, CultureInfo existingCulture)
        {
            // Latin culture always wins

            if (IsLatinCulture(existingCulture.Name))
                return false;

            if (IsLatinCulture(newCulture.Name))
                return true;

            return null;

            static bool IsLatinCulture(string cultureName) => cultureName.Contains("-Latn-");
        }

        static bool? ShouldOverrideIfLessSpecific(CultureInfo newCulture, CultureInfo existingCulture)
        {
            // Count number of dashes in the culture name to determine specificity

            int newSpecificity = newCulture == CultureInfo.InvariantCulture ? -1 : newCulture.Name.Count(c => c == '-');
            int existingSpecificity = existingCulture == CultureInfo.InvariantCulture ? -1 : existingCulture.Name.Count(c => c == '-');

            if (newSpecificity < existingSpecificity)
                return true; // New culture is less specific, so it should override

            if (newSpecificity > existingSpecificity)
                return false; // New culture is more specific, so it should not override

            return null;
        }

        static bool? ShouldOverrideIfAscii(string newValue, string existingValue)
        {
            bool isNewAscii = IsAscii(newValue);
            bool isExistingAscii = IsAscii(existingValue);

            if (isNewAscii)
                return isExistingAscii ? null : true;

            return isExistingAscii ? false : null;

            static bool IsAscii(string value)
            {
#if NET
            return Ascii.IsValid(value);
#else
                return value.All(c => c < 128);
#endif
            }
        }

        static bool? ShouldOverrideIfShorter(string newValue, string existingValue)
        {
            if (newValue.Length < existingValue.Length)
                return true;

            if (newValue.Length > existingValue.Length)
                return false;

            return null;
        }

        static void RemoveRedundantEntries(Dictionary<(Currency Currency, CultureInfo Culture), (CultureInfo SourceCulture, string Value)> lookup)
        {
            // Remove redundant more specific names that match less specific parent name

            var mutable = lookup;
            lookup = new(lookup);

            foreach (var entry in lookup)
            {
                var (key, valueInfo) = (entry.Key, entry.Value);

                if (key.Culture == CultureInfo.InvariantCulture)
                    continue;

                if (lookup.TryGetValue((key.Currency, key.Culture.Parent), out var parentValueInfo) && parentValueInfo.Value == valueInfo.Value)
                    mutable.Remove(key);
            }
        }
    }

    private (StringKeyListDictionaryLookupSwitcher<Currency> Lookup, string? ParseError) GetOrBuildCurrenciesBySymbolLookup(CultureInfo culture)
    {
        string lookupCultureName = culture == CultureInfo.InvariantCulture ? "en" : culture.Name;

        if (!_localizedSymbolLookups.TryGetValue(lookupCultureName, out var lookupInfo))
        {
            lookupInfo = _localizedSymbolLookups.GetOrAdd(lookupCultureName, _ => {
                var lookup = new StringKeyListDictionaryLookupSwitcher<Currency>(BuildCurrenciesBySymbolLookup(culture));
                string parseError = null;

                foreach (string symbol in lookup.Dictionary.Keys)
                {
                    if (!Currency.IsSymbolOrCodeParsable(symbol, out parseError))
                        break;
                }

                return (lookup, parseError);
            });
        }

        return lookupInfo;

        ListDictionary<string, Currency> BuildCurrenciesBySymbolLookup(CultureInfo culture)
        {
            var lookup = new ListDictionary<string, Currency>();

            foreach (var c in _currencies.OrderBy(c => c.CurrencyCode, StringComparer.OrdinalIgnoreCase))
                lookup[c.GetLocalizedSymbol(culture)].Add(c);

            return lookup;
        }
    }

    #region Explicit Interface Implementations

    /// <summary>
    /// Gets a value indicating whether the bag is read-only. Always returns <see langword="true"/>.
    /// </summary>
    bool ICollection<Currency>.IsReadOnly => true;

    /// <summary>
    /// Copies the currencies in this registry to an array.
    /// </summary>
    void ICollection<Currency>.CopyTo(Currency[] array, int arrayIndex) => _currencies.CopyTo(array, arrayIndex);

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator<Currency> IEnumerable<Currency>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Not Supported

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ISet<Currency>.Add(Currency? item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.ExceptWith(IEnumerable<Currency>? other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.IntersectWith(IEnumerable<Currency>? other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.SymmetricExceptWith(IEnumerable<Currency>? other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.UnionWith(IEnumerable<Currency>? other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Currency>.Add(Currency? item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Currency>.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ICollection<Currency>.Remove(Currency? item) => throw new NotSupportedException();

    #endregion

    /// <summary>
    /// Iterates over the currencies in the registry.
    /// </summary>
    public struct Enumerator : IEnumerator<Currency>
    {
        private ImmutableArray<Currency>.Enumerator _enumerator;

        internal Enumerator(ImmutableArray<Currency> orderedCurrencies)
        {
            _enumerator = orderedCurrencies.GetEnumerator();
        }

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public Currency Current => _enumerator.Current;

        /// <inheritdoc cref="IEnumerator.Current"/>
        object IEnumerator.Current => Current;

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public readonly void Dispose() { }

        /// <inheritdoc cref="IEnumerator.MoveNext"/>
        public bool MoveNext() => _enumerator.MoveNext();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public void Reset() => throw new NotSupportedException();
    }
}