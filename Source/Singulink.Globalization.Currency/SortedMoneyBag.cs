using System.Collections;

namespace Singulink.Globalization;

/// <summary>
/// Represents a bag of monetary values in one or more currencies sorted by currency code.
/// </summary>
/// <remarks>
/// <para>
/// Money bags have one value for each currency they contain, and values are ordered by their currency. If a value is added (or subtracted) in a currency that
/// the bag already contains, the value is added to (or subtracted from) the existing value.</para>
/// <para>
/// Money bags never contain any default <see cref="MonetaryValue"/> values (i.e. zero amount values that are not associated with any currency). Default values
/// are ignored when being added to or subtracted from a bag.</para>
/// </remarks>
public sealed partial class SortedMoneyBag : IMoneyBag
{
    private readonly CurrencyRegistry _registry;
    private readonly SortedDictionary<Currency, decimal> _amountLookup = new(CurrencyByCodeComparer.Default);
    private CurrencyCollection? _currencyCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedMoneyBag"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public SortedMoneyBag() : this(CurrencyRegistry.Default) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedMoneyBag"/> class with the specified currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public SortedMoneyBag(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc cref="MoneyBag(ReadOnlySpan{MonetaryValue})"/>
    public SortedMoneyBag(IEnumerable<MonetaryValue> values) : this(CurrencyRegistry.Default, values) { }

    /// <inheritdoc cref="MoneyBag(CurrencyRegistry, ReadOnlySpan{MonetaryValue})"/>
    public SortedMoneyBag(CurrencyRegistry registry, IEnumerable<MonetaryValue> values)
        : this(registry, values, values is not IReadOnlyMoneyBag s || s.Registry != registry) { }

    /// <inheritdoc cref="MoneyBag(ReadOnlySpan{MonetaryValue})"/>
    public SortedMoneyBag(params MonetaryValue[] values) : this(CurrencyRegistry.Default, values) { }

    /// <inheritdoc cref="MoneyBag(CurrencyRegistry, ReadOnlySpan{MonetaryValue})"/>
    public SortedMoneyBag(CurrencyRegistry registry, params MonetaryValue[] values) : this(registry, values.AsSpan()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedMoneyBag"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry and adds all
    /// the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public SortedMoneyBag(ReadOnlySpan<MonetaryValue> values) : this(CurrencyRegistry.Default, values) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedMoneyBag"/> class with the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public SortedMoneyBag(CurrencyRegistry registry, ReadOnlySpan<MonetaryValue> values)
    {
        _registry = registry;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            EnsureCurrencyAllowed(currency, nameof(values));

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                _amountLookup[currency] = existingAmount + value.Amount;
            }
            else
            {
                _amountLookup.Add(currency, value.Amount);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedMoneyBag"/> class. Trusted internal constructor.
    /// </summary>
    internal SortedMoneyBag(CurrencyRegistry registry, IEnumerable<MonetaryValue> values, bool ensureValuesInRegistry) : this(registry)
    {
        AddRangeInternal(values, ensureValuesInRegistry);
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.this[string]"/>
    public MonetaryValue this[TargetDependentStringKey currencyCode]
    {
        get {
            if (_registry.TryGetCurrency(currencyCode, out var currency) && _amountLookup.TryGetValue(currency, out decimal amount))
                return new MonetaryValue(amount, currency);

            return default;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.this[Currency]"/>
    public MonetaryValue this[Currency currency]
    {
        get {
            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new MonetaryValue(amount, currency);

            return default;
        }
    }

    /// <inheritdoc cref="IMoneyBag.Count"/>
    public int Count => _amountLookup.Count;

    /// <inheritdoc cref="IReadOnlyMoneyBag.Currencies"/>
    public CurrencyCollection Currencies => _currencyCollection ??= new(this);

    /// <inheritdoc cref="IReadOnlyMoneyBag.Registry"/>
    public CurrencyRegistry Registry => _registry;

    /// <inheritdoc cref="IMoneyBag.Add(MonetaryValue)"/>
    public void Add(MonetaryValue value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency is null)
            return;

        EnsureCurrencyAllowed(currency, nameof(value));
        AddInternal(value.Amount, currency);
    }

    /// <inheritdoc cref="IMoneyBag.Add(decimal, string)"/>
    public void Add(decimal amount, TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IMoneyBag.Add(decimal, Currency)"/>
    public void Add(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IMoneyBag.AddRange(IEnumerable{MonetaryValue})"/>
    public void AddRange(IEnumerable<MonetaryValue> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneyBag s || s.Registry != _registry;
        AddRangeInternal(values, ensureCurrenciesInRegistry);
    }

    /// <summary>
    /// Removes all values from this bag.
    /// </summary>
    public void Clear() => _amountLookup.Clear();

    /// <inheritdoc cref="IReadOnlyMoneyBag.Contains(MonetaryValue)"/>
    public bool Contains(MonetaryValue value) => _amountLookup.TryGetValue(value.Currency, out decimal amount) && amount == value.Amount;

    /// <inheritdoc cref="IReadOnlyMoneyBag.Contains(decimal, Currency)"/>
    public bool Contains(decimal amount, Currency currency) => _amountLookup.TryGetValue(currency, out decimal existingAmount) && existingAmount == amount;

    /// <inheritdoc cref="IReadOnlyMoneyBag.Contains(decimal, string)"/>
    public bool Contains(decimal amount, TargetDependentStringKey currencyCode)
    {
        return _registry.TryGetCurrency(currencyCode, out var currency) &&
            _amountLookup.TryGetValue(currency, out decimal existingAmount) &&
            existingAmount == amount;
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.ContainsCurrency(Currency)"/>
    public bool ContainsCurrency(Currency currency) => _amountLookup.ContainsKey(currency);

    /// <inheritdoc cref="IReadOnlyMoneyBag.ContainsCurrency(string)"/>
    public bool ContainsCurrency(TargetDependentStringKey currencyCode) => _registry.TryGetCurrency(currencyCode, out var currency) && _amountLookup.ContainsKey(currency);

    /// <summary>
    /// Returns an enumerator that iterates through the values in this bag.
    /// </summary>
    public Enumerator GetEnumerator() => new(_amountLookup);

    /// <inheritdoc cref="IMoneyBag.Remove(string)"/>
    public bool Remove(TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        return _amountLookup.Remove(currency);
    }

    /// <inheritdoc cref="IMoneyBag.Remove(Currency)"/>
    public bool Remove(Currency currency)
    {
        bool removed = _amountLookup.Remove(currency);

        if (!removed)
        {
            EnsureCurrencyAllowed(currency, nameof(currency));
        }

        return removed;
    }

    /// <inheritdoc cref="IMoneyBag.RemoveAll(IEnumerable{Currency})"/>
    public int RemoveAll(IEnumerable<Currency> currencies)
    {
        int count = 0;
        List<Currency> disallowedCurrencies = null;

        foreach (var currency in currencies)
        {
            if (!_registry.Contains(currency))
            {
                disallowedCurrencies ??= [];
                disallowedCurrencies.Add(currency);
                continue;
            }

            if (Remove(currency))
                count++;
        }

        if (disallowedCurrencies is not null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(currencies));

        return count;
    }

    /// <inheritdoc cref="IMoneyBag.RemoveAll(Func{MonetaryValue, bool})"/>
    public int RemoveAll(Func<MonetaryValue, bool> predicate)
    {
        List<Currency> currenciesToRemove = null;

        foreach (var kvp in _amountLookup)
        {
            var money = new MonetaryValue(kvp.Value, kvp.Key);

            if (predicate(money))
            {
                currenciesToRemove ??= [];
                currenciesToRemove.Add(kvp.Key);
            }
        }

        if (currenciesToRemove is not null)
        {
            foreach (var currency in currenciesToRemove)
                _amountLookup.Remove(currency);

            return currenciesToRemove.Count;
        }

        return 0;
    }

    /// <inheritdoc cref="IMoneyBag.RoundToCurrencyDigits()"/>
    public void RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <inheritdoc cref="IMoneyBag.RoundToCurrencyDigits(MidpointRounding)"/>
    public void RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count is 0)
            return;

        List<KeyValuePair<Currency, decimal>> updatedEntries = null;

        foreach (var entry in _amountLookup)
        {
            decimal roundedValue = decimal.Round(entry.Value, entry.Key.DecimalDigits, mode);

            if (roundedValue != entry.Value)
            {
                updatedEntries ??= [];
                updatedEntries.Add(new(entry.Key, roundedValue));
            }
        }

        if (updatedEntries is null)
            return;

        foreach (var entry in updatedEntries)
            _amountLookup[entry.Key] = entry.Value;
    }

    /// <inheritdoc cref="IMoneyBag.SetValue(MonetaryValue)"/>
    public void SetValue(MonetaryValue value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency is null)
            return;

        SetAmount(value.Amount, currency);
    }

    /// <inheritdoc cref="IMoneyBag.SetAmount(decimal, string)"/>
    public void SetAmount(decimal amount, TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        _amountLookup[currency] = amount;
    }

    /// <inheritdoc cref="IMoneyBag.SetAmount(decimal, Currency)"/>
    public void SetAmount(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        _amountLookup[currency] = amount;
    }

    /// <inheritdoc cref="IMoneyBag.Subtract(MonetaryValue)"/>
    public void Subtract(MonetaryValue value) => Add(-value);

    /// <inheritdoc cref="IMoneyBag.Subtract(decimal, string)"/>
    public void Subtract(decimal amount, TargetDependentStringKey currencyCode) => Add(-amount, currencyCode);

    /// <inheritdoc cref="IMoneyBag.Subtract(decimal, Currency)"/>
    public void Subtract(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        AddInternal(-amount, currency);
    }

    /// <inheritdoc cref="IMoneyBag.SubtractRange(IEnumerable{MonetaryValue})"/>
    public void SubtractRange(IEnumerable<MonetaryValue> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneyBag s || s.Registry != _registry;
        SubtractRangeInternal(values, ensureCurrenciesInRegistry);
    }

    /// <summary>
    /// Copies the values in this bag to a new immutable bag that uses the same registry as this bag.
    /// </summary>
    public ImmutableSortedMoneyBag ToImmutableSet() => new ImmutableSortedMoneyBag(_registry, this, false);

    /// <summary>
    /// Returns a string representation of the monetary values this bag contains.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="MoneyCollectionExtensions.ToString(IReadOnlyMoneyBag, string?, IFormatProvider?)"/>
    public string ToString(string? format, IFormatProvider? provider = null) => MoneyCollectionExtensions.ToString(this, format, provider);

    /// <inheritdoc cref="IMoneyBag.TransformValues(Func{MonetaryValue, decimal})"/>
    public void TransformValues(Func<MonetaryValue, decimal> transform)
    {
        if (Count is 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal newAmount = transform(new MonetaryValue(kvp.Value, kvp.Key));

            if (newAmount != kvp.Value)
                _amountLookup[kvp.Key] = newAmount;
        }
    }

    /// <inheritdoc cref="IMoneyBag.TransformValues(Func{MonetaryValue, decimal?})"/>
    public void TransformValues(Func<MonetaryValue, decimal?> transform)
    {
        if (Count is 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal? newAmountOrNull = transform(new MonetaryValue(kvp.Value, kvp.Key));

            if (newAmountOrNull is not decimal newAmount)
                _amountLookup.Remove(kvp.Key);
            else if (newAmount != kvp.Value)
                _amountLookup[kvp.Key] = newAmount;
        }
    }

    /// <inheritdoc cref="IMoneyBag.TransformAmounts(Func{decimal, decimal})"/>
    public void TransformAmounts(Func<decimal, decimal> transform)
    {
        if (Count is 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal oldAmount = kvp.Value;
            decimal newAmount = transform(oldAmount);

            if (newAmount != oldAmount)
                _amountLookup[kvp.Key] = newAmount;
        }
    }

    /// <inheritdoc cref="IMoneyBag.TransformAmounts(Func{decimal, decimal?})"/>
    public void TransformAmounts(Func<decimal, decimal?> transform)
    {
        if (Count is 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal? newAmountOrNull = transform(kvp.Value);

            if (newAmountOrNull is not decimal newAmount)
                _amountLookup.Remove(kvp.Key);
            else if (newAmount != kvp.Value)
                _amountLookup[kvp.Key] = newAmount;
        }
    }

    /// <inheritdoc cref="IMoneyBag.TrimZeroAmounts"/>
    public int TrimZeroAmounts()
    {
        List<Currency> currenciesToRemove = null;

        foreach (var kvp in _amountLookup)
        {
            if (kvp.Value is 0)
            {
                currenciesToRemove ??= [];
                currenciesToRemove.Add(kvp.Key);
            }
        }

        if (currenciesToRemove is not null)
        {
            foreach (var currency in currenciesToRemove)
                _amountLookup.Remove(currency);

            return currenciesToRemove.Count;
        }

        return 0;
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.TryGetAmount(Currency, out decimal)"/>
    public bool TryGetAmount(Currency currency, out decimal amount) => _amountLookup.TryGetValue(currency, out amount);

    /// <inheritdoc cref="IReadOnlyMoneyBag.TryGetAmount(string, out decimal)"/>
    public bool TryGetAmount(TargetDependentStringKey currencyCode, out decimal amount)
    {
        if (_registry.TryGetCurrency(currencyCode, out var currency))
            return _amountLookup.TryGetValue(currency, out amount);

        amount = 0;
        return false;
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.TryGetValue(Currency, out MonetaryValue)"/>
    public bool TryGetValue(Currency currency, out MonetaryValue value)
    {
        if (_amountLookup.TryGetValue(currency, out decimal amount))
        {
            value = new MonetaryValue(amount, currency);
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc cref="IReadOnlyMoneyBag.TryGetValue(string, out MonetaryValue)"/>
    public bool TryGetValue(TargetDependentStringKey currencyCode, out MonetaryValue value)
    {
        if (_registry.TryGetCurrency(currencyCode, out var currency))
            return TryGetValue(currency, out value);

        value = default;
        return false;
    }

    private void AddInternal(decimal amount, Currency currency)
    {
        if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
            _amountLookup[currency] = existingAmount + amount;
        else
            _amountLookup[currency] = amount;
    }

    private void AddRangeInternal(IEnumerable<MonetaryValue> values, bool ensureCurrenciesInRegistry)
    {
        List<Currency> disallowedCurrencies = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            if (ensureCurrenciesInRegistry && !_registry.Contains(currency))
            {
                disallowedCurrencies ??= [];
                disallowedCurrencies.Add(currency);
                continue;
            }

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                _amountLookup[currency] = existingAmount + value.Amount;
            }
            else
            {
                _amountLookup.Add(currency, value.Amount);
            }
        }

        if (disallowedCurrencies is not null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(values));
    }

    private void SubtractRangeInternal(IEnumerable<MonetaryValue> values, bool ensureCurrenciesInRegistry)
    {
        List<Currency> disallowedCurrencies = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            if (ensureCurrenciesInRegistry && !_registry.Contains(currency))
            {
                disallowedCurrencies ??= [];
                disallowedCurrencies.Add(currency);
                continue;
            }

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                _amountLookup[currency] = existingAmount - value.Amount;
            }
            else
            {
                _amountLookup.Add(currency, -value.Amount);
            }
        }

        if (disallowedCurrencies is not null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(values));
    }

    private void EnsureCurrencyAllowed(Currency currency, string paramName)
    {
        if (!_registry.Contains(currency))
            Throw(currency, paramName);

        static void Throw(Currency currency, string paramName)
        {
            throw new ArgumentException($"The currency '{currency}' is not present in the bag's currency registry.", paramName);
        }
    }

    [DoesNotReturn]
    private static void ThrowCurrenciesDisallowed(List<Currency> currencies, string paramName)
    {
        throw new ArgumentException($"The following currencies are not present in the bag's currency registry: {string.Join(", ", currencies.Distinct())}", paramName);
    }

    #region Explicit Interface Implementations

#if NET

    /// <inheritdoc/>
    static IReadOnlyMoneyBag IReadOnlyMoneyBag.Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => new SortedMoneyBag(registry, values);

    /// <inheritdoc/>
    static IMoneyBag IMoneyBag.Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => new SortedMoneyBag(registry, values);

#endif

    /// <inheritdoc/>
    bool IReadOnlyMoneyBag.IsSorted => true;

    /// <inheritdoc/>
    bool ICollection<MonetaryValue>.IsReadOnly => false;

    /// <inheritdoc/>
    IReadOnlyCollection<Currency> IReadOnlyMoneyBag.Currencies => Currencies;

    /// <inheritdoc/>
    void ICollection<MonetaryValue>.CopyTo(MonetaryValue[] array, int arrayIndex)
    {
        CollectionCopy.CheckParams(Count, array, arrayIndex);

        foreach (var value in this)
            array[arrayIndex++] = value;
    }

    /// <inheritdoc/>
    bool ICollection<MonetaryValue>.Remove(MonetaryValue item)
    {
        if (Contains(item))
            return Remove(item.Currency);

        return false;
    }

    /// <inheritdoc/>
    IEnumerator<MonetaryValue> IEnumerable<MonetaryValue>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    /// Enumerates the elements of a <see cref="SortedMoneyBag"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<MonetaryValue>
    {
        private SortedDictionary<Currency, decimal>.Enumerator _amountLookupEnumerator;

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public MonetaryValue Current => new(_amountLookupEnumerator.Current.Value, _amountLookupEnumerator.Current.Key);

        /// <inheritdoc/>
        object? IEnumerator.Current => Current;

        internal Enumerator(SortedDictionary<Currency, decimal> amountLookup)
        {
            _amountLookupEnumerator = amountLookup.GetEnumerator();
        }

        /// <summary>
        /// Releases all the resources used by the enumerator.
        /// </summary>
        public void Dispose() => _amountLookupEnumerator.Dispose();

        /// <summary>
        /// Advances the enumerator to the next element.
        /// </summary>
        public bool MoveNext() => _amountLookupEnumerator.MoveNext();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        void IEnumerator.Reset() => throw new NotSupportedException();
    }
}