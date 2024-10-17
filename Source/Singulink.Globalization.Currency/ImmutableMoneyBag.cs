using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable bag of monetary values in one or more currencies.
/// </summary>
/// <remarks>
/// <para>
/// Money bags have one value for each currency they contain, and values are ordered by their currency. If a value is added (or subtracted) in a currency that
/// the bag already contains, the value is added to (or subtracted from) the existing value.</para>
/// <para>
/// Money bags never contain any default <see cref="MonetaryValue"/> values (i.e. zero amount values that are not associated with any currency). Default values
/// are ignored when being added to or subtracted from a bag.</para>
/// </remarks>
[CollectionBuilder(typeof(ImmutableMoneyBag), nameof(Create))]
public sealed partial class ImmutableMoneyBag : IImmutableMoneyBag
{
    private static readonly ImmutableDictionary<Currency, decimal> EmptyLookup = ImmutableDictionary.Create<Currency, decimal>();

    private readonly CurrencyRegistry _registry;
    private readonly ImmutableDictionary<Currency, decimal> _amountLookup;
    private CurrencyCollection? _currencyCollection;

    /// <summary>
    /// Creates an empty immutable money bag that uses the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public static ImmutableMoneyBag Create() => Create(CurrencyRegistry.Default);

    /// <summary>
    /// Creates an empty immutable money bag that uses the <see cref="CurrencyRegistry.Default"/> currency registry and contains the specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableMoneyBag Create(MonetaryValue value) => Create(CurrencyRegistry.Default, value);

    /// <summary>
    /// Creates an empty immutable money bag that uses the specified currency registry.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableMoneyBag Create(CurrencyRegistry registry) => new(registry, EmptyLookup);

    /// <summary>
    /// Creates an immutable money bag that uses the specified currency registry and contains the specified value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableMoneyBag Create(CurrencyRegistry registry, MonetaryValue value)
    {
        var amountLookup = EmptyLookup;
        var currency = value.CurrencyOrDefault;

        if (currency is not null)
        {
            EnsureCurrencyAllowed(registry, currency, nameof(value));
            amountLookup = amountLookup.Add(currency, value.Amount);
        }

        return new ImmutableMoneyBag(registry, amountLookup);
    }

    /// <inheritdoc cref="Create(ReadOnlySpan{MonetaryValue})"/>
    public static ImmutableMoneyBag Create(params MonetaryValue[] values) => Create(values.AsSpan());

    /// <inheritdoc cref="Create(CurrencyRegistry, ReadOnlySpan{MonetaryValue})"/>
    public static ImmutableMoneyBag Create(CurrencyRegistry registry, params MonetaryValue[] values) => Create(registry, values.AsSpan());

    /// <summary>
    /// Creates an immutable money bag that uses the <see cref="CurrencyRegistry.Default"/> currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableMoneyBag Create(ReadOnlySpan<MonetaryValue> values) => Create(CurrencyRegistry.Default, values);

    /// <summary>
    /// Creates an immutable money bag that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static ImmutableMoneyBag Create(CurrencyRegistry registry, ReadOnlySpan<MonetaryValue> values)
    {
        if (values.Length is 0)
            return Create(registry);

        var builder = EmptyLookup.ToBuilder();

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            EnsureCurrencyAllowed(registry, currency, nameof(values));

            if (builder.TryGetValue(currency, out decimal existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                builder[currency] = existingAmount + value.Amount;
            }
            else
            {
                builder.Add(currency, value.Amount);
            }
        }

        return new ImmutableMoneyBag(registry, builder.ToImmutable());
    }

    /// <inheritdoc cref="Create(ReadOnlySpan{MonetaryValue})"/>
    public static ImmutableMoneyBag CreateRange(IEnumerable<MonetaryValue> values) => CreateRange(CurrencyRegistry.Default, values);

