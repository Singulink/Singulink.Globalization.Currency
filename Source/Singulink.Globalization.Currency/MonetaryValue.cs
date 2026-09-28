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
    public static MonetaryValue CreateOrDefault(decimal amount, string? currencyCode) => CreateOrDefault(amount, currencyCode is null ? null : Currency.GetCurrency(currencyCode));

    /// <summary>
    /// Creates a new <see cref="MonetaryValue"/> value with the specified amount and currency. Allows creating default monetary values by passing <c>0</c> for
    /// the amount and <see langword="null"/> for the currency. Currency must be provided if the amount is non-zero.
    /// </summary>
    public static MonetaryValue CreateOrDefault(decimal amount, Currency? currency)
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
    /// Returns a value rounded according to the currency's <see cref="Currency.RoundingPolicy"/> using the specified midpoint rounding mode (which
    /// defaults to <see cref="MidpointRounding.ToEven"/> "banker's rounding").
    /// </summary>
    public MonetaryValue Round(MidpointRounding mode = MidpointRounding.ToEven)
    {
        return _currency is null ? this : new MonetaryValue(_currency.RoundingPolicy.RoundAmount(_amount, mode), _currency);
    }

    /// <summary>
    /// Returns a value rounded according to the currency's <see cref="Currency.CashRoundingPolicy"/> using the specified midpoint rounding mode (which
    /// defaults to <see cref="MidpointRounding.ToEven"/> "banker's rounding").
    /// </summary>
    /// <exception cref="NotSupportedException">Cash rounding rules are not available for the currency. See <see cref="Currency.CashRoundingPolicy"/> for
    /// more information.</exception>
    public MonetaryValue RoundToCash(MidpointRounding mode = MidpointRounding.ToEven)
    {
        return _currency is null ? this : new MonetaryValue(_currency.GetRequiredCashRoundingPolicy().RoundAmount(_amount, mode), _currency);
    }

    /// <summary>
    /// Returns <see cref="Default"/> if this value's <see cref="Amount"/> is <c>0</c>, otherwise returns this value.
    /// </summary>
    public MonetaryValue ToDefaultIfZero() => _amount is 0 ? default : this;

    /// <summary>
    /// Returns a value with the absolute value of this value's amount in the same currency.
    /// </summary>
    public MonetaryValue Abs() => _amount < 0 ? new MonetaryValue(-_amount, _currency!) : this;

    /// <summary>
    /// Returns the smaller of two monetary values.
    /// </summary>
    /// <exception cref="ArgumentException">The values have different currencies.</exception>
    public static MonetaryValue Min(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount <= y._amount ? x : y;
    }

    /// <summary>
    /// Returns the larger of two monetary values.
    /// </summary>
    /// <exception cref="ArgumentException">The values have different currencies.</exception>
    public static MonetaryValue Max(MonetaryValue x, MonetaryValue y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount >= y._amount ? x : y;
    }

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