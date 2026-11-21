using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Singulink.Globalization;

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
[JsonConverter(typeof(MonetaryValueJsonConverter))]
[TypeConverter(typeof(MonetaryValueTypeConverter))]
public readonly partial struct MonetaryValue : IComparable<MonetaryValue>, IComparable, IEquatable<MonetaryValue>
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

    // Not called by this library. Entity Framework Core binds a constructor to mapped properties by parameter name, and it also binds private constructors.
    // A complex property that maps CurrencyOrDefault (and not Currency) is therefore created through this constructor, so it can hold the default value.
    // The parameter names must stay equal to the property names, and the parameter order must differ from the public constructor.
    private MonetaryValue(Currency? currencyOrDefault, decimal amount)
    {
        this = CreateOrDefault(amount, currencyOrDefault);
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
    /// Deconstructs this value into its amount and its currency, i.e. <c>var (amount, currency) = value;</c>. The currency is <see langword="null"/> for the
    /// default value, as <see cref="CurrencyOrDefault"/> is.
    /// </summary>
    /// <param name="amount">The amount of this value.</param>
    /// <param name="currency">The currency of this value, or <see langword="null"/> if this is the default value.</param>
    public void Deconstruct(out decimal amount, out Currency? currency)
    {
        amount = _amount;
        currency = _currency;
    }

    /// <summary>
    /// Compares this value to the specified value. Values in different currencies cannot be compared.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid comparison between values that have different currencies.</exception>
    public int CompareTo(MonetaryValue other)
    {
        EnsureSameCurrencyForCompare(_currency, other._currency);
        return _amount.CompareTo(other._amount);
    }

    /// <summary>
    /// Compares this value to the specified object, which must be a <see cref="MonetaryValue"/> or <see langword="null"/>. Any value is greater than <see
    /// langword="null"/>. Values in different currencies cannot be compared.
    /// </summary>
    /// <exception cref="ArgumentException">The object is not a <see cref="MonetaryValue"/>, or the values have different currencies.</exception>
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not MonetaryValue other)
            throw new ArgumentException($"Object must be of type {nameof(MonetaryValue)}.", nameof(obj));

        return CompareTo(other);
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
    /// Returns a value in the same currency with the amount rounded to the specified number of decimal places using the specified midpoint rounding mode
    /// (which defaults to <see cref="MidpointRounding.ToEven"/> "banker's rounding"), whatever the currency's own decimal digits are.
    /// </summary>
    /// <param name="decimals">The number of decimal places to round the amount to, from <c>0</c> to <c>28</c>.</param>
    /// <param name="mode">The midpoint rounding mode.</param>
    /// <remarks>
    /// Use this method for amounts that are finer than the currency, such as a unit price or a rate. An amount that already has no more than the specified
    /// decimal places is returned unchanged.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimals"/> is less than <c>0</c> or greater than <c>28</c>.</exception>
    public MonetaryValue Round(int decimals, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (decimals is < 0 or > 28)
            throw new ArgumentOutOfRangeException(nameof(decimals), "Decimals must be between 0 and 28.");

        return _currency is null ? this : new MonetaryValue(Math.Round(_amount, decimals, mode), _currency);
    }

    /// <summary>
    /// Returns a value in the same currency with the amount rounded according to the specified rounding policy using the specified midpoint rounding
    /// mode (which defaults to <see cref="MidpointRounding.ToEven"/> "banker's rounding").
    /// </summary>
    /// <param name="policy">The rounding policy to round the amount to, including its rounding increment, such as a policy that rounds to <c>0.05</c>.</param>
    /// <param name="mode">The midpoint rounding mode.</param>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public MonetaryValue Round(RoundingPolicy policy, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (policy is null)
            throw new ArgumentNullException(nameof(policy));

        return _currency is null ? this : new MonetaryValue(policy.RoundAmount(_amount, mode), _currency);
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
    /// Returns a value in the same currency with the amount that the specified function returns for this value's amount.
    /// </summary>
    /// <param name="transform">The function that is called once with the amount and returns the new amount.</param>
    /// <remarks>
    /// <para>
    /// A value with a currency keeps its currency whatever the function returns, so a result of zero is a zero in that currency, not <see
    /// cref="Default"/>.</para>
    /// <para>
    /// The default value has no currency. The function is called with <c>0</c>: a result of zero returns the default value, and any other result throws
    /// the same exception as <see cref="CreateOrDefault(decimal, Currency?)"/> with a non-zero amount and no currency. Check <see cref="IsDefault"/> first
    /// if the function can turn zero into another amount.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="transform"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">This value is the default value and the function returns a non-zero amount.</exception>
    public MonetaryValue TransformAmount(Func<decimal, decimal> transform)
    {
        if (transform is null)
            throw new ArgumentNullException(nameof(transform));

        return CreateOrDefault(transform(_amount), _currency);
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