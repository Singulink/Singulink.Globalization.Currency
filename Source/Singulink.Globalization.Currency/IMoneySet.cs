using System.Runtime.CompilerServices;
using Singulink.Globalization.Internal;

namespace Singulink.Globalization;

/// <summary>
/// Represents a set of <see cref="Money"/> values.
/// </summary>
[CollectionBuilder(typeof(MoneySetBuilder), nameof(MoneySetBuilder.Create))]
public interface IMoneySet : ICollection<Money>, IReadOnlyMoneySet
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a set that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract new IMoneySet Create(CurrencyRegistry registry, IEnumerable<Money> values);
#endif

    /// <summary>
    /// Gets the number of values in this set.
    /// </summary>
    public new int Count { get; }

    /// <summary>
    /// Adds the specified value to this set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public new void Add(Money value);

    /// <summary>
    /// Adds the specified currency and amount to this set.
    /// </summary>
    public void Add(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this set.
    /// </summary>
    public void Add(decimal amount, Currency currency);

    /// <summary>
    /// Adds the specified values to this set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// If any of the specified currencies are not present in the set's currency registry, an exception will be thrown.
    /// However, all valid currencies will be added prior to the exception being thrown.
    /// </remarks>
    public void AddRange(IEnumerable<Money> values);

    /// <summary>
    /// Removes the value with the given currency code.
    /// </summary>
    public bool Remove(string currencyCode);

    /// <summary>
    /// Removes the value with the given currency.
    /// </summary>
    public bool Remove(Currency currency);

    /// <summary>
    /// Removes all the values from this set that match the specified currencies.
    /// </summary>
    /// <remarks>
    /// If any of the specified currencies are not present in the set's currency registry, an exception will be thrown.
    /// However, all valid currencies will be removed prior to the exception being thrown.
    /// </remarks>
    /// <exception cref="ArgumentException">"The following currencies are not present in the set's currency registry.".</exception>
    public int RemoveAll(IEnumerable<Currency> currencies);

    /// <summary>
    /// Removes all the values from this set that match the specified predicate and returns the resulting set.
    /// </summary>
    public int RemoveAll(Func<Money, bool> predicate);

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
    /// Sets the value this set contains for the currency of the specified value.
    /// </summary>
    public void SetValue(Money value);

    /// <summary>
    /// Sets the amount the set contains for the specified currency code.
    /// </summary>
    public void SetAmount(decimal amount, string currencyCode);

    /// <summary>
    /// Sets the amount the set contains for the specified currency code.
    /// </summary>
    public void SetAmount(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified value from this set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public void Subtract(Money value);

    /// <summary>
    /// Subtracts the specified currency and amount from this set.
    /// </summary>
    public void Subtract(decimal amount, string currencyCode);

    /// <summary>
    /// Adds the specified currency and amount to this set.
    /// </summary>
    public void Subtract(decimal amount, Currency currency);

    /// <summary>
    /// Subtracts the specified values from this set. Zero amounts are not trimmed from the set.
    /// </summary>
    /// <remarks>
    /// Default values that are not associated with any currency are ignored.
    /// </remarks>
    public void SubtractRange(IEnumerable<Money> values);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set.
    /// </summary>
    public void TransformValues(Func<Money, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set. Values transformed to a <see langword="null"/> amount are removed.
    /// </summary>
    public void TransformValues(Func<Money, decimal?> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set.
    /// </summary>
    public void TransformAmounts(Func<decimal, decimal> transform);

    /// <summary>
    /// Applies the specified transformation to each value's amount in this set. Amounts transformed to a <see langword="null"/> amount are removed.
    /// </summary>
    public void TransformAmounts(Func<decimal, decimal?> transform);

    /// <summary>
    /// Removes all zero amounts from this set.
    /// </summary>
    public int TrimZeroAmounts();
}