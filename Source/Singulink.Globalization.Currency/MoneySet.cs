using System.Collections;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Represents a set of <see cref="Money"/> values.
/// </summary>
/// <remarks>
/// <para>
/// Money sets have one value for each currency they contain, and values are ordered by their currency. If a value is added (or subtracted) in a currency that
/// the set already contains, the value is added to (or subtracted from) the existing value.</para>
/// <para>
/// Money sets never contain any default <see cref="Money"/> values (i.e. zero amount values that are not associated with any currency). Default values are
/// ignored when being added to or subtracted from a set.</para>
/// </remarks>
public class MoneySet : IMoneySet
{
    private readonly CurrencyRegistry _registry;
    private readonly Dictionary<Currency, decimal> _amountLookup = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneySet"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public MoneySet() : this(CurrencyRegistry.Default) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneySet"/> class with the specified currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public MoneySet(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneySet"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry and adds all
    /// the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public MoneySet(IEnumerable<Money> values) : this(CurrencyRegistry.Default, values) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneySet"/> class with the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public MoneySet(CurrencyRegistry registry, IEnumerable<Money> values)
        : this(registry, values, values is not IReadOnlyMoneySet s || s.Registry != registry) { }

    /// <inheritdoc cref="MoneySet(IEnumerable{Money})"/>
    public MoneySet(ReadOnlySpan<Money> values) : this(CurrencyRegistry.Default, values) { }

    /// <inheritdoc cref="MoneySet(CurrencyRegistry, IEnumerable{Money})"/>
    public MoneySet(CurrencyRegistry registry, ReadOnlySpan<Money> values)
    {
        _registry = registry;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            EnsureCurrencyAllowed(currency, nameof(values));

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
                _amountLookup[currency] = existingAmount + value.Amount;
            else
                _amountLookup.Add(currency, value.Amount);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneySet"/> class. Trusted internal constructor.
    /// </summary>
    internal MoneySet(CurrencyRegistry registry, IEnumerable<Money> values, bool ensureValuesInRegistry) : this(registry)
    {
        AddRangeInternal(values, ensureValuesInRegistry);
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.this[string]"/>
    public Money this[string currencyCode]
    {
        get {
            var currency = _registry[currencyCode];

            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new Money(amount, currency);

            return default;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.this[Currency]"/>
    public Money this[Currency currency]
    {
        get {
            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new Money(amount, currency);

            EnsureCurrencyAllowed(currency, nameof(currency));
            return default;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.Count"/>
    public int Count => _amountLookup.Count;

    /// <inheritdoc cref="IReadOnlyMoneySet.Currencies"/>
    public IReadOnlyCollection<Currency> Currencies => _amountLookup.Keys;

    /// <inheritdoc cref="IReadOnlyMoneySet.Registry"/>
    public CurrencyRegistry Registry => _registry;

    /// <inheritdoc cref="IMoneySet.Add(Money)"/>
    public void Add(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return;

        EnsureCurrencyAllowed(currency, nameof(value));
        AddInternal(value.Amount, currency);
    }

    /// <inheritdoc cref="IMoneySet.Add(decimal, string)"/>
    public void Add(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IMoneySet.Add(decimal, Currency)"/>
    public void Add(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IMoneySet.AddRange(IEnumerable{Money})"/>
    public void AddRange(IEnumerable<Money> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneySet s || s.Registry != _registry;
        AddRangeInternal(values, ensureCurrenciesInRegistry);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the values in this set.
    /// </summary>
    public Enumerator GetEnumerator() => new(_amountLookup);

    /// <inheritdoc cref="IMoneySet.Remove(string)"/>
    public bool Remove(string currencyCode)
    {
        var currency = _registry[currencyCode];
        return _amountLookup.Remove(currency);
    }

    /// <inheritdoc cref="IMoneySet.Remove(Currency)"/>
    public bool Remove(Currency currency)
    {
        bool removed = _amountLookup.Remove(currency);

        if (!removed)
        {
            EnsureCurrencyAllowed(currency, nameof(currency));
        }

        return removed;
    }

    /// <inheritdoc cref="IMoneySet.RemoveAll(IEnumerable{Currency})"/>
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

        if (disallowedCurrencies != null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(currencies));

        return count;
    }

    /// <inheritdoc cref="IMoneySet.RemoveAll(Func{Money, bool})"/>
    public int RemoveAll(Func<Money, bool> predicate)
    {
        List<Currency> currenciesToRemove = null;
        foreach (var kvp in _amountLookup)
        {
            var money = new Money(kvp.Value, kvp.Key);
            if (predicate(money))
            {
                currenciesToRemove ??= [];
                currenciesToRemove.Add(kvp.Key);
            }
        }

        if (currenciesToRemove != null)
        {
            foreach (var currency in currenciesToRemove)
            {
                _amountLookup.Remove(currency);
            }

            return currenciesToRemove.Count;
        }

        return 0;
    }

    /// <inheritdoc cref="IMoneySet.RoundToCurrencyDigits()"/>
    public void RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <inheritdoc cref="IMoneySet.RoundToCurrencyDigits(MidpointRounding)"/>
    public void RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count == 0)
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

        if (updatedEntries == null)
            return;

        foreach (var entry in updatedEntries)
        {
            _amountLookup[entry.Key] = entry.Value;
        }
    }

    /// <inheritdoc cref="IMoneySet.SetValue(Money)"/>
    public void SetValue(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return;

        SetAmount(value.Amount, currency);
    }

    /// <inheritdoc cref="IMoneySet.SetAmount(decimal, string)"/>
    public void SetAmount(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        _amountLookup[currency] = amount;
    }

    /// <inheritdoc cref="IMoneySet.SetAmount(decimal, Currency)"/>
    public void SetAmount(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        _amountLookup[currency] = amount;
    }

    /// <inheritdoc cref="IMoneySet.Subtract(Money)"/>
    public void Subtract(Money value) => Add(-value);

    /// <inheritdoc cref="IMoneySet.Subtract(decimal, string)"/>
    public void Subtract(decimal amount, string currencyCode) => Add(-amount, currencyCode);

    /// <inheritdoc cref="IMoneySet.Subtract(decimal, Currency)"/>
    public void Subtract(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        AddInternal(-amount, currency);
    }

    /// <inheritdoc cref="IMoneySet.SubtractRange(IEnumerable{Money})"/>
    public void SubtractRange(IEnumerable<Money> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneySet s || s.Registry != _registry;
        SubtractRangeInternal(values, ensureCurrenciesInRegistry);
    }

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    /// <param name="format">The format to use for each money value. See <see cref="Money.ToString(string?, IFormatProvider?)"/> for valid money formats.
    /// Prepend the desired money format with the <c>!</c> character to ignore zero amount values.</param>
    /// <param name="formatProvider">The format provider that will be used to obtain number format information. This should be a <see cref="CultureInfo"/>
    /// instance for formats that depend on the culture, otherwise the current culture is used.</param>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        bool ignoreZeroAmounts;
        int count;

        if (format != null && format.StartsWith('!'))
        {
            format = format[1..];
            ignoreZeroAmounts = true;
            count = GetNonZeroCount();
        }
        else
        {
            ignoreZeroAmounts = false;
            count = Count;
        }

        if (count == 0)
            return string.Empty;

        var sb = new StringBuilder(count * 8);
        bool first = true;

        foreach (var value in this)
        {
            if (ignoreZeroAmounts && value.Amount == 0)
                continue;

            if (first)
                first = false;
            else
                sb.Append(", ");

            sb.Append(value.ToString(format, formatProvider));
        }

        return sb.ToString();

        int GetNonZeroCount()
        {
            int count = 0;

            foreach (decimal amount in _amountLookup.Values)
            {
                if (amount != 0)
                    count++;
            }

            return count;
        }
    }

    /// <inheritdoc cref="IMoneySet.TransformValues(Func{Money, decimal})"/>
    public void TransformValues(Func<Money, decimal> transform)
    {
        if (Count == 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal newAmount = transform(new Money(kvp.Value, kvp.Key));

            if (newAmount != kvp.Value)
            {
                _amountLookup[kvp.Key] = newAmount;
            }
        }
    }

    /// <inheritdoc cref="IMoneySet.TransformValues(Func{Money, decimal?})"/>
    public void TransformValues(Func<Money, decimal?> transform)
    {
        if (Count == 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal? newAmountOrNull = transform(new Money(kvp.Value, kvp.Key));

            if (newAmountOrNull is not decimal newAmount)
            {
                _amountLookup.Remove(kvp.Key);
            }
            else if (newAmount != kvp.Value)
            {
                _amountLookup[kvp.Key] = newAmount;
            }
        }
    }

    /// <inheritdoc cref="IMoneySet.TransformAmounts(Func{decimal, decimal})"/>
    public void TransformAmounts(Func<decimal, decimal> transform)
    {
        if (Count == 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal oldAmount = kvp.Value;
            decimal newAmount = transform(oldAmount);

            if (newAmount != oldAmount)
            {
                _amountLookup[kvp.Key] = newAmount;
            }
        }
    }

    /// <inheritdoc cref="IMoneySet.TransformAmounts(Func{decimal, decimal?})"/>
    public void TransformAmounts(Func<decimal, decimal?> transform)
    {
        if (Count == 0)
            return;

        // TODO: Optimize if no values change.

        foreach (var kvp in _amountLookup.ToList())
        {
            decimal? newAmountOrNull = transform(kvp.Value);

            if (newAmountOrNull is not decimal newAmount)
            {
                _amountLookup.Remove(kvp.Key);
            }
            else if (newAmount != kvp.Value)
            {
                _amountLookup[kvp.Key] = newAmount;
            }
        }
    }

    /// <inheritdoc cref="IMoneySet.TrimZeroAmounts"/>
    public int TrimZeroAmounts()
    {
        List<Currency> currenciesToRemove = null;

        foreach (var kvp in _amountLookup)
        {
            if (kvp.Value == 0)
            {
                currenciesToRemove ??= [];
                currenciesToRemove.Add(kvp.Key);
            }
        }

        if (currenciesToRemove != null)
        {
            foreach (var currency in currenciesToRemove)
            {
                _amountLookup.Remove(currency);
            }

            return currenciesToRemove.Count;
        }

        return 0;
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetAmount(Currency, out decimal)"/>
    public bool TryGetAmount(Currency currency, out decimal amount)
    {
        if (_amountLookup.TryGetValue(currency, out amount))
            return true;

        EnsureCurrencyAllowed(currency, nameof(currency));
        return false;
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetAmount(string, out decimal)"/>
    public bool TryGetAmount(string currencyCode, out decimal amount)
    {
        var currency = _registry[currencyCode];
        return _amountLookup.TryGetValue(currency, out amount);
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetValue(Currency, out Money)"/>
    public bool TryGetValue(Currency currency, out Money value)
    {
        if (_amountLookup.TryGetValue(currency, out decimal amount))
        {
            value = new Money(amount, currency);
            return true;
        }

        EnsureCurrencyAllowed(currency, nameof(currency));

        value = default;
        return false;
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetValue(string, out Money)"/>
    public bool TryGetValue(string currencyCode, out Money value)
    {
        var currency = _registry[currencyCode];
        return TryGetValue(currency, out value);
    }

    private void AddInternal(decimal amount, Currency currency)
    {
        if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
        {
            _amountLookup[currency] = existingAmount + amount;
        }
        else
        {
            _amountLookup[currency] = amount;
        }
    }

    private void AddRangeInternal(IEnumerable<Money> values, bool ensureCurrenciesInRegistry)
    {
        List<Currency> disallowedCurrencies = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            if (ensureCurrenciesInRegistry && !_registry.Contains(currency))
            {
                disallowedCurrencies ??= [];
                disallowedCurrencies.Add(currency);
                continue;
            }

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
                _amountLookup[currency] = existingAmount + value.Amount;
            else
                _amountLookup.Add(currency, value.Amount);
        }

        if (disallowedCurrencies != null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(values));
    }

    private void SubtractRangeInternal(IEnumerable<Money> values, bool ensureCurrenciesInRegistry)
    {
        List<Currency> disallowedCurrencies = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            if (ensureCurrenciesInRegistry && !_registry.Contains(currency))
            {
                disallowedCurrencies ??= [];
                disallowedCurrencies.Add(currency);
                continue;
            }

            if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
                _amountLookup[currency] = existingAmount - value.Amount;
            else
                _amountLookup.Add(currency, -value.Amount);
        }

        if (disallowedCurrencies != null)
            ThrowCurrenciesDisallowed(disallowedCurrencies, nameof(values));
    }

    private void EnsureCurrencyAllowed(Currency currency, string paramName)
    {
        if (!_registry.Contains(currency))
            Throw(currency, paramName);

        static void Throw(Currency currency, string paramName)
        {
            throw new ArgumentException($"The currency '{currency}' is not present in the set's currency registry.", paramName);
        }
    }

    [DoesNotReturn]
    private void ThrowCurrenciesDisallowed(List<Currency> currencies, string paramName)
    {
        throw new ArgumentException($"The following currencies are not present in the set's currency registry: {string.Join(", ", currencies.Distinct())}", paramName);
    }

    #region Explicit Interface Implementations

#if NET7_0_OR_GREATER

    /// <inheritdoc/>
    static IReadOnlyMoneySet IReadOnlyMoneySet.Create(CurrencyRegistry registry, IEnumerable<Money> values) => new MoneySet(registry, values);

    /// <inheritdoc/>
    static IMoneySet IMoneySet.Create(CurrencyRegistry registry, IEnumerable<Money> values) => new MoneySet(registry, values);

#endif

    /// <inheritdoc/>
    IEnumerable<Currency> IReadOnlyMoneySet.Currencies => Currencies;

    /// <inheritdoc/>
    IEnumerator<Money> IEnumerable<Money>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    /// Enumerates the elements of a <see cref="MoneySet"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<Money>
    {
        private Dictionary<Currency, decimal>.Enumerator _amountLookupEnumerator;

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public Money Current => new(_amountLookupEnumerator.Current.Value, _amountLookupEnumerator.Current.Key);

        /// <inheritdoc/>
        object? IEnumerator.Current => Current;

        internal Enumerator(Dictionary<Currency, decimal> amountLookup)
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