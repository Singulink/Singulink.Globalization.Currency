using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable set of <see cref="Money"/> values sorted by currency code.
/// </summary>
/// <remarks>
/// <para>
/// Money sets have one value for each currency they contain, and values are ordered by their currency. If a value is added (or subtracted) in a currency that
/// the set already contains, the value is added to (or subtracted from) the existing value.</para>
/// <para>
/// Money sets never contain any default <see cref="Money"/> values (i.e. zero amount values that are not associated with any currency). Default values are
/// ignored when being added to or subtracted from a set.</para>
/// </remarks>
[CollectionBuilder(typeof(ImmutableSortedMoneySet), nameof(Create))]
public sealed class ImmutableSortedMoneySet : IImmutableMoneySet
{
    private static readonly ImmutableSortedDictionary<Currency, decimal> EmptyLookup = ImmutableSortedDictionary.Create<Currency, decimal>(CurrencyByCodeComparer.Default);

    private readonly CurrencyRegistry _registry;
    private readonly ImmutableSortedDictionary<Currency, decimal> _amountLookup;

    /// <summary>
    /// Creates an empty immutable sorted money set that uses the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public static ImmutableSortedMoneySet Create() => Create(CurrencyRegistry.Default);

    /// <summary>
    /// Creates an empty immutable sorted money set that uses the <see cref="CurrencyRegistry.Default"/> currency registry and contains the specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableSortedMoneySet Create(Money value) => Create(CurrencyRegistry.Default, value);

    /// <summary>
    /// Creates an empty immutable sorted money set that uses the specified currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableSortedMoneySet Create(CurrencyRegistry registry) => new ImmutableSortedMoneySet(registry, EmptyLookup);

    /// <summary>
    /// Creates an immutable sorted money set that uses the specified currency registry and contains the specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableSortedMoneySet Create(CurrencyRegistry registry, Money value)
    {
        var amountLookup = EmptyLookup;
        var currency = value.CurrencyOrDefault;

        if (currency != null)
        {
            EnsureCurrencyAllowed(registry, currency, nameof(value));
            amountLookup = amountLookup.Add(currency, value.Amount);
        }

        return new ImmutableSortedMoneySet(registry, amountLookup);
    }

    /// <inheritdoc cref="Create(ReadOnlySpan{Money})"/>
    public static ImmutableSortedMoneySet Create(params Money[] values) => Create(values.AsSpan());

    /// <inheritdoc cref="Create(CurrencyRegistry, ReadOnlySpan{Money})"/>
    public static ImmutableSortedMoneySet Create(CurrencyRegistry registry, params Money[] values) => Create(registry, values.AsSpan());

    /// <summary>
    /// Creates an immutable sorted money set that uses the <see cref="CurrencyRegistry.Default"/> currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableSortedMoneySet Create(ReadOnlySpan<Money> values) => Create(CurrencyRegistry.Default, values);

    /// <summary>
    /// Creates an immutable sorted money set that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableSortedMoneySet Create(CurrencyRegistry registry, ReadOnlySpan<Money> values)
    {
        if (values.Length == 0)
            return Create(registry);

        var builder = EmptyLookup.ToBuilder();

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency == null)
                continue;

            EnsureCurrencyAllowed(registry, currency, nameof(values));

