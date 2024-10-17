using System.Runtime.CompilerServices;
using Singulink.Globalization.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents a read-only bag of monetary values in one or more currencies.
/// </summary>
[CollectionBuilder(typeof(MoneyBagBuilder), nameof(MoneyBagBuilder.Create))]
public interface IReadOnlyMoneyBag : IReadOnlyCollection<MonetaryValue>, IFormattable
{
#if NET
    /// <summary>
    /// Creates a bag that uses the specified currency registry and adds all the specified values.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Attempted to add a value with a currency that is not available in the currency registry.
    /// </exception>
    public static abstract IReadOnlyMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values);
#endif

    /// <summary>
    /// Gets the value this bag contains with the specified currency code. Returns the default monetary value if it does not contain the currency.
    /// </summary>
    public MonetaryValue this[string currencyCode] { get; }

    /// <summary>
    /// Gets the value this bag contains of the specified currency. Returns the default monetary value if it does not contain the currency.
    /// </summary>
    public MonetaryValue this[Currency currency] { get; }

    /// <summary>
    /// Gets the currencies that this bag contains.
    /// </summary>
    public IReadOnlyCollection<Currency> Currencies { get; }

    /// <summary>
    /// Gets a value indicating whether this bag is sorted by each value's currency code.
    /// </summary>
    public bool IsSorted { get; }

    /// <summary>
    /// Gets the currency registry associated with this bag.
    /// </summary>
    CurrencyRegistry Registry { get; }

    /// <summary>
    /// Determines whether this bag contains the specified value.
    /// </summary>
    public bool Contains(MonetaryValue value);

    /// <summary>
    /// Determines whether this bag contains the specified amount and currency.
    /// </summary>
    public bool Contains(decimal amount, Currency currency);

    /// <summary>
    /// Determines whether this bag contains the specified amount and currency code.
    /// </summary>
    public bool Contains(decimal amount, string currencyCode);

    /// <summary>
    /// Determines whether this bag contains a value with the specified currency.
    /// </summary>
    public bool ContainsCurrency(Currency currency);

    /// <summary>
    /// Determines whether this bag contains a value with the specified currency code.
    /// </summary>
    public bool ContainsCurrency(string currencyCode);

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
    /// Gets the monetary value associated with the specified currency.
    /// </summary>
    /// <param name="currency">The currency of the value to get.</param>
    /// <param name="value">When the method returns, contains the value associated with the specified currency if a value for the currency was found;
    /// otherwise a default <see cref="MonetaryValue"/> value.</param>
    /// <returns><see langword="true"/> if a value for the currency was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(Currency currency, out MonetaryValue value);

    /// <summary>
    /// Gets the monetary value associated with the specified currency code.
    /// </summary>
    /// <param name="currencyCode">The currency code of the value to get.</param>
    /// <param name="value">When the method returns, contains the value associated with the specified currency code if a value for the currency was found;
    /// otherwise a default <see cref="MonetaryValue"/> value.</param>
    /// <returns><see langword="true"/> if a value for the currency code was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(string currencyCode, out MonetaryValue value);
}