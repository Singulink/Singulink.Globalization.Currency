using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

#pragma warning disable SA1203 // Constants should appear before fields

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
public readonly partial struct Money : IFormattable, IComparable<Money>, IEquatable<Money>
{
    /// <summary>
    /// Gets the default <see cref="Money"/> value, which is not associated with any currency and has a zero amount.
    /// </summary>
    public static Money Default => default;

    private readonly decimal _amount;
    private readonly Currency? _currency;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct with the specified amount and currency code.
    /// </summary>
    public Money(decimal amount, string currencyCode) : this(amount, Currency.Get(currencyCode)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct with the specified amount and currency.
    /// </summary>
    public Money(decimal amount, Currency currency)
    {
        _amount = amount;
        _currency = currency;
    }

    /// <summary>
    /// Creates a new <see cref="Money"/> value with the specified amount and currency code.
    /// </summary>
    public static Money Create(decimal amount, string currencyCode) => new(amount, currencyCode);

    /// <summary>
    /// Creates a new <see cref="Money"/> value with the specified amount and currency.
    /// </summary>
    public static Money Create(decimal amount, Currency currency) => new(amount, currency);

    /// <summary>
    /// Creates a new <see cref="Money"/> value with the specified amount and currency code. Allows creating default money values by passing <c>0</c> for
    /// the amount and <see langword="null"/> for the currency code. Currency code must be provided if the amount is non-zero.
    /// </summary>
    public static Money CreateDefaultable(decimal amount, string? currencyCode) => CreateDefaultable(amount, currencyCode == null ? null : Currency.Get(currencyCode));

    /// <summary>
    /// Creates a new <see cref="Money"/> value with the specified amount and currency. Allows creating default money values by passing <c>0</c> for
    /// the amount and <see langword="null"/> for the currency. Currency must be provided if the amount is non-zero.
    /// </summary>
    public static Money CreateDefaultable(decimal amount, Currency? currency)
    {
        if (currency == null)
        {
            if (amount != 0)
                ThrowCurrencyRequiredForNonZeroAmount();

            return default;
        }

        return new Money(amount, currency);
    }

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
                static void Throw() => throw new InvalidOperationException("Default money values do not have a currency associated with them.");
                Throw();
            }

            return _currency;
        }
    }

    /// <summary>
    /// Gets the currency associated with this value or <see langword="null"/> if this is a default money value with no currency associated with it.
    /// </summary>
    public Currency? CurrencyOrDefault => _currency;

    /// <summary>
    /// Gets the amount this value represents in its currency.
    /// </summary>
    public decimal Amount => _amount;

    /// <summary>
    /// Gets a value indicating whether this is a default value. Default values have a <c>0</c> amount and do not have a currency associated with them.
    /// </summary>
    [MemberNotNullWhen(false, nameof(CurrencyOrDefault))]
    [MemberNotNullWhen(false, nameof(_currency))]
    public bool IsDefault => _currency == null;

    #region Operators

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static bool operator ==(Money x, Money y) => x.Equals(y);

    public static bool operator !=(Money x, Money y) => !x.Equals(y);

    public static bool operator <(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount < y._amount;
    }

    public static bool operator >(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount > y._amount;
    }

    public static bool operator <=(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount <= y._amount;
    }

    public static bool operator >=(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount >= y._amount;
    }

    public static Money operator +(Money x, Money y) => CreateDefaultable(x._amount + y._amount, CombineCurrencies(x._currency, y._currency));

    public static Money operator +(Money x, decimal y) => CreateDefaultable(x._amount + y, x._currency);

    public static Money operator +(decimal x, Money y) => y + x;

    public static Money operator -(Money x, Money y) => CreateDefaultable(x._amount - y._amount, CombineCurrencies(x._currency, y._currency));

    public static Money operator -(Money x, decimal y) => CreateDefaultable(x._amount - y, x._currency);

    public static Money operator -(decimal x, Money y) => CreateDefaultable(x - y._amount, y._currency);

    public static Money operator *(Money x, decimal y) => CreateDefaultable(x._amount * y, x._currency);

    public static Money operator *(decimal x, Money y) => y * x;

    public static Money operator /(Money x, decimal y) => CreateDefaultable(x._amount / y, x._currency);

    public static Money operator /(decimal x, Money y) => CreateDefaultable(x / y._amount, y._currency);

    public static Money operator ++(Money value)
    {
        if (value.IsDefault)
            ThrowCurrencyRequiredForNonZeroAmount();

        return new(value.Amount + 1, value._currency);
    }

    public static Money operator --(Money value)
    {
        if (value.IsDefault)
            ThrowCurrencyRequiredForNonZeroAmount();

        return new(value.Amount - 1, value._currency);
    }

    public static Money operator +(Money value) => value;

    public static Money operator -(Money value)
    {
        if (value.IsDefault)
            return default;

        return new(-value.Amount, value._currency);
    }

#pragma warning restore CS1591

    #endregion

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using <see cref="MidpointRounding.ToEven"/> midpoint rounding ("banker's
    /// rounding").
    /// </summary>
    public Money RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using the specified midpoint rounding mode.
    /// </summary>
    public Money RoundToCurrencyDigits(MidpointRounding mode)
    {
        return _currency == null ? this : new Money(Math.Round(_amount, _currency.DecimalDigits, mode), Currency);
    }

    /// <summary>
    /// Returns <see cref="Default"/> if this value's <see cref="Amount"/> is <c>0</c>, otherwise returns this value.
    /// </summary>
    public Money ToDefaultIfZero() => _amount == 0 ? default : this;

    #region Equality and Comparison

    /// <summary>
    /// Compares this value to the specified value. Values in different currencies are ordered by their currencies first.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid comparison between values that have different currencies.</exception>
    public int CompareTo(Money other)
    {
        EnsureSameCurrencyForCompare(_currency, other._currency);
        return _amount.CompareTo(other._amount);
    }

    /// <summary>
    /// Determines whether the two specified values are equal.
    /// </summary>
    public static bool Equals(Money x, Money y) => x.Equals(y);

    /// <summary>
    /// Determines whether this value is equal to the specifiied object.
    /// </summary>
    public override bool Equals(object? obj) => obj is Money value && Equals(value);

    /// <summary>
    /// Determines whether this value is equal to the specified value.
    /// </summary>
    public bool Equals(Money value) => _currency == value._currency && _amount == value._amount;

    /// <summary>
    /// Gets the hash code for this value.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(_currency, _amount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureSameCurrencyForCompare(Currency? x, Currency? y)
    {
        if (x != y)
        {
            static void Throw() => throw new ArgumentException("Currencies must match in order to compare money values.");
            Throw();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Currency? CombineCurrencies(Currency? x, Currency? y)
    {
        if (x == y)
            return x;

        if (x is null)
            return y;

        if (y is not null)
        {
            static void Throw() => throw new ArgumentException("Currencies must match (or one of the values can be a default value that has no currency associated with it).");
            Throw();
        }

        return x;
    }

    [DoesNotReturn]
    private static void ThrowCurrencyRequiredForNonZeroAmount() => throw new ArgumentException("Non-zero amount money values must have a currency associated with them.");

    #endregion
}