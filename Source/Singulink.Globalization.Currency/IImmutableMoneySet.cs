using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable set of <see cref="Money"/> values.
/// </summary>
[CollectionBuilder(typeof(ImmutableMoneySet), nameof(ImmutableMoneySet.Create))]
public interface IImmutableMoneySet : ICollection<Money>, IReadOnlyMoneySet
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a set that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract new IImmutableMoneySet Create(CurrencyRegistry registry, IEnumerable<Money> values);
#endif

    /// <summary>
    /// Gets the number of values in this set.
    /// </summary>
    public new int Count { get; }

    /// <summary>
    /// Adds the specified value to this set and returns the resulting set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public new IImmutableMoneySet Add(Money value);

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Add(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Add(decimal amount, Currency currency);

    /// <summary>
    /// Adds the specified values to this set and returns the resulting set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneySet AddRange(IEnumerable<Money> values);

    /// <summary>
    /// Returns an empty immutable set that has the same currency registry as this set.
    /// </summary>
    public new IImmutableMoneySet Clear();

    /// <summary>
    /// Removes the value with the given currency code and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Remove(string currencyCode);

    /// <summary>
    /// Removes the value with the given currency and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Remove(Currency currency);

    /// <summary>
    /// Removes all the values from this set that match the specified currencies and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet RemoveAll(IEnumerable<Currency> currencies);

    /// <summary>
    /// Removes all the values from this set that match the specified predicate and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet RemoveAll(Func<Money, bool> predicate);

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using <see cref="MidpointRounding.ToEven"/> midpoint rounding
    /// (i.e. "banker's rounding") and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet RoundToCurrencyDigits();

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using the specified midpoint rounding mode and returns the resulting
    /// set.
    /// </summary>
    public IImmutableMoneySet RoundToCurrencyDigits(MidpointRounding mode);

    /// <summary>
    /// Sets the value this set contains for the currency of the specified value and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet SetValue(Money value);

    /// <summary>
    /// Sets the amount the set contains for the specified currency code and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet SetAmount(decimal amount, string currencyCode);

    /// <summary>
    /// Sets the amount the set contains for the specified currency code and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet SetAmount(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified value from this set and returns the resulting set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneySet Subtract(Money value);

    /// <summary>
    /// Subtracts the specified currency and amount from this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Subtract(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet Subtract(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified values from this set and returns the resulting set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneySet SubtractRange(IEnumerable<Money> values);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet TransformValues(Func<Money, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set and returns the resulting set. Values transformed to a <see langword="null"/>
    /// amount are removed.
    /// </summary>
    public IImmutableMoneySet TransformValues(Func<Money, decimal?> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet TransformAmounts(Func<decimal, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set and returns the resulting set. Amounts transformed to a <see langword="null"/>
    /// amount are removed.
    /// </summary>
    public IImmutableMoneySet TransformAmounts(Func<decimal, decimal?> transform);

    /// <summary>
    /// Removes all zero amounts from this set and returns the resulting set.
    /// </summary>
    public IImmutableMoneySet TrimZeroAmounts();

    #region Explicit Interface Implementations

    /// <summary>
    /// Gets a value indicating whether the set is read-only. Always returns <see langword="true"/>.
    /// </summary>
    bool ICollection<Money>.IsReadOnly => true;

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Money>.Add(Money item) => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    void ICollection<Money>.Clear() => throw new NotSupportedException();

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">This operation is not supported.</exception>
    bool ICollection<Money>.Remove(Money item) => throw new NotSupportedException();

    #endregion
}