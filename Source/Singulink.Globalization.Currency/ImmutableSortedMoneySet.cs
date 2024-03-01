using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable set of <see cref="Money"/> values.
/// </summary>
/// <remarks>
/// <para>
/// Money sets have one value for each currency they contain, and values are ordered by their currency. If a value is added (or subtracted) in a currency that
/// the set already contains, the value is added to (or subtracted from) the existing value.</para>
/// <para>
/// Money sets never contain any default <see cref="Money"/> values (i.e. zero amount values that are not associated with any currency). Default values are
/// ignored when being added to or subtracted from a set.</para>
/// </remarks>
public sealed class ImmutableSortedMoneySet : IReadOnlyMoneySet, IEquatable<ImmutableSortedMoneySet>, IFormattable
{
    private static readonly ImmutableSortedDictionary<Currency, decimal> EmptyLookup = ImmutableSortedDictionary.Create<Currency, decimal>(CurrencyByCodeComparer.Default);

    private readonly CurrencyRegistry _registry;
    private readonly ImmutableSortedDictionary<Currency, decimal> _amountLookup;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public ImmutableSortedMoneySet() : this(CurrencyRegistry.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry and
    /// specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableSortedMoneySet(Money value) : this(CurrencyRegistry.Default, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the specified currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableSortedMoneySet(CurrencyRegistry registry)
    {
        _registry = registry;
        _amountLookup = EmptyLookup;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the specified currency registry and value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableSortedMoneySet(CurrencyRegistry registry, Money value) : this(registry)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return;

        EnsureCurrencyAllowed(currency, nameof(value));
        _amountLookup = _amountLookup.Add(currency, value.Amount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the <see cref="CurrencyRegistry.Default"/> currency registry and adds all
    /// the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableSortedMoneySet(params Money[] values) : this(CurrencyRegistry.Default, values, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class with the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableSortedMoneySet(CurrencyRegistry registry, params Money[] values) : this(registry, values, true)
    {
    }

    /// <inheritdoc cref="ImmutableSortedMoneySet(Money[])"/>
    public ImmutableSortedMoneySet(IEnumerable<Money> values) : this(CurrencyRegistry.Default, values)
    {
    }

    /// <inheritdoc cref="ImmutableSortedMoneySet(CurrencyRegistry?, Money[])"/>
    public ImmutableSortedMoneySet(CurrencyRegistry registry, IEnumerable<Money> values) : this(registry, values, values is not IReadOnlyMoneySet s || s.Registry != registry)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class. Trusted private constructor.
    /// </summary>
    private ImmutableSortedMoneySet(CurrencyRegistry registry, ImmutableSortedDictionary<Currency, decimal> amountLookup)
    {
        _registry = registry;
        _amountLookup = amountLookup;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableSortedMoneySet"/> class. Trusted internal constructor.
    /// </summary>
    internal ImmutableSortedMoneySet(CurrencyRegistry registry, IEnumerable<Money> values, bool ensureValuesInRegistry) : this(registry)
    {
        _amountLookup = AddRangeInternal(values, ensureValuesInRegistry);
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
            EnsureCurrencyAllowed(currency, nameof(currency));

            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new Money(amount, currency);

            return default;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.Registry"/>
    public CurrencyRegistry Registry => _registry;

    /// <inheritdoc cref="IReadOnlyMoneySet.Count"/>
    public int Count => _amountLookup.Count;

    /// <inheritdoc cref="IReadOnlyMoneySet.Currencies"/>
    public IEnumerable<Currency> Currencies => _amountLookup.Keys;

    /// <summary>
    /// Adds the specified value to this set and returns the resulting set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public ImmutableSortedMoneySet Add(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return this;

        EnsureCurrencyAllowed(currency, nameof(value));
        return AddInternal(value.Amount, currency);
    }

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Add(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        return AddInternal(amount, currency);
    }

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Add(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(amount, currency);
    }

    /// <summary>
    /// Adds the specified values to this set and returns the resulting set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public ImmutableSortedMoneySet AddRange(IEnumerable<Money> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneySet s || s.Registry != _registry;
        var newAmountLookup = AddRangeInternal(values, ensureCurrenciesInRegistry);
        return new ImmutableSortedMoneySet(_registry, newAmountLookup);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the values in this set.
    /// </summary>
    public Enumerator GetEnumerator() => new(_amountLookup);

    /// <summary>
    /// Removes the value with the given currency code and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Remove(string currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.Remove(currency);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <summary>
    /// Removes the value with the given currency and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Remove(Currency currency)
    {
        var updatedLookup = _amountLookup.Remove(currency);

        if (updatedLookup == _amountLookup)
        {
            EnsureCurrencyAllowed(currency, nameof(currency));
            return this;
        }

        return new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <summary>
    /// Removes all the values from this set that match the specified currencies and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet RemoveAll(IEnumerable<Currency> currencies)
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var currency in currencies)
        {
            if (_amountLookup.ContainsKey(currency))
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(currency);
            }
            else
            {
                EnsureCurrencyAllowed(currency, nameof(currencies));
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <summary>
    /// Removes all the values from this set that match the specified predicate and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet RemoveAll(Func<Money, bool> predicate)
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            if (predicate(new Money(kvp.Value, kvp.Key)))
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using <see cref="MidpointRounding.ToEven"/> midpoint rounding
    /// (i.e. "banker's rounding") and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <summary>
    /// Rounts each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using the specified midpoint rounding mode and returns the resulting
    /// set.
    /// </summary>
    public ImmutableSortedMoneySet RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count == 0)
            return this;

        return new ImmutableSortedMoneySet(_registry, this.Select(v => v.RoundToCurrencyDigits(mode)), false);
    }

    public ImmutableSortedMoneySet SetValue(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return this;

        return SetAmount(value.Amount, currency);
    }

    public ImmutableSortedMoneySet SetAmount(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    public ImmutableSortedMoneySet SetAmount(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));

        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <summary>
    /// Subtracts the specified value from this set and returns the resulting set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public ImmutableSortedMoneySet Subtract(Money value) => Add(-value);

    /// <summary>
    /// Subtracts the specified currency and amount from this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Subtract(decimal amount, string currencyCode) => Add(-amount, currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet Subtract(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(amount, currency);
    }

    /// <summary>
    /// Subtracts the specified values from this set and returns the resulting set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public ImmutableSortedMoneySet SubtractRange(IEnumerable<Money> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneySet s || s.Registry != _registry;
        var newAmountLookup = SubtractRangeInternal(values, ensureCurrenciesInRegistry);
        return new ImmutableSortedMoneySet(_registry, newAmountLookup);
    }

    /// <summary>
    /// Applies the specified transformation to all the values in this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet TransformValues(Func<Money, Money> transform)
    {
        if (Count == 0)
            return this;

        return new ImmutableSortedMoneySet(_registry, this.Select(transform));
    }

    /// <summary>
    /// Applies the specified transformation to all the value amounts in this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet TransformAmounts(Func<decimal, decimal> transform)
    {
        if (Count == 0)
            return this;

        return new ImmutableSortedMoneySet(_registry, _amountLookup.Select(kvp => new Money(transform(kvp.Value), kvp.Key)), false);
    }

    /// <summary>
    /// Removes all zero amounts from this set and returns the resulting set.
    /// </summary>
    public ImmutableSortedMoneySet TrimZeroAmounts()
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            if (kvp.Value == 0)
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
        }

