using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents an immutable bag of monetary values in one or more currencies.
/// </summary>
[CollectionBuilder(typeof(ImmutableMoneyBag), nameof(ImmutableMoneyBag.Create))]
public interface IImmutableMoneyBag : ICollection<MonetaryValue>, IReadOnlyMoneyBag
{
#if NET
    /// <summary>
    /// Creates a bag that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract new IImmutableMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values);
#endif

    /// <summary>
    /// Gets the number of values in this bag.
    /// </summary>
    public new int Count { get; }

    /// <summary>
    /// Adds the specified value to this bag and returns the resulting bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public new IImmutableMoneyBag Add(MonetaryValue value);

    /// <summary>
    /// Adds the specified currency and amount to this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Add(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Add(decimal amount, Currency currency);

    /// <summary>
    /// Adds the specified values to this bag and returns the resulting bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneyBag AddRange(IEnumerable<MonetaryValue> values);

    /// <summary>
    /// Returns an empty immutable bag that has the same currency registry as this bag.
    /// </summary>
    public new IImmutableMoneyBag Clear();

    /// <summary>
    /// Removes the value with the given currency code and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Remove(string currencyCode);

    /// <summary>
    /// Removes the value with the given currency and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Remove(Currency currency);

    /// <summary>
    /// Removes all the values from this bag that match the specified currencies and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag RemoveAll(IEnumerable<Currency> currencies);

    /// <summary>
    /// Removes all the values from this bag that match the specified predicate and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag RemoveAll(Func<MonetaryValue, bool> predicate);

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using <see cref="MidpointRounding.ToEven"/> midpoint rounding
    /// (i.e. "banker's rounding") and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag RoundToCurrencyDigits();

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using the specified midpoint rounding mode and returns the resulting
    /// bag.
    /// </summary>
    public IImmutableMoneyBag RoundToCurrencyDigits(MidpointRounding mode);

    /// <summary>
    /// Sets the value this bag contains for the currency of the specified value and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag SetValue(MonetaryValue value);

    /// <summary>
    /// Sets the amount the bag contains for the specified currency code and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag SetAmount(decimal amount, string currencyCode);

    /// <summary>
    /// Sets the amount the bag contains for the specified currency code and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag SetAmount(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified value from this bag and returns the resulting bag. Zero amounts are not trimmed from the bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneyBag Subtract(MonetaryValue value);

    /// <summary>
    /// Subtracts the specified currency and amount from this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Subtract(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag Subtract(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified values from this bag and returns the resulting bag. Zero amounts are not trimmed from the bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public IImmutableMoneyBag SubtractRange(IEnumerable<MonetaryValue> values);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag TransformValues(Func<MonetaryValue, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag and returns the resulting bag. Values transformed to a <see langword="null"/>
    /// amount are removed.
    /// </summary>
    public IImmutableMoneyBag TransformValues(Func<MonetaryValue, decimal?> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag TransformAmounts(Func<decimal, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag and returns the resulting bag. Amounts transformed to a <see langword="null"/>
    /// amount are removed.
    /// </summary>
    public IImmutableMoneyBag TransformAmounts(Func<decimal, decimal?> transform);

    /// <summary>
    /// Removes all zero amounts from this bag and returns the resulting bag.
    /// </summary>
    public IImmutableMoneyBag TrimZeroAmounts();
}