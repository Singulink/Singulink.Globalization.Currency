namespace Singulink.Globalization;

/// <content>
/// Contains Parse and TryParse implementations for <see cref="MonetaryValue"/>.
/// </content>
partial struct MonetaryValue
#if NET
    : IParsable<MonetaryValue>, ISpanParsable<MonetaryValue>
#endif
{
    /// <inheritdoc cref="Parse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?)"/>
    public static MonetaryValue Parse(string s, IFormatProvider? provider) => Parse(s.AsSpan(), provider);

    /// <inheritdoc cref="Parse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?)"/>
    public static MonetaryValue Parse(string s, MonetaryStyles style = MonetaryStyles.CurrencyCode, IFormatProvider? provider = null) => Parse(s.AsSpan(), style, provider);

    /// <inheritdoc cref="Parse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?)"/>
    public static MonetaryValue Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, MonetaryStyles.CurrencyCode, provider);

    /// <summary>
    /// Converts the string representation of a monetary value to its <see cref="MonetaryValue"/> equivalent using the <see cref="CurrencyRegistry.Default"/> currency
    /// registry.
    /// </summary>
    /// <param name="s">The string representation of the monetary value to convert.</param>
    /// <param name="style">A combination of <see cref="MonetaryStyles"/> values that indicate the styles that can be parsed.</param>
    /// <param name="provider">A format provider that supplies culture-specific parsing information.</param>
    public static MonetaryValue Parse(ReadOnlySpan<char> s, MonetaryStyles style = MonetaryStyles.CurrencyCode, IFormatProvider? provider = null)
    {
        if (!CurrencyRegistry.Default.TryParseMoney(s, style, provider, out var result, out string message))
            throw new FormatException(message);

        return result;
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue)"/>
    public static bool TryParse([NotNullWhen(true)] string? s, out MonetaryValue result) => TryParse(s.AsSpan(), out result);

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue)"/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out MonetaryValue result) => TryParse(s.AsSpan(), provider, out result);

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue)"/>
    public static bool TryParse([NotNullWhen(true)] string? s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result) => TryParse(s.AsSpan(), style, provider, out result);

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue)"/>
    public static bool TryParse(ReadOnlySpan<char> s, out MonetaryValue result) => TryParse(s, MonetaryStyles.CurrencyCode, null, out result);

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue)"/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out MonetaryValue result) => TryParse(s, MonetaryStyles.CurrencyCode, provider, out result);

    /// <summary>
    /// Converts the string representation of a monetary value to its <see cref="MonetaryValue"/> equivalent using the <see cref="CurrencyRegistry.Default"/> currency
    /// registry.
    /// </summary>
    /// <param name="s">The string representation of the monetary value to convert.</param>
    /// <param name="style">A combination of <see cref="MonetaryStyles"/> values that indicate the styles that can be parsed.</param>
    /// <param name="provider">A format provider that supplies culture-specific parsing information.</param>
    /// <param name="result">The parsed monetary value if parsing was successful; otherwise a default monetary value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result)
    {
        return CurrencyRegistry.Default.TryParseMoney(s, style, provider, out result);
    }
}