        return builder == null ? this : new ImmutableSortedMoneySet(_registry, builder.ToImmutable());
    }

    /// <summary>
    /// Copies the values in this set to a new mutable set that uses the same registry as this set.
    /// </summary>
    public SortedMoneySet ToSet() => new(_registry, this, false);

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    /// <param name="format">The format to use for each money value. See <see cref="Money.ToString(string?, IFormatProvider?)"/> for valid money formats.
    /// Prepend the format with the <c>"!"</c> character to ignore zero amount values.</param>
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

            foreach (var amount in _amountLookup.Values)
            {
                if (amount != 0)
                    count++;
            }

            return count;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetAmount(Currency, out decimal)"/>
    public bool TryGetAmount(Currency currency, out decimal amount)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return _amountLookup.TryGetValue(currency, out amount);
    }

    public bool TryGetAmount(string currencyCode, out decimal amount)
    {
        var currency = _registry[currencyCode];
        return _amountLookup.TryGetValue(currency, out amount);
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetValue(string, out Money)"/>
    public bool TryGetValue(string currencyCode, out Money value)
    {
        var currency = _registry[currencyCode];

        if (_amountLookup.TryGetValue(currency, out decimal amount))
        {
            value = new Money(amount, currency);
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.TryGetValue(Currency, out Money)"/>
    public bool TryGetValue(Currency currency, out Money value)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));

        if (_amountLookup.TryGetValue(currency, out decimal amount))
        {
            value = new Money(amount, currency);
            return true;
        }

        value = default;
        return false;
    }

    private ImmutableSortedMoneySet AddInternal(decimal amount, Currency currency)
    {
        ImmutableSortedDictionary<Currency, decimal> amountLookup;

        if (_amountLookup.TryGetValue(currency, out decimal existingAmount))
        {
            decimal newAmount = existingAmount + amount;

            if (newAmount == existingAmount)
                return this;

            amountLookup = _amountLookup.SetItem(currency, newAmount);
        }
        else
        {
            amountLookup = _amountLookup.Add(currency, amount);
        }

        return amountLookup == null ? this : new ImmutableSortedMoneySet(_registry, amountLookup);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImmutableSortedDictionary<Currency, decimal> AddRangeInternal(IEnumerable<Money> values, bool ensureCurrenciesInRegistry)
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            if (ensureCurrenciesInRegistry)
                EnsureCurrencyAllowed(currency, nameof(values));

            if (builder?.TryGetValue(currency, out decimal existingAmount) ?? _amountLookup.TryGetValue(currency, out existingAmount))
            {
                decimal newAmount = existingAmount + value.Amount;

                if (newAmount == existingAmount)
                    continue;

                builder ??= _amountLookup.ToBuilder();
                builder[currency] = newAmount;
            }
            else
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Add(currency, value.Amount);
            }
        }

        return builder != null ? builder.ToImmutable() : _amountLookup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImmutableSortedDictionary<Currency, decimal> SubtractRangeInternal(IEnumerable<Money> values, bool ensureCurrenciesInRegistry)
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            if (ensureCurrenciesInRegistry)
                EnsureCurrencyAllowed(currency, nameof(values));

            if (builder?.TryGetValue(currency, out decimal existingAmount) ?? _amountLookup.TryGetValue(currency, out existingAmount))
            {
                decimal newAmount = existingAmount - value.Amount;

                if (newAmount == existingAmount)
                    continue;

                builder ??= _amountLookup.ToBuilder();
                builder[currency] = newAmount;
            }
            else
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Add(currency, -value.Amount);
            }
        }

        return builder != null ? builder.ToImmutable() : _amountLookup;
    }

    private void EnsureCurrencyAllowed(Currency currency, string paramName)
    {
        if (!_registry.Contains(currency))
            Throw(currency, paramName);

        void Throw(Currency currency, string paramName)
        {
            throw new ArgumentException($"Currency '{currency}' not found in the '{_registry.Name}' registry assigned to this set.", paramName);
        }
    }

    #region Equality

    /// <summary>
    /// Determines whether the specified set is equal to this set. Immutable money sets compare by their values, and the <see cref="Registry"/> must also match
    /// in order for the sets to be considered equal.
    /// </summary>
    public bool Equals(ImmutableSortedMoneySet? other)
    {
        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (_registry != other._registry || Count != other.Count)
            return false;

        var e1 = _amountLookup.GetEnumerator();
        var e2 = other._amountLookup.GetEnumerator();

        while (e1.MoveNext())
        {
            e2.MoveNext();

            var kvp1 = e1.Current;
            var kvp2 = e2.Current;

            if (kvp1.Key != kvp2.Key || kvp1.Value != kvp2.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to this set. Immutable money sets compare by their values, and the <see cref="Registry"/> must also
    /// match in order for the sets to be considered equal.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as ImmutableSortedMoneySet);

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode()
    {
        HashCode hash = default;
        hash.Add(_registry);

        foreach (var v in this)
            hash.Add(v);

        return hash.ToHashCode();
    }

    #endregion

    #region Explicit Interface Implementations

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator<Money> IEnumerable<Money>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    /// Enumerates the elements of a <see cref="ImmutableSortedMoneySet"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<Money>
    {
        private ImmutableSortedDictionary<Currency, decimal>.Enumerator _amountLookupEnumerator;

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public Money Current => new(_amountLookupEnumerator.Current.Value, _amountLookupEnumerator.Current.Key);

        /// <inheritdoc cref="Current"/>
        object? IEnumerator.Current => Current;

        internal Enumerator(ImmutableSortedDictionary<Currency, decimal> amountLookup)
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