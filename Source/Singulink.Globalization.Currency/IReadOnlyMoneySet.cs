using System;
using System.Collections.Generic;
using System.Text;

namespace Singulink.Globalization;

/// <summary>
/// Represents a read-only set of <see cref="Money"/> values.
/// </summary>
public interface IReadOnlyMoneySet : IEnumerable<Money>
{
    /// <summary>
    /// Gets the value this set contains with the specified currency code. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[string currencyCode] { get; }

    /// <summary>
    /// Gets the value this set contains of the given currency. Returns the default money value if it does not contain the currency.
    /// </summary>
    public Money this[Currency currency] { get; }

    /// <summary>
    /// Gets the currency registry associated with this set.
    /// </summary>
    CurrencyRegistry Registry { get; }

    /// <summary>
    /// Gets the number of values in this set.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the currencies that this set contains.
    /// </summary>
    public IEnumerable<Currency> Currencies { get; }

    public bool TryGetAmount(Currency currency, out decimal amount);

    public bool TryGetAmount(string currencyCode, out decimal amount);

    public bool TryGetValue(Currency currency, out Money value);

    public bool TryGetValue(string currencyCode, out Money value);
}