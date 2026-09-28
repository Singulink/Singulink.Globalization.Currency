using System.Collections.Immutable;
using System.Diagnostics;

namespace Singulink.Globalization;

/// <content>
/// Contains the allocation functionality for <see cref="MonetaryValue"/>.
/// </content>
partial struct MonetaryValue
{
    /// <summary>
    /// Allocates this value into the specified number of equal parts that are valid amounts under the currency's <see cref="Currency.RoundingPolicy"/>,
    /// distributing any remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="count">The number of parts to allocate the value into.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than <c>1</c>.</exception>
    /// <remarks>
    /// <inheritdoc cref="Allocate(ReadOnlySpan{decimal}, RoundingPolicy)" path="/remarks/*"/>
    /// </remarks>
    public ImmutableArray<MonetaryValue> Allocate(int count)
        => Allocate(count, _currency?.RoundingPolicy ?? RoundingPolicy.Default);

    /// <summary>
    /// Allocates this value into parts proportional to the specified ratios that are valid amounts under the currency's <see
    /// cref="Currency.RoundingPolicy"/>, distributing any remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="ratios">The relative weights of each part. Weights can be any non-negative values and do not need to sum to <c>1</c> or
    /// <c>100</c>.</param>
    /// <exception cref="ArgumentException"><paramref name="ratios"/> is empty, contains a negative value, or all values are zero.</exception>
    /// <remarks>
    /// <inheritdoc cref="Allocate(ReadOnlySpan{decimal}, RoundingPolicy)" path="/remarks/*"/>
    /// </remarks>
    public ImmutableArray<MonetaryValue> Allocate(params ReadOnlySpan<decimal> ratios)
        => Allocate(ratios, _currency?.RoundingPolicy ?? RoundingPolicy.Default);

    /// <summary>
    /// Allocates this value into the specified number of equal parts that are valid cash amounts under the currency's <see
    /// cref="Currency.CashRoundingPolicy"/>, distributing any remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="count">The number of parts to allocate the value into.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than <c>1</c>.</exception>
    /// <exception cref="NotSupportedException">Cash rounding rules are not available for the currency. See <see cref="Currency.CashRoundingPolicy"/> for
    /// more information.</exception>
    /// <remarks>
    /// <inheritdoc cref="Allocate(ReadOnlySpan{decimal}, RoundingPolicy)" path="/remarks/*"/>
    /// </remarks>
    public ImmutableArray<MonetaryValue> AllocateToCash(int count)
        => Allocate(count, _currency?.GetRequiredCashRoundingPolicy() ?? RoundingPolicy.Default);

    /// <summary>
    /// Allocates this value into parts proportional to the specified ratios that are valid cash amounts under the currency's <see
    /// cref="Currency.CashRoundingPolicy"/>, distributing any remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="ratios">The relative weights of each part. Weights can be any non-negative values and do not need to sum to <c>1</c> or
    /// <c>100</c>.</param>
    /// <exception cref="ArgumentException"><paramref name="ratios"/> is empty, contains a negative value, or all values are zero.</exception>
    /// <exception cref="NotSupportedException">Cash rounding rules are not available for the currency. See <see cref="Currency.CashRoundingPolicy"/> for
    /// more information.</exception>
    /// <remarks>
    /// <inheritdoc cref="Allocate(ReadOnlySpan{decimal}, RoundingPolicy)" path="/remarks/*"/>
    /// </remarks>
    public ImmutableArray<MonetaryValue> AllocateToCash(params ReadOnlySpan<decimal> ratios)
        => Allocate(ratios, _currency?.GetRequiredCashRoundingPolicy() ?? RoundingPolicy.Default);

    /// <summary>
    /// Allocates this value into the specified number of equal parts that are valid amounts under the specified rounding policy, distributing any
    /// remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="count">The number of parts to allocate the value into.</param>
    /// <param name="policy">The rounding policy that determines the smallest unit that parts are allocated in, i.e. a policy with <c>0</c> decimal digits
    /// allocates in whole units of the currency.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than <c>1</c>.</exception>
    /// <remarks>
    /// <inheritdoc cref="Allocate(ReadOnlySpan{decimal}, RoundingPolicy)" path="/remarks/*"/>
    /// </remarks>
    public ImmutableArray<MonetaryValue> Allocate(int count, RoundingPolicy policy)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1.");

        if (_currency is null)
            return CreateDefaultParts(count);

