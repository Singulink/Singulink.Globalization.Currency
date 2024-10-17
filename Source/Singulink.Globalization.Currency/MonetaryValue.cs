using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
public readonly partial struct MonetaryValue : IComparable<MonetaryValue>, IEquatable<MonetaryValue>
{
    /// <summary>
    /// Gets the default <see cref="MonetaryValue"/> value, which is not associated with any currency and has a zero amount.
    /// </summary>
    public static MonetaryValue Default => default;

    private readonly decimal _amount;
    private readonly Currency? _currency;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValue"/> struct with the specified amount and currency code from the <see cref="CurrencyRegistry.Default"/>
    /// currency registry.
    /// </summary>
    public MonetaryValue(decimal amount, string currencyCode) : this(amount, Currency.GetCurrency(currencyCode)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValue"/> struct with the specified amount and currency.
    /// </summary>
    public MonetaryValue(decimal amount, Currency currency)
    {
        _amount = amount;
        _currency = currency;
    }

    /// <summary>
    /// Creates a new <see cref="MonetaryValue"/> value with the specified amount and currency code from the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    public static MonetaryValue Create(decimal amount, string currencyCode) => new(amount, currencyCode);

    /// <summary>
    /// Creates a new <see cref="MonetaryValue"/> value with the specified amount and currency.
    /// </summary>
    public static MonetaryValue Create(decimal amount, Currency currency) => new(amount, currency);

    /// <summary>
    /// Creates a new <see cref="MonetaryValue"/> value with the specified amount and currency code from the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// Allows creating default monetary values by passing <c>0</c> for the amount and <see langword="null"/> for the currency code. Currency code must be provided
    /// if the amount is non-zero.
    /// </summary>
    public static MonetaryValue CreateDefaultable(decimal amount, string? currencyCode) => CreateDefaultable(amount, currencyCode is null ? null : Currency.GetCurrency(currencyCode));

    /// <summary>
    /// Creates a new <see cref="MonetaryValue"/> value with the specified amount and currency. Allows creating default monetary values by passing <c>0</c> for
    /// the amount and <see langword="null"/> for the currency. Currency must be provided if the amount is non-zero.
    /// </summary>
    public static MonetaryValue CreateDefaultable(decimal amount, Currency? currency)
    {
        if (currency is null)
        {
            if (amount != 0)
            {
                static void Throw() => throw new ArgumentException("Non-zero amount monetary values must have a currency associated with them.");
                Throw();
            }

            return default;
        }

        return new MonetaryValue(amount, currency);
    }

    /// <summary>
    /// Gets the amount this value represents in its currency.
    /// </summary>
    public decimal Amount => _amount;

    /// <summary>
    /// Gets the currency associated with this value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Attempted to get the currency on a default value which has no currency associated with it.
    /// </exception>
    public Currency Currency
    {
        get {
            if (_currency is null)
            {
                [DoesNotReturn]
                static void Throw() => throw new InvalidOperationException("Default monetary values do not have a currency associated with them.");
                Throw();
            }

            return _currency;
        }
    }

    /// <summary>
    /// Gets the currency associated with this value or <see langword="null"/> if this is a default monetary value with no currency associated with it.
    /// </summary>
    public Currency? CurrencyOrDefault => _currency;

    /// <summary>
    /// Gets a value indicating whether this is a default value. Default values have a <c>0</c> amount and do not have a currency associated with them.
    /// </summary>
    [MemberNotNullWhen(false, nameof(CurrencyOrDefault))]
    [MemberNotNullWhen(false, nameof(_currency))]
    public bool IsDefault => _currency is null;

    /// <summary>
    /// Compares this value to the specified value. Values in different currencies are ordered by their currencies first.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid comparison between values that have different currencies.</exception>
    public int CompareTo(MonetaryValue other)
    {
        EnsureSameCurrencyForCompare(_currency, other._currency);
        return _amount.CompareTo(other._amount);
    }

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using <see cref="MidpointRounding.ToEven"/> midpoint rounding ("banker's
    /// rounding").
    /// </summary>
    public MonetaryValue RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using the specified midpoint rounding mode.
    /// </summary>
    public MonetaryValue RoundToCurrencyDigits(MidpointRounding mode)
    {
        return _currency is null ? this : new MonetaryValue(Math.Round(_amount, _currency.DecimalDigits, mode), Currency);
    }

    /// <summary>
    /// Returns <see cref="Default"/> if this value's <see cref="Amount"/> is <c>0</c>, otherwise returns this value.
    /// </summary>
    public MonetaryValue ToDefaultIfZero() => _amount is 0 ? default : this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureSameCurrencyForCompare(Currency? x, Currency? y)
    {
        if (x != y)
        {
            static void Throw() => throw new ArgumentException("Currencies must match in order to compare monetary values.");
            Throw();
        }
    }

    #region Equals and GetHashCode

    /// <summary>
    /// Determines whether the two specified values are equal.
    /// </summary>
    public static bool Equals(MonetaryValue x, MonetaryValue y) => x.Equals(y);

    /// <summary>
    /// Determines whether this value is equal to the specified object.
    /// </summary>
    public override bool Equals(object? obj) => obj is MonetaryValue value && Equals(value);

    /// <summary>
    /// Determines whether this value is equal to the specified value.
    /// </summary>
    public bool Equals(MonetaryValue value) => _currency == value._currency && _amount == value._amount;

    /// <summary>
    /// Gets the hash code for this value.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(_currency, _amount);

    #endregion
}