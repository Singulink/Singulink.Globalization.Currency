using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <summary>
/// Represents the rounding rules for monetary amounts of a currency, consisting of the number of decimal digits and an optional rounding increment
/// expressed in units of the last decimal digit.
/// </summary>
/// <remarks>
/// <para>
/// A policy with <c>2</c> decimal digits and no rounding increment rounds amounts to the nearest <c>0.01</c>. A policy with <c>2</c> decimal digits and a
/// rounding increment of <c>5</c> rounds amounts to the nearest <c>0.05</c>, which is how cash amounts are rounded for currencies such as the Canadian
/// dollar and Swiss franc. A policy with <c>0</c> decimal digits rounds amounts to whole units.
/// </para>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class RoundingPolicy : IEquatable<RoundingPolicy>
{
    private decimal? _smallestUnitAmount;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingPolicy"/> class.
    /// </summary>
    /// <param name="decimalDigits">The number of decimal digits that amounts are rounded to.</param>
    /// <param name="decimalUnits">The rounding increment in units of the last decimal digit. A value of <c>0</c> or <c>1</c> indicates no additional
    /// rounding beyond the number of decimal digits.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalDigits"/> is less than <c>0</c> or greater than <c>28</c>, or
    /// <paramref name="decimalUnits"/> is less than <c>0</c>.</exception>
    public RoundingPolicy(int decimalDigits, int decimalUnits = 0)
    {
        if (decimalDigits is < 0 or > 28)
            throw new ArgumentOutOfRangeException(nameof(decimalDigits), "Decimal digits must be between 0 and 28.");

        if (decimalUnits < 0)
            throw new ArgumentOutOfRangeException(nameof(decimalUnits), "Decimal units cannot be negative.");

        DecimalDigits = decimalDigits;
        DecimalUnits = decimalUnits is 1 ? 0 : decimalUnits;
    }

    /// <summary>
    /// Gets the default rounding policy, which rounds amounts to <c>2</c> decimal digits with no additional rounding increment.
    /// </summary>
    public static RoundingPolicy Default { get; } = new(2);

    /// <summary>
    /// Gets the number of decimal digits that amounts are rounded to.
    /// </summary>
    public int DecimalDigits { get; }

    /// <summary>
    /// Gets the rounding increment in units of the last decimal digit, where a unit is the smallest amount that can be represented with the number of
    /// <see cref="DecimalDigits"/>. A value of <c>0</c> indicates no additional rounding beyond the number of decimal digits.
    /// </summary>
    /// <remarks>
    /// For example, a value of <c>5</c> when <see cref="DecimalDigits"/> is <c>2</c> means amounts are rounded to the nearest <c>0.05</c>. This property
    /// never returns <c>1</c> since that is equivalent to <c>0</c> and is coerced to <c>0</c> when the policy is created.
    /// </remarks>
    public int DecimalUnits { get; }

    /// <summary>
    /// Gets the smallest amount that is a valid multiple under this policy, based on <see cref="DecimalDigits"/> and <see cref="DecimalUnits"/>.
    /// </summary>
    public decimal SmallestUnitAmount
    {
        get {
            return _smallestUnitAmount ??= Compute();

            [MethodImpl(MethodImplOptions.NoInlining)]
            decimal Compute()
            {
                decimal amount = new(1, 0, 0, false, (byte)DecimalDigits);

                if (DecimalUnits > 0)
                    amount *= DecimalUnits;

                return amount;
            }
        }
    }

    /// <summary>
    /// Returns the specified amount rounded according to this policy using the specified midpoint rounding mode, which defaults to <see
    /// cref="MidpointRounding.ToEven"/> ("banker's rounding") if not specified.
    /// </summary>
    public decimal RoundAmount(decimal amount, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (DecimalUnits is 0)
            return Math.Round(amount, DecimalDigits, mode);

        decimal unitAmount = SmallestUnitAmount;
        return Math.Round(amount / unitAmount, 0, mode) * unitAmount;
    }

    /// <summary>
    /// Determines whether the specified policy has the same decimal digits and decimal units as this policy.
    /// </summary>
    public bool Equals(RoundingPolicy? other) => other is not null && DecimalDigits == other.DecimalDigits && DecimalUnits == other.DecimalUnits;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RoundingPolicy);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(DecimalDigits, DecimalUnits);

    /// <summary>
    /// Returns a string describing the smallest unit amount of this policy, i.e. <c>"0.01"</c> or <c>"0.05"</c>.
    /// </summary>
    public override string ToString() => SmallestUnitAmount.ToString(CultureInfo.InvariantCulture);
}