            if (builder.TryGetValue(currency, out decimal existingAmount))
            {
                decimal newAmount = existingAmount + value.Amount;

                if (newAmount == existingAmount)
                    continue;

                builder[currency] = newAmount;
            }
            else
            {
                builder.Add(currency, value.Amount);
            }
        }

        return new ImmutableSortedMoneySet(registry, builder.ToImmutable());
    }

    /// <inheritdoc cref="Create(ReadOnlySpan{Money})"/>
    public static ImmutableSortedMoneySet CreateRange(IEnumerable<Money> values) => CreateRange(CurrencyRegistry.Default, values);

    /// <inheritdoc cref="Create(CurrencyRegistry, ReadOnlySpan{Money})"/>
    public static ImmutableSortedMoneySet CreateRange(CurrencyRegistry registry, IEnumerable<Money> values)
    {
        return new ImmutableSortedMoneySet(registry, values, values is not IReadOnlyMoneySet s || s.Registry != registry);
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
    internal ImmutableSortedMoneySet(CurrencyRegistry registry, IEnumerable<Money> values, bool ensureValuesInRegistry)
    {
        _registry = registry;
        _amountLookup = EmptyLookup;
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
            if (_amountLookup.TryGetValue(currency, out decimal amount))
                return new Money(amount, currency);

            EnsureCurrencyAllowed(currency, nameof(currency));
            return default;
        }
    }

    /// <inheritdoc cref="IReadOnlyMoneySet.Registry"/>
    public CurrencyRegistry Registry => _registry;

    /// <inheritdoc cref="IReadOnlyMoneySet.Count"/>
    public int Count => _amountLookup.Count;

    /// <inheritdoc cref="IReadOnlyMoneySet.Currencies"/>
    public IEnumerable<Currency> Currencies => _amountLookup.Keys;

    /// <inheritdoc cref="IImmutableMoneySet.Add(Money)"/>
    public ImmutableSortedMoneySet Add(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return this;

        EnsureCurrencyAllowed(currency, nameof(value));
        return AddInternal(value.Amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneySet.Add(decimal, string)"/>
    public ImmutableSortedMoneySet Add(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        return AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneySet.Add(decimal, Currency)"/>
    public ImmutableSortedMoneySet Add(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneySet.AddRange(IEnumerable{Money})"/>
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

    /// <inheritdoc cref="IImmutableMoneySet.Remove(string)"/>
    public ImmutableSortedMoneySet Remove(string currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.Remove(currency);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneySet.Remove(Currency)"/>
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

    /// <inheritdoc cref="IImmutableMoneySet.RemoveAll(IEnumerable{Currency})"/>
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

    /// <inheritdoc cref="IImmutableMoneySet.RemoveAll(Func{Money, bool})"/>
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

    /// <inheritdoc cref="IImmutableMoneySet.RoundToCurrencyDigits()"/>
    public ImmutableSortedMoneySet RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <inheritdoc cref="IImmutableMoneySet.RoundToCurrencyDigits(MidpointRounding)"/>
    public ImmutableSortedMoneySet RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count == 0)
            return this;

        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var entry in _amountLookup)
        {
            decimal roundedValue = decimal.Round(entry.Value, entry.Key.DecimalDigits, mode);

            if (roundedValue != entry.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[entry.Key] = roundedValue;
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneySet.SetValue(Money)"/>
    public ImmutableSortedMoneySet SetValue(Money value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency == null)
            return this;

        return SetAmount(value.Amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneySet.SetAmount(decimal, string)"/>
    public ImmutableSortedMoneySet SetAmount(decimal amount, string currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneySet.SetAmount(decimal, Currency)"/>
    public ImmutableSortedMoneySet SetAmount(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));

        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableSortedMoneySet(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneySet.Subtract(Money)"/>
    public ImmutableSortedMoneySet Subtract(Money value) => Add(-value);

    /// <inheritdoc cref="IImmutableMoneySet.Subtract(decimal, string)"/>
    public ImmutableSortedMoneySet Subtract(decimal amount, string currencyCode) => Add(-amount, currencyCode);

    /// <inheritdoc cref="IImmutableMoneySet.Subtract(decimal, Currency)"/>
    public ImmutableSortedMoneySet Subtract(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(-amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneySet.SubtractRange(IEnumerable{Money})"/>
    public ImmutableSortedMoneySet SubtractRange(IEnumerable<Money> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneySet s || s.Registry != _registry;
        var newAmountLookup = SubtractRangeInternal(values, ensureCurrenciesInRegistry);
        return new ImmutableSortedMoneySet(_registry, newAmountLookup);
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

    /// <inheritdoc cref="IImmutableMoneySet.TransformValues(Func{Money, decimal})"/>
    public ImmutableSortedMoneySet TransformValues(Func<Money, decimal> transform)
    {
        if (Count == 0)
            return this;

        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal newAmount = transform(new Money(kvp.Value, kvp.Key));

            if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneySet.TransformValues(Func{Money, decimal?})"/>
    public ImmutableSortedMoneySet TransformValues(Func<Money, decimal?> transform)
    {
        if (Count == 0)
            return this;

        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal? newAmountOrNull = transform(new Money(kvp.Value, kvp.Key));

            if (newAmountOrNull is not decimal newAmount)
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
            else if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneySet.TransformAmounts(Func{decimal, decimal})"/>
    public ImmutableSortedMoneySet TransformAmounts(Func<decimal, decimal> transform)
    {
        if (Count == 0)
            return this;

        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal newAmount = transform(kvp.Value);

            if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneySet.TransformAmounts(Func{decimal, decimal?})"/>
    public ImmutableSortedMoneySet TransformAmounts(Func<decimal, decimal?> transform)
    {
        if (Count == 0)
            return this;

        ImmutableSortedDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal? newAmountOrNull = transform(kvp.Value);

            if (newAmountOrNull is not decimal newAmount)
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
            else if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder != null ? new ImmutableSortedMoneySet(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneySet.TrimZeroAmounts"/>
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
        if (_amountLookup.TryGetValue(currency, out decimal amount))
        {
            value = new Money(amount, currency);
            return true;
        }

        EnsureCurrencyAllowed(currency, nameof(currency));

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

    private void EnsureCurrencyAllowed(Currency currency, string paramName) => EnsureCurrencyAllowed(_registry, currency, paramName);

    private static void EnsureCurrencyAllowed(CurrencyRegistry registry, Currency currency, string paramName)
    {
        if (!registry.Contains(currency))
            Throw(registry, currency, paramName);

        static void Throw(CurrencyRegistry registry, Currency currency, string paramName)
        {
            throw new ArgumentException($"Currency '{currency}' not found in the '{registry.Name}' registry assigned to this set.", paramName);
        }
    }

    #region Explicit Interface Implementations

#if NET7_0_OR_GREATER
    /// <inheritdoc/>
    static IReadOnlyMoneySet IReadOnlyMoneySet.Create(CurrencyRegistry registry, IEnumerable<Money> values) => CreateRange(registry, values);
#endif

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Add(Money value) => Add(value);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Add(decimal amount, string currencyCode) => Add(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Add(decimal amount, Currency currency) => Add(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.AddRange(IEnumerable<Money> values) => AddRange(values);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Remove(string currencyCode) => Remove(currencyCode);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Remove(Currency currency) => Remove(currency);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.RemoveAll(IEnumerable<Currency> currencies) => RemoveAll(currencies);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.RemoveAll(Func<Money, bool> predicate) => RemoveAll(predicate);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.RoundToCurrencyDigits() => RoundToCurrencyDigits();

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.RoundToCurrencyDigits(MidpointRounding mode) => RoundToCurrencyDigits(mode);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.SetValue(Money value) => SetValue(value);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.SetAmount(decimal amount, string currencyCode) => SetAmount(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.SetAmount(decimal amount, Currency currency) => SetAmount(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Subtract(Money value) => Subtract(value);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Subtract(decimal amount, string currencyCode) => Subtract(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.Subtract(decimal amount, Currency currency) => Subtract(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.SubtractRange(IEnumerable<Money> values) => SubtractRange(values);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.TransformValues(Func<Money, decimal> transform) => TransformValues(transform);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.TransformValues(Func<Money, decimal?> transform) => TransformValues(transform);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.TransformAmounts(Func<decimal, decimal> transform) => TransformAmounts(transform);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.TransformAmounts(Func<decimal, decimal?> transform) => TransformAmounts(transform);

    /// <inheritdoc/>
    IImmutableMoneySet IImmutableMoneySet.TrimZeroAmounts() => TrimZeroAmounts();

    /// <inheritdoc/>
    IEnumerator<Money> IEnumerable<Money>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
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

        /// <inheritdoc/>
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