        return AllocateCore(count, null, policy);
    }

    /// <summary>
    /// Allocates this value into parts proportional to the specified ratios that are valid amounts under the specified rounding policy, distributing any
    /// remainder so that the parts always sum to this value rounded to the policy.
    /// </summary>
    /// <param name="ratios">The relative weights of each part. Weights can be any non-negative values and do not need to sum to <c>1</c> or
    /// <c>100</c>.</param>
    /// <param name="policy">The rounding policy that determines the smallest unit that parts are allocated in, i.e. a policy with <c>0</c> decimal digits
    /// allocates in whole units of the currency.</param>
    /// <exception cref="ArgumentException"><paramref name="ratios"/> is empty, contains a negative value, or all values are zero.</exception>
    /// <remarks>
    /// <para>
    /// The value is first rounded to the policy using <see cref="MidpointRounding.ToEven"/> rounding, and the parts always sum exactly to that rounded
    /// value. The remainder that results from rounding the individual parts down to the policy's smallest unit is distributed one unit at a time to the
    /// parts that lost the largest fraction (the "largest remainder" method), with ties resolved in favor of earlier parts. For equal allocations this
    /// means earlier parts receive the extra units, i.e. <c>USD 100.00</c> allocated into <c>3</c> parts yields <c>34.34</c>, <c>33.33</c> and
    /// <c>33.33</c>.
    /// </para>
    /// <para>
    /// Negative values are allocated the same way as their absolute value with the parts negated. Parts with a zero ratio are always zero. A default
    /// value is allocated into default values.
    /// </para>
    /// </remarks>
    public ImmutableArray<MonetaryValue> Allocate(ReadOnlySpan<decimal> ratios, RoundingPolicy policy)
    {
        if (ratios.Length is 0)
            throw new ArgumentException("At least one ratio must be specified.", nameof(ratios));

        decimal ratioSum = 0;

        foreach (decimal ratio in ratios)
        {
            if (ratio < 0)
                throw new ArgumentException("Ratios cannot be negative.", nameof(ratios));

            ratioSum += ratio;
        }

        if (ratioSum is 0)
            throw new ArgumentException("At least one ratio must be greater than zero.", nameof(ratios));

        if (_currency is null)
            return CreateDefaultParts(ratios.Length);

        return AllocateCore(ratios.Length, ratios.ToArray(), policy);
    }

    private ImmutableArray<MonetaryValue> AllocateCore(int count, decimal[]? ratios, RoundingPolicy policy)
    {
        Debug.Assert(_currency is not null, "Currency must be set.");

        decimal unit = policy.SmallestUnitAmount;
        decimal total = policy.RoundAmount(_amount);
        bool negative = total < 0;

        if (negative)
            total = -total;

        // Work in whole units of the policy so that all intermediate values are exact.

        decimal totalUnits = decimal.Truncate(total / unit);
        decimal ratioSum = 0;

        if (ratios is not null)
        {
            foreach (decimal ratio in ratios)
                ratioSum += ratio;
        }

        var units = new decimal[count];
        var remainders = new decimal[count];
        decimal allocatedUnits = 0;

        for (int i = 0; i < count; i++)
        {
            decimal share = ratios is null ? totalUnits / count : totalUnits * ratios[i] / ratioSum;
            units[i] = decimal.Truncate(share);
            remainders[i] = share - units[i];
            allocatedUnits += units[i];
        }

        // Distribute the leftover units one at a time to the parts with the largest remainders (ties go to earlier parts). The number of leftover units is
        // always less than the number of parts that have a non-zero remainder, so this loop terminates with every unit assigned.

        int leftover = (int)(totalUnits - allocatedUnits);

        while (leftover > 0)
        {
            int index = -1;

            for (int i = 0; i < count; i++)
            {
                if (remainders[i] > 0 && (index < 0 || remainders[i] > remainders[index]))
                    index = i;
            }

            Debug.Assert(index >= 0, "There must be a part with a remainder to receive a leftover unit.");

            units[index]++;
            remainders[index] = 0;
            leftover--;
        }

        var parts = ImmutableArray.CreateBuilder<MonetaryValue>(count);

        for (int i = 0; i < count; i++)
        {
            decimal amount = units[i] * unit;
            parts.Add(new MonetaryValue(negative ? -amount : amount, _currency));
        }

        return parts.MoveToImmutable();
    }

    private static ImmutableArray<MonetaryValue> CreateDefaultParts(int count)
    {
        var parts = ImmutableArray.CreateBuilder<MonetaryValue>(count);

        for (int i = 0; i < count; i++)
            parts.Add(default);

        return parts.MoveToImmutable();
    }
}
