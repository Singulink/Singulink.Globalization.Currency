using System.Runtime.CompilerServices;
using Singulink.Globalization.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents a bag of monetary values in one or more currencies.
/// </summary>
[CollectionBuilder(typeof(MoneyBagBuilder), nameof(MoneyBagBuilder.Create))]
public interface IMoneyBag : ICollection<MonetaryValue>, IReadOnlyMoneyBag
{
#if NET
    /// <summary>
    /// Creates a bag that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract new IMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values);
#endif

    /// <summary>
    /// Gets the number of values/currencies in this bag.
    /// </summary>
    public new int Count { get; }

    /// <summary>
    /// Adds the specified value to this bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public new void Add(MonetaryValue value);

    /// <summary>
    /// Adds the specified currency and amount to this bag.
    /// </summary>
    public void Add(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this bag.
    /// </summary>
    public void Add(decimal amount, Currency currency);

    /// <summary>
    /// Adds the specified values to this bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// If any of the specified currencies are not present in the bag's currency registry, an exception will be thrown.
    /// However, all valid currencies will be added prior to the exception being thrown.
    /// </remarks>
    public void AddRange(IEnumerable<MonetaryValue> values);

    /// <summary>
    /// Removes the value with the given currency code.
    /// </summary>
    public bool Remove(string currencyCode);

    /// <summary>
    /// Removes the value with the given currency.
    /// </summary>
    public bool Remove(Currency currency);

    /// <summary>
    /// Removes all the values from this bag that match the specified currencies.
    /// </summary>
    /// <remarks>
    /// If any of the specified currencies are not present in the bag's currency registry, an exception will be thrown.
    /// However, all valid currencies will be removed prior to the exception being thrown.
    /// </remarks>
    /// <exception cref="ArgumentException">"The following currencies are not present in the bag's currency registry.".</exception>
    public int RemoveAll(IEnumerable<Currency> currencies);

    /// <summary>
    /// Removes all the values from this bag that match the specified predicate and returns the resulting bag.
    /// </summary>
    public int RemoveAll(Func<MonetaryValue, bool> predicate);

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using <see cref="MidpointRounding.ToEven"/> midpoint rounding
    /// (i.e. "banker's rounding").
    /// </summary>
    public void RoundToCurrencyDigits();

    /// <summary>
    /// Rounds each value's amount to its currency's <see cref="Currency.DecimalDigits"/> using the specified midpoint rounding mode.
    /// </summary>
    public void RoundToCurrencyDigits(MidpointRounding mode);

    /// <summary>
    /// Sets the value this bag contains for the currency of the specified value.
    /// </summary>
    public void SetValue(MonetaryValue value);

    /// <summary>
    /// Sets the amount the bag contains for the specified currency code.
    /// </summary>
    public void SetAmount(decimal amount, string currencyCode);

    /// <summary>
    /// Sets the amount the bag contains for the specified currency code.
    /// </summary>
    public void SetAmount(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified value from this bag. Zero amounts are not trimmed from the bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public void Subtract(MonetaryValue value);

    /// <summary>
    /// Subtracts the specified currency and amount from this bag. Zero amounts are not trimmed from the bag.
    /// </summary>
    public void Subtract(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this bag.
    /// </summary>
    public void Subtract(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified values from this bag. Zero amounts are not trimmed from the bag.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public void SubtractRange(IEnumerable<MonetaryValue> values);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag.
    /// </summary>
    public void TransformValues(Func<MonetaryValue, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag. Values transformed to a <see langword="null"/> amount are removed.
    /// </summary>
    public void TransformValues(Func<MonetaryValue, decimal?> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag.
    /// </summary>
    public void TransformAmounts(Func<decimal, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this bag. Amounts transformed to a <see langword="null"/> amount are removed.
    /// </summary>
    public void TransformAmounts(Func<decimal, decimal?> transform);

    /// <summary>
    /// Removes all zero amounts from this bag.
    /// </summary>
    public int TrimZeroAmounts();
}