using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents a collection of currencies.
/// </summary>
public sealed class CurrencyRegistry : ISet<Currency>, IReadOnlySet<Currency>
{
    private static CurrencyRegistry? _default;
    private static CurrencyRegistry? _system;

    private readonly string _name;
    private readonly HashSet<Currency> _currencies;
    private readonly Dictionary<string, Currency> _currencyLookup;
    private ImmutableArray<Currency> _orderedCurrencies;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyRegistry"/> class with the specified name and set of currencies.
    /// </summary>
    /// <exception cref="ArgumentException">The <paramref name="currencies"/> argument had multiple currencies with the same currency code.</exception>
    public CurrencyRegistry(string name, IEnumerable<Currency> currencies)
    {
        _name = name.Trim();

        if (_name.Length == 0)
            throw new ArgumentException("Name is required.", nameof(name));

        _currencies = [];
        _currencyLookup = new(StringComparer.OrdinalIgnoreCase);

        foreach (var currency in currencies)
        {
            if (_currencies.Add(currency) && !_currencyLookup.TryAdd(currency.CurrencyCode, currency))
                throw new ArgumentException($"Multiple currencies with currency code '{currency.CurrencyCode}'.", nameof(currencies));
        }
    }

    /// <summary>
    /// Gets or sets the default currency registry. The default registry can only be set once and cannot be set after it has been accessed. If it is not
    /// set explicitly then it gets set to the <see cref="System"/> registry on first access.
    /// </summary>
    /// <exception cref="InvalidOperationException">Attempt to set the default registry after it has already been set or accessed.</exception>
    /// <remarks>
    /// If you want to set a the default registry to something other than the <see cref="System"/> registry, you should do it as early as possible during
    /// application startup. Once it has been set or accessed, it cannot be set again.
    /// </remarks>
    public static CurrencyRegistry Default
    {
        get => _default ??= System;
        set {
            if (_default != null)
                throw new InvalidOperationException("Default currency registry cannot be set after it has already been set or accessed.");

            _default = value;
        }
    }

    /// <summary>
    /// Gets a currency registry built from system globalization data.
    /// </summary>
    public static CurrencyRegistry System => _system ??= Currency.CreateSystemRegistry();

    /// <summary>
    /// Gets a currency from this registry with the specified currency code.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid or unknown currency code.</exception>
    public Currency this[string currencyCode]
    {
        get {
            if (!TryGetCurrency(currencyCode, out var currency))
                Throw(currencyCode);

            return currency;

            [DoesNotReturn]
            void Throw(string currencyCode) => throw new ArgumentException($"Currency code '{currencyCode}' not found in the '{_name}' registry.", nameof(currencyCode));
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
    /// Gets a value indicating whether the set is read-only. Always returns true.
    /// </summary>
    bool ICollection<Currency>.IsReadOnly => true;

    /// <summary>
    /// Gets a currency from this registry with the specified currency code.
    /// </summary>
    public bool TryGetCurrency(string currencyCode, [MaybeNullWhen(false)] out Currency currency) => _currencyLookup.TryGetValue(currencyCode, out currency);

    /// <summary>
    /// Gets a value indicating whether this registry contains a currency with the specified currency code.
    /// </summary>
    public bool Contains(string currencyCode) => _currencyLookup.ContainsKey(currencyCode);

    /// <summary>
    /// Gets a value indicating whether this registry contains the specified currency.
    /// </summary>
    public bool Contains(Currency currency) => _currencies.Contains(currency);

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

    /// <summary>
    /// Copies the currencies in this registry to an array.
    /// </summary>
    void ICollection<Currency>.CopyTo(Currency[] array, int arrayIndex) => _currencies.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the currencies in this registry.
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

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator<Currency> IEnumerable<Currency>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Not Supported

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ISet<Currency>.Add(Currency item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.ExceptWith(IEnumerable<Currency> other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.IntersectWith(IEnumerable<Currency> other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.SymmetricExceptWith(IEnumerable<Currency> other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ISet<Currency>.UnionWith(IEnumerable<Currency> other) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Currency>.Add(Currency item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Currency>.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ICollection<Currency>.Remove(Currency item) => throw new NotSupportedException();

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
        public void Dispose() { }

        /// <inheritdoc cref="IEnumerator.MoveNext"/>
        public bool MoveNext() => _enumerator.MoveNext();

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">This operation is not supported.</exception>
        public void Reset() => throw new NotSupportedException();
    }
}