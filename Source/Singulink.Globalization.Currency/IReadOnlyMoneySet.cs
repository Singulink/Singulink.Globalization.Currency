using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents a read-only set of <see cref="Money"/> values.
/// </summary>
[CollectionBuilder(typeof(MoneySetBuilder), nameof(MoneySetBuilder.Create))]
public interface IReadOnlyMoneySet : IEnumerable<Money>, IFormattable
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a set that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract IReadOnlyMoneySet Create(CurrencyRegistry registry, IEnumerable<Money> values);
#endif

    /// <summary>
    /// Gets the value this set contains with the specified currency code. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[string currencyCode] { get; }

    /// <summary>
    /// Gets the value this set contains of the given currency. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[Currency currency] { get; }

    /// <summary>
    /// Gets the number of values in this set.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the currencies that this set contains.
    /// </summary>
    public IEnumerable<Currency> Currencies { get; }

    /// <summary>
    /// Gets a value indicating whether this set is sorted by each value's currency code.
    /// </summary>
    public bool IsSorted { get; }

    /// <summary>
    /// Gets the currency registry associated with this set.
    /// </summary>
    CurrencyRegistry Registry { get; }

    /// <summary>
    /// Gets the amount associated with the specified currency.
    /// </summary>
    /// <param name="currency">The currency of the amount to get.</param>
    /// <param name="amount">When the method returns, contains the amount associated with the specified currency if an amount for the currency was found;
    /// otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> if an amount for the currency was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetAmount(Currency currency, out decimal amount);

    /// <summary>
    /// Gets the amount associated with the specified currency code.
    /// </summary>
    /// <param name="currencyCode">The currency code of the amount to get.</param>
    /// <param name="amount">When the method returns, contains the amount associated with the specified currency code if an amount for the currency code was
    /// found; otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> if an amount for the currency code was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetAmount(string currencyCode, out decimal amount);

    /// <summary>
    /// Gets the money value associated with the specified currency.
    /// </summary>
    /// <param name="currency">The currency of the value to get.</param>
    /// <param name="value">When the method returns, contains the value associated with the specified currency if a value for the currency was found;
    /// otherwise a default <see cref="Money"/> value.</param>
    /// <returns><see langword="true"/> if a value for the currency was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(Currency currency, out Money value);

    /// <summary>
    /// Gets the money value associated with the specified currency code.
    /// </summary>
    /// <param name="currencyCode">The currency code of the value to get.</param>
    /// <param name="value">When the method returns, contains the value associated with the specified currency code if a value for the currency was found;
    /// otherwise a default <see cref="Money"/> value.</param>
    /// <returns><see langword="true"/> if a value for the currency code was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(string currencyCode, out Money value);
}