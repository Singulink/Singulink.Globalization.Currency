using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable set of <see cref="Money"/> values.
/// </summary>
/// <remarks>
/// <para>
/// Money sets have one value for each currency they contain, and values are ordered by their currency. If values in a
/// currency that the set already contains are added, the value is added to the existing value.</para>
/// <para>
/// Note: Money sets never contain any default <see cref="Money"/> values (i.e. zero amount values that are not associated with any currency). These are
/// ignored if an attempt is made to add them to a set.</para>
/// </remarks>
public sealed class ImmutableMoneySet : IEnumerable<Money>, IFormattable
{
    private static readonly ImmutableSortedDictionary<Currency, decimal> _emptyLookup = ImmutableSortedDictionary.Create<Currency, decimal>(CurrencyByCodeComparer.Default);
    private readonly CurrencyRegistry _registry;
    private readonly ImmutableSortedDictionary<Currency, decimal> _amountLookup;
    private readonly int _zeroAmountCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class using the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public ImmutableMoneySet() : this(CurrencyRegistry.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class using the <see cref="CurrencyRegistry.Default"/> currency registry and adds the
    /// specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableMoneySet(Money value) : this(CurrencyRegistry.Default, value) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class using the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableMoneySet(CurrencyRegistry registry)
    {
        _registry = registry;
        _amountLookup = _emptyLookup;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class using the specified currency registry (or <see
    /// cref="CurrencyRegistry.Default"/> if no registry is provided) and adds the value specified.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableMoneySet(CurrencyRegistry registry, Money value) : this(registry)
    {
        if (value.IsDefault)
            return;

        EnsureCurrencyAllowed(value.Currency, nameof(value));
        _amountLookup = _amountLookup.Add(value.CurrencyOrDefault, value.Amount);

        if (value.Amount == 0)
            _zeroAmountCount = 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class using the <see cref="CurrencyRegistry.Default"/>
    /// currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableMoneySet(params Money[] values) : this(CurrencyRegistry.Default, values.AsEnumerable())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class with the specified money values and currency registry (or <see
    /// cref="Currency.SystemCurrencies"/> if no registry is provided).
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public ImmutableMoneySet(CurrencyRegistry registry, params Money[] values) : this(registry, values.AsEnumerable())
    {
    }

    /// <inheritdoc cref="ImmutableMoneySet(Money[])"/>
    public ImmutableMoneySet(IEnumerable<Money> values) : this(CurrencyRegistry.Default, values)
    {
    }

    /// <inheritdoc cref="ImmutableMoneySet(CurrencyRegistry?, Money[])"/>
    public ImmutableMoneySet(CurrencyRegistry registry, IEnumerable<Money> values) : this(registry)
    {
        _amountLookup = AddRangeInternal(values, true, out _zeroAmountCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class. Trusted internal constructor.
    /// </summary>
    internal ImmutableMoneySet(CurrencyRegistry registry, ImmutableSortedDictionary<Currency, decimal> amountLookup, int zeroAmountCount)
    {
        _registry = registry;
        _amountLookup = amountLookup;
        _zeroAmountCount = zeroAmountCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneySet"/> class. Trusted private constructor.
    /// </summary>
    private ImmutableMoneySet(CurrencyRegistry registry, IEnumerable<Money> values, bool ensureValuesInRegistry) : this(registry)
    {
        _amountLookup = AddRangeInternal(values, ensureValuesInRegistry, out _zeroAmountCount);
    }

    /// <summary>
    /// Gets the value this set contains with the specified currency code. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[string currencyCode] => this[_registry[currencyCode]];

    /// <summary>
    /// Gets the value this set contains of the given currency. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[Currency currency]
    {
        get {
            EnsureCurrencyAllowed(currency, nameof(currency));

            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new Money(amount, currency);

            return default;
        }
    }

    /// <summary>
    /// Gets the currency registry associated with this set of values.
    /// </summary>
    public CurrencyRegistry Registry => _registry;

    /// <summary>
    /// Gets the number of values in the money set.
    /// </summary>
    public int Count => _amountLookup.Count;

    /// <summary>
    /// Adds the specified amount to all the values in this set.
    /// </summary>
    public ImmutableMoneySet AddToAll(decimal amount)
    {
        if (_amountLookup.Count == 0)
            return this;

        return new ImmutableMoneySet(_registry, _amountLookup.Select(kvp => new Money(kvp.Value + amount, kvp.Key)), false);
    }

    /// <summary>
    /// Subtracts the specified amount from all the values in this set.
    /// </summary>
    public ImmutableMoneySet SubtractFromAll(decimal amount)
    {
        if (_amountLookup.Count == 0)
            return this;

        return new ImmutableMoneySet(_registry, _amountLookup.Select(kvp => new Money(kvp.Value - amount, kvp.Key)), false);
    }

    /// <summary>
    /// Multiplies all the values in this by the specified amount.
    /// </summary>
    public ImmutableMoneySet MultiplyAll(decimal amount)
    {
        if (_amountLookup.Count == _zeroAmountCount)
            return this;

        return new ImmutableMoneySet(_registry, _amountLookup.Select(kvp => new Money(kvp.Value * amount, kvp.Key)), false);
    }

    /// <summary>
    /// Divides all the values in this by the specified amount.
    /// </summary>
    public ImmutableMoneySet DivideAll(decimal amount)
    {
        if (_amountLookup.Count == _zeroAmountCount)
        {
            if (amount == 0)
                ThrowDivideByZero();

            return this;
        }

        var builder = ImmutableSortedDictionary.CreateBuilder<Currency, decimal>();

        return new ImmutableMoneySet(_registry, _amountLookup.Select(kvp => new Money(kvp.Value / amount, kvp.Key)), false);

        static void ThrowDivideByZero() => throw new DivideByZeroException();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the values in this set.
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(_amountLookup);

    /// <summary>
    /// Returns a multi-currency amount with each value rounded to its currency's number of decimal digits using <see cref="MidpointRounding.ToEven"/>
    /// midpoint rounding (i.e. "banker's rounding").
    /// </summary>
    public ImmutableMoneySet RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <summary>
    /// Returns a multi-currency amount with each value rounded to its currency's number of decimal digits using the specified midpoint rounding mode.
    /// </summary>
    public ImmutableMoneySet RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count == 0)
            return this;

        return new ImmutableMoneySet(_registry, this.Select(v => v.RoundToCurrencyDigits(mode)), false);
    }

    /// <summary>
    /// Returns a money set with all zero amount values removed.
    /// </summary>
    public ImmutableMoneySet TrimZeroAmounts()
    {
        if (_zeroAmountCount == 0)
            return this;

        if (_zeroAmountCount == _amountLookup.Count)
            return new ImmutableMoneySet(_registry);

        ImmutableSortedDictionary<Currency, decimal>.Builder builder;

        if (_zeroAmountCount > _amountLookup.Count >>> 1)
        {
            // More zeros than values so build from an empty lookup and add non-zero items

            builder = _emptyLookup.ToBuilder();

            foreach (var kvp in _amountLookup)
            {
                if (kvp.Value != 0)
                    builder.Add(kvp.Key, kvp.Value);
            }
        }
        else
        {
            // More values than zeros so start from the current set and remove zeros

            builder = _amountLookup.ToBuilder();

            foreach (var kvp in builder)
            {
                if (kvp.Value == 0)
                    builder.Remove(kvp.Key);
            }
        }

        return new ImmutableMoneySet(_registry, builder.ToImmutable(), 0);
    }

    private ImmutableSortedDictionary<Currency, decimal> AddRangeInternal(IEnumerable<Money> values, bool ensureCurrenciesInRegistry, out int zeroAmountDiff)
    {
        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;
        zeroAmountDiff = 0;

        foreach (var value in values)
        {
            if (value.IsDefault)
                continue;

            var currency = value.CurrencyOrDefault;

            if (ensureCurrenciesInRegistry)
                EnsureCurrencyAllowed(currency, nameof(values));

            if (value.Amount == 0)
            {
                if (builder?.ContainsKey(currency) ?? _amountLookup.ContainsKey(currency))
                    continue;

                builder ??= _amountLookup.ToBuilder();
                builder[currency] = 0;
                zeroAmountDiff++;

                continue;
            }

            builder ??= _amountLookup.ToBuilder();

            if (!builder.TryGetValue(currency, out decimal amount))
            {
                builder.Add(currency, value.Amount);
            }
            else
            {
                if (amount == 0)
                    zeroAmountDiff--;

                decimal newAmount = amount + value.Amount;

                if (newAmount == 0)
                    zeroAmountDiff++;

                builder[currency] = newAmount;
            }
        }

        return builder != null ? builder.ToImmutable() : _amountLookup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCurrencyAllowed(Currency currency, string paramName)
    {
        if (!_registry.Contains(currency))
            Throw(currency, paramName);

        static void Throw(Currency currency, string paramName)
        {
            throw new ArgumentException($"The currency '{currency}' is not present in the set's currency registry.", paramName);
        }
    }

    #region String Formatting

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string representation of the money values this set contains.
    /// </summary>
    /// <param name="format">The format to use. See <see cref="Money.ToString(string?, IFormatProvider?)"/> for valid money formats. You can optionally prepend
    /// the money format with <c>"!"</c> to ignore zero values.</param>
    /// <param name="formatProvider">The format provider that will be used to obtain number format information. This should be a <see cref="CultureInfo"/>
    /// instance for formats that depend on the culture, otherwise the current culture is used.</param>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (IsEmpty)
            return string.Empty;

        // Format string gets passed to each Money, with the exception of a leading ! indicating that empty values should not be displayed.

        IEnumerable<Money> values = _amountLookup;

        if (format?.Length >= 1 && format[0] == '!')
        {
            values = values.Where(v => v.Amount != 0);
            format = format.Substring(1);
        }

        return string.Join(", ", values.Select(v => v.ToString(format, formatProvider)));
    }

    #endregion

    #region Explicit Interface Implementations

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator<Money> IEnumerable<Money>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    /// Enumerates the elements of a <see cref="ImmutableMoneySet"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<Money>
    {
        private ImmutableSortedDictionary<Currency, decimal>.Enumerator _amountLookupEnumerator;

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public Money Current => new Money(_amountLookupEnumerator.Current.Value, _amountLookupEnumerator.Current.Key);

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