namespace Singulink.Globalization;

/// <content>
/// Contains the conversions of <see cref="MonetaryValue"/> to and from minor units.
/// </content>
partial struct MonetaryValue
{
    /// <summary>
    /// Creates a value from an amount in minor units of the currency with the specified currency code from the <see cref="CurrencyRegistry.Default"/>
    /// registry, i.e. <c>1234</c> cents for <c>USD 12.34</c>.
    /// </summary>
    /// <inheritdoc cref="FromMinorUnits(long, Currency)"/>
    public static MonetaryValue FromMinorUnits(long minorUnits, string currencyCode) => FromMinorUnits(minorUnits, Currency.GetCurrency(currencyCode));

    /// <summary>
    /// Creates a value from an amount in minor units of the specified currency, i.e. <c>1234</c> cents for <c>USD 12.34</c>.
    /// </summary>
    /// <param name="minorUnits">The amount as a whole number of minor units.</param>
    /// <param name="currency">The currency of the value.</param>
    /// <remarks>
    /// A minor unit is <c>10</c> to the power of minus the currency's <see cref="Currency.DecimalDigits"/>: <c>0.01</c> for a currency with <c>2</c> decimal
    /// digits, <c>1</c> for a currency with none and <c>0.001</c> for a currency with <c>3</c>. A rounding increment in the currency's <see
    /// cref="Currency.RoundingPolicy"/> does not change the minor unit.
    /// </remarks>
    public static MonetaryValue FromMinorUnits(long minorUnits, Currency currency)
    {
        return new MonetaryValue(minorUnits / GetMinorUnitsPerUnit(currency), currency);
    }

    /// <summary>
    /// Gets the amount of this value as a whole number of minor units of its currency, i.e. <c>1234</c> for <c>USD 12.34</c>. The default value gives
    /// <c>0</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A minor unit is <c>10</c> to the power of minus the currency's <see cref="Currency.DecimalDigits"/>. A rounding increment in the currency's <see
    /// cref="Currency.RoundingPolicy"/> does not change the minor unit.</para>
    /// <para>
    /// Nothing is rounded: an amount that is not a whole number of minor units, such as <c>USD 12.345</c>, throws. Use <see
    /// cref="ToMinorUnits(MidpointRounding)"/> to round it first.</para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">The amount is not a whole number of minor units of the currency.</exception>
    /// <exception cref="OverflowException">The number of minor units is outside the range of <see cref="long"/>.</exception>
    public long ToMinorUnits()
    {
        if (_currency is null)
            return 0;

        decimal minorUnits = _amount * GetMinorUnitsPerUnit(_currency);

        if (minorUnits != decimal.Truncate(minorUnits))
        {
            throw new InvalidOperationException(
                $"The amount {_amount.ToString(CultureInfo.InvariantCulture)} {_currency.CurrencyCode} is not a whole number of minor units. Round it, for " +
                $"example with {nameof(ToMinorUnits)}({nameof(MidpointRounding)}).");
        }

        return decimal.ToInt64(minorUnits);
    }

    /// <summary>
    /// Gets the amount of this value as a whole number of minor units of its currency, after rounding the amount to the minor unit with the specified
    /// midpoint rounding mode. The default value gives <c>0</c>.
    /// </summary>
    /// <param name="mode">The midpoint rounding mode, i.e. <see cref="MidpointRounding.ToEven"/> for banker's rounding.</param>
    /// <remarks>
    /// The amount is rounded to the minor unit, which is <c>10</c> to the power of minus the currency's <see cref="Currency.DecimalDigits"/>, and not to a
    /// rounding increment in the currency's <see cref="Currency.RoundingPolicy"/>. Use <see cref="Round(MidpointRounding)"/> first to apply the policy.
    /// </remarks>
    /// <exception cref="OverflowException">The number of minor units is outside the range of <see cref="long"/>.</exception>
    public long ToMinorUnits(MidpointRounding mode)
    {
        if (_currency is null)
            return 0;

        decimal rounded = decimal.Round(_amount, _currency.DecimalDigits, mode);
        return decimal.ToInt64(rounded * GetMinorUnitsPerUnit(_currency));
    }

    private static decimal GetMinorUnitsPerUnit(Currency currency)
    {
        decimal factor = 1;

        for (int i = 0; i < currency.DecimalDigits; i++)
            factor *= 10;

        return factor;
    }
}