    /// <inheritdoc cref="Create(CurrencyRegistry, ReadOnlySpan{MonetaryValue})"/>
    public static ImmutableMoneyBag CreateRange(CurrencyRegistry registry, IEnumerable<MonetaryValue> values)
    {
        return new ImmutableMoneyBag(registry, values, values is not IReadOnlyMoneyBag s || s.Registry != registry);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneyBag"/> class. Trusted private constructor.
    /// </summary>
    private ImmutableMoneyBag(CurrencyRegistry registry, ImmutableDictionary<Currency, decimal> amountLookup)
    {
        _registry = registry;
        _amountLookup = amountLookup;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableMoneyBag"/> class. Trusted internal constructor.
    /// </summary>
    internal ImmutableMoneyBag(CurrencyRegistry registry, IEnumerable<MonetaryValue> values, bool ensureValuesInRegistry)
    {
        _registry = registry;
        _amountLookup = EmptyLookup;
        _amountLookup = AddRangeInternal(values, ensureValuesInRegistry);
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

    /// <inheritdoc cref="IImmutableMoneyBag.Count"/>
    public int Count => _amountLookup.Count;

    /// <inheritdoc cref="IReadOnlyMoneyBag.Currencies"/>
    public CurrencyCollection Currencies => _currencyCollection ??= new(this);

    /// <inheritdoc cref="IReadOnlyMoneyBag.Registry"/>
    public CurrencyRegistry Registry => _registry;

    /// <inheritdoc cref="IImmutableMoneyBag.Add(MonetaryValue)"/>
    public ImmutableMoneyBag Add(MonetaryValue value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency is null)
            return this;

        EnsureCurrencyAllowed(currency, nameof(value));
        return AddInternal(value.Amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.Add(decimal, string)"/>
    public ImmutableMoneyBag Add(decimal amount, TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        return AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.Add(decimal, Currency)"/>
    public ImmutableMoneyBag Add(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.AddRange(IEnumerable{MonetaryValue})"/>
    public ImmutableMoneyBag AddRange(IEnumerable<MonetaryValue> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneyBag s || s.Registry != _registry;
        var newAmountLookup = AddRangeInternal(values, ensureCurrenciesInRegistry);
        return new ImmutableMoneyBag(_registry, newAmountLookup);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.Clear"/>
    public ImmutableMoneyBag Clear()
    {
        if (Count is 0)
            return this;

        return new ImmutableMoneyBag(_registry, EmptyLookup);
    }

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

    /// <inheritdoc cref="IImmutableMoneyBag.Remove(string)"/>
    public ImmutableMoneyBag Remove(TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.Remove(currency);
        return updatedLookup == _amountLookup ? this : new ImmutableMoneyBag(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.Remove(Currency)"/>
    public ImmutableMoneyBag Remove(Currency currency)
    {
        var updatedLookup = _amountLookup.Remove(currency);

        if (updatedLookup == _amountLookup)
        {
            EnsureCurrencyAllowed(currency, nameof(currency));
            return this;
        }

        return new ImmutableMoneyBag(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.RemoveAll(IEnumerable{Currency})"/>
    public ImmutableMoneyBag RemoveAll(IEnumerable<Currency> currencies)
    {
        ImmutableDictionary<Currency, decimal>.Builder builder = null;

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

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.RemoveAll(Func{MonetaryValue, bool})"/>
    public ImmutableMoneyBag RemoveAll(Func<MonetaryValue, bool> predicate)
    {
        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            if (predicate(new MonetaryValue(kvp.Value, kvp.Key)))
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
        }

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.RoundToCurrencyDigits()"/>
    public ImmutableMoneyBag RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <inheritdoc cref="IImmutableMoneyBag.RoundToCurrencyDigits(MidpointRounding)"/>
    public ImmutableMoneyBag RoundToCurrencyDigits(MidpointRounding mode)
    {
        if (Count is 0)
            return this;

        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var entry in _amountLookup)
        {
            decimal roundedValue = decimal.Round(entry.Value, entry.Key.DecimalDigits, mode);

            if (roundedValue != entry.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[entry.Key] = roundedValue;
            }
        }

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.SetValue(MonetaryValue)"/>
    public ImmutableMoneyBag SetValue(MonetaryValue value)
    {
        var currency = value.CurrencyOrDefault;

        if (currency is null)
            return this;

        return SetAmount(value.Amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.SetAmount(decimal, string)"/>
    public ImmutableMoneyBag SetAmount(decimal amount, TargetDependentStringKey currencyCode)
    {
        var currency = _registry[currencyCode];
        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableMoneyBag(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.SetAmount(decimal, Currency)"/>
    public ImmutableMoneyBag SetAmount(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));

        var updatedLookup = _amountLookup.SetItem(currency, amount);
        return updatedLookup == _amountLookup ? this : new ImmutableMoneyBag(_registry, updatedLookup);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.Subtract(MonetaryValue)"/>
    public ImmutableMoneyBag Subtract(MonetaryValue value) => Add(-value);

    /// <inheritdoc cref="IImmutableMoneyBag.Subtract(decimal, string)"/>
    public ImmutableMoneyBag Subtract(decimal amount, TargetDependentStringKey currencyCode) => Add(-amount, currencyCode);

    /// <inheritdoc cref="IImmutableMoneyBag.Subtract(decimal, Currency)"/>
    public ImmutableMoneyBag Subtract(decimal amount, Currency currency)
    {
        EnsureCurrencyAllowed(currency, nameof(currency));
        return AddInternal(-amount, currency);
    }

    /// <inheritdoc cref="IImmutableMoneyBag.SubtractRange(IEnumerable{MonetaryValue})"/>
    public ImmutableMoneyBag SubtractRange(IEnumerable<MonetaryValue> values)
    {
        bool ensureCurrenciesInRegistry = values is not IReadOnlyMoneyBag s || s.Registry != _registry;
        var newAmountLookup = SubtractRangeInternal(values, ensureCurrenciesInRegistry);
        return new ImmutableMoneyBag(_registry, newAmountLookup);
    }

    /// <summary>
    /// Returns a string representation of the monetary values this bag contains.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="MoneyCollectionExtensions.ToString(IReadOnlyMoneyBag, string?, IFormatProvider?)"/>
    public string ToString(string? format, IFormatProvider? provider = null) => MoneyCollectionExtensions.ToString(this, format, provider);

    /// <inheritdoc cref="IImmutableMoneyBag.TransformValues(Func{MonetaryValue, decimal})"/>
    public ImmutableMoneyBag TransformValues(Func<MonetaryValue, decimal> transform)
    {
        if (Count is 0)
            return this;

        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal newAmount = transform(new MonetaryValue(kvp.Value, kvp.Key));

            if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.TransformValues(Func{MonetaryValue, decimal?})"/>
    public ImmutableMoneyBag TransformValues(Func<MonetaryValue, decimal?> transform)
    {
        if (Count is 0)
            return this;

        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal? newAmountOrNull = transform(new MonetaryValue(kvp.Value, kvp.Key));

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

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.TransformAmounts(Func{decimal, decimal})"/>
    public ImmutableMoneyBag TransformAmounts(Func<decimal, decimal> transform)
    {
        if (Count is 0)
            return this;

        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            decimal newAmount = transform(kvp.Value);

            if (newAmount != kvp.Value)
            {
                builder ??= _amountLookup.ToBuilder();
                builder[kvp.Key] = newAmount;
            }
        }

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.TransformAmounts(Func{decimal, decimal?})"/>
    public ImmutableMoneyBag TransformAmounts(Func<decimal, decimal?> transform)
    {
        if (Count is 0)
            return this;

        ImmutableDictionary<Currency, decimal>.Builder builder = null;

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

        return builder is not null ? new ImmutableMoneyBag(_registry, builder.ToImmutable()) : this;
    }

    /// <inheritdoc cref="IImmutableMoneyBag.TrimZeroAmounts"/>
    public ImmutableMoneyBag TrimZeroAmounts()
    {
        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var kvp in _amountLookup)
        {
            if (kvp.Value is 0)
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Remove(kvp.Key);
            }
        }

        return builder is null ? this : new ImmutableMoneyBag(_registry, builder.ToImmutable());
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

    private ImmutableMoneyBag AddInternal(decimal amount, Currency currency)
    {
        ImmutableDictionary<Currency, decimal> amountLookup;

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

        return amountLookup is null ? this : new ImmutableMoneyBag(_registry, amountLookup);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImmutableDictionary<Currency, decimal> AddRangeInternal(IEnumerable<MonetaryValue> values, bool ensureCurrenciesInRegistry)
    {
        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            if (ensureCurrenciesInRegistry)
                EnsureCurrencyAllowed(currency, nameof(values));

            if (builder?.TryGetValue(currency, out decimal existingAmount) ?? _amountLookup.TryGetValue(currency, out existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                builder ??= _amountLookup.ToBuilder();
                builder[currency] = existingAmount + value.Amount;
            }
            else
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Add(currency, value.Amount);
            }
        }

        return builder is not null ? builder.ToImmutable() : _amountLookup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImmutableDictionary<Currency, decimal> SubtractRangeInternal(IEnumerable<MonetaryValue> values, bool ensureCurrenciesInRegistry)
    {
        ImmutableDictionary<Currency, decimal>.Builder builder = null;

        foreach (var value in values)
        {
            var currency = value.CurrencyOrDefault;

            if (currency is null)
                continue;

            if (ensureCurrenciesInRegistry)
                EnsureCurrencyAllowed(currency, nameof(values));

            if (builder?.TryGetValue(currency, out decimal existingAmount) ?? _amountLookup.TryGetValue(currency, out existingAmount))
            {
                if (value.Amount is 0)
                    continue;

                builder ??= _amountLookup.ToBuilder();
                builder[currency] = existingAmount - value.Amount;
            }
            else
            {
                builder ??= _amountLookup.ToBuilder();
                builder.Add(currency, -value.Amount);
            }
        }

        return builder is not null ? builder.ToImmutable() : _amountLookup;
    }

    private void EnsureCurrencyAllowed(Currency currency, string paramName) => EnsureCurrencyAllowed(_registry, currency, paramName);

    private static void EnsureCurrencyAllowed(CurrencyRegistry registry, Currency currency, string paramName)
    {
        if (!registry.Contains(currency))
            Throw(currency, paramName);

        static void Throw(Currency currency, string paramName)
        {
            throw new ArgumentException($"The currency '{currency}' is not present in the bag's currency registry.", paramName);
        }
    }

    #region Explicit Interface Implementations

#if NET7_0_OR_GREATER

    /// <inheritdoc/>
    static IReadOnlyMoneyBag IReadOnlyMoneyBag.Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => CreateRange(registry, values);

    /// <inheritdoc/>
    static IImmutableMoneyBag IImmutableMoneyBag.Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => CreateRange(registry, values);

#endif

    /// <inheritdoc/>
    IReadOnlyCollection<Currency> IReadOnlyMoneyBag.Currencies => Currencies;

    /// <inheritdoc/>
    bool IReadOnlyMoneyBag.IsSorted => false;

    /// <summary>
    /// Gets a value indicating whether the bag is read-only. Always returns <see langword="true"/>.
    /// </summary>
    bool ICollection<MonetaryValue>.IsReadOnly => true;

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Add(MonetaryValue value) => Add(value);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Add(decimal amount, string currencyCode) => Add(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Add(decimal amount, Currency currency) => Add(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.AddRange(IEnumerable<MonetaryValue> values) => AddRange(values);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Clear() => Clear();

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Remove(string currencyCode) => Remove(currencyCode);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Remove(Currency currency) => Remove(currency);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.RemoveAll(IEnumerable<Currency> currencies) => RemoveAll(currencies);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.RemoveAll(Func<MonetaryValue, bool> predicate) => RemoveAll(predicate);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.RoundToCurrencyDigits() => RoundToCurrencyDigits();

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.RoundToCurrencyDigits(MidpointRounding mode) => RoundToCurrencyDigits(mode);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.SetValue(MonetaryValue value) => SetValue(value);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.SetAmount(decimal amount, string currencyCode) => SetAmount(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.SetAmount(decimal amount, Currency currency) => SetAmount(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Subtract(MonetaryValue value) => Subtract(value);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Subtract(decimal amount, string currencyCode) => Subtract(amount, currencyCode);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.Subtract(decimal amount, Currency currency) => Subtract(amount, currency);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.SubtractRange(IEnumerable<MonetaryValue> values) => SubtractRange(values);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.TransformValues(Func<MonetaryValue, decimal> transform) => TransformValues(transform);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.TransformValues(Func<MonetaryValue, decimal?> transform) => TransformValues(transform);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.TransformAmounts(Func<decimal, decimal> transform) => TransformAmounts(transform);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.TransformAmounts(Func<decimal, decimal?> transform) => TransformAmounts(transform);

    /// <inheritdoc/>
    IImmutableMoneyBag IImmutableMoneyBag.TrimZeroAmounts() => TrimZeroAmounts();

    /// <inheritdoc/>
    void ICollection<MonetaryValue>.CopyTo(MonetaryValue[] array, int arrayIndex)
    {
        CollectionCopy.CheckParams(Count, array, arrayIndex);

        foreach (var value in this)
            array[arrayIndex++] = value;
    }

    /// <inheritdoc/>
    IEnumerator<MonetaryValue> IEnumerable<MonetaryValue>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Not Supported

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<MonetaryValue>.Add(MonetaryValue item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<MonetaryValue>.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ICollection<MonetaryValue>.Remove(MonetaryValue item) => throw new NotSupportedException();

    #endregion

    /// <summary>
    /// Enumerates the elements of a <see cref="ImmutableMoneyBag"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<MonetaryValue>
    {
        private ImmutableDictionary<Currency, decimal>.Enumerator _amountLookupEnumerator;

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public MonetaryValue Current => new(_amountLookupEnumerator.Current.Value, _amountLookupEnumerator.Current.Key);

        /// <inheritdoc/>
        object? IEnumerator.Current => Current;

        internal Enumerator(ImmutableDictionary<Currency, decimal> amountLookup)
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