namespace Singulink.Globalization;

/// <summary>
/// Determines the styles permitted when parsing a monetary string to a <see cref="MonetaryValue"/> value.
/// </summary>
/// <remarks>
/// <para>
/// Note that the order of precedence for parsing styles is as follows: <see cref="AllowCurrencyCode"/>, then <see cref="AllowLocalSymbol"/>, and lastly <see
/// cref="AllowUnambiguousSymbols"/>.</para>
/// <para>
/// This means that if all three flags are specified, the parser will first try to match what looks like the currency code or currency symbol to a currency code
/// in the registry, then to the local currency symbol, and then to any other unambiguous non-local currency symbols in the registry.</para>
/// </remarks>
[Flags]
public enum MonetaryStyles
{
    /// <inheritdoc cref="NumberStyles.AllowLeadingWhite"/>
    AllowLeadingWhite = 1,

    /// <inheritdoc cref="NumberStyles.AllowTrailingWhite"/>
    AllowTrailingWhite = 2,

    /// <inheritdoc cref="NumberStyles.AllowLeadingSign"/>
    AllowLeadingSign = 4,

    /// <inheritdoc cref="NumberStyles.AllowTrailingSign"/>
    AllowTrailingSign = 8,

    /// <inheritdoc cref="NumberStyles.AllowParentheses"/>
    AllowParentheses = 16,

    /// <inheritdoc cref="NumberStyles.AllowDecimalPoint"/>
    AllowDecimalPoint = 32,

    /// <inheritdoc cref="NumberStyles.AllowThousands"/>
    AllowThousands = 64,

    /// <summary>
    /// Indicates that the monetary string can use a currency code to indicate the currency.
    /// </summary>
    AllowCurrencyCode = 1_048_576,

    /// <summary>
    /// Indicates that the monetary string can use the local currency symbol of the parsing culture's region to indicate that it is in the local currency.
    /// </summary>
    AllowLocalSymbol = 2_097_152,

    /// <summary>
    /// Indicates that the monetary string can use any currency symbol in the registry to indicate the currency as long as the symbol does not match with
    /// multiple currencies.
    /// </summary>
    AllowUnambiguousSymbols = 4_194_304,

    /// <summary>
    /// Indicates that the <see cref="AllowCurrencyCode"/>, <see cref="AllowLeadingWhite"/>, <see cref="AllowTrailingWhite"/>, <see cref="AllowLeadingSign"/>,
    /// <see cref="AllowTrailingSign"/>, <see cref="AllowParentheses"/>, <see cref="AllowDecimalPoint"/> and <see cref="AllowThousands"/> styles are used. This
    /// is a composite monetary format style.
    /// </summary>
    CurrencyCode = AllowCurrencyCode | AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | AllowTrailingSign | AllowParentheses | AllowDecimalPoint | AllowThousands,

    /// <summary>
    /// Indicates that the <see cref="AllowLocalSymbol"/>, <see cref="AllowLeadingWhite"/>, <see cref="AllowTrailingWhite"/>, <see cref="AllowLeadingSign"/>,
    /// <see cref="AllowTrailingSign"/>, <see cref="AllowParentheses"/>, <see cref="AllowDecimalPoint"/> and <see cref="AllowThousands"/> styles are used. This
    /// is a composite monetary format style.
    /// </summary>
    LocalSymbol = AllowLocalSymbol | AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | AllowTrailingSign | AllowParentheses | AllowDecimalPoint | AllowThousands,

    /// <summary>
    /// Indicates that the <see cref="AllowUnambiguousSymbols"/>, <see cref="AllowLeadingWhite"/>, <see cref="AllowTrailingWhite"/>, <see cref="AllowLeadingSign"/>,
    /// <see cref="AllowTrailingSign"/>, <see cref="AllowParentheses"/>, <see cref="AllowDecimalPoint"/> and <see cref="AllowThousands"/> styles are used. This
    /// is a composite monetary format style.
    /// </summary>
    UnambiguousSymbols = AllowUnambiguousSymbols | AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | AllowTrailingSign | AllowParentheses | AllowDecimalPoint | AllowThousands,

    /// <summary>
    /// Indicates that the <see cref="AllowCurrencyCode"/>, <see cref="AllowLocalSymbol"/>, <see cref="AllowLeadingWhite"/>, <see cref="AllowTrailingWhite"/>,
    /// <see cref="AllowLeadingSign"/>, <see cref="AllowTrailingSign"/>, <see cref="AllowParentheses"/>, <see cref="AllowDecimalPoint"/> and <see
    /// cref="AllowThousands"/> styles are used. This is a composite monetary format style.
    /// </summary>
    CurrencyCodeOrLocalSymbol = CurrencyCode | AllowLocalSymbol,

    /// <summary>
    /// Indicates that the <see cref="AllowCurrencyCode"/>, <see cref="AllowUnambiguousSymbols"/>, <see cref="AllowLeadingWhite"/>, <see
    /// cref="AllowTrailingWhite"/>, <see cref="AllowLeadingSign"/>, <see cref="AllowTrailingSign"/>, <see cref="AllowParentheses"/>, <see
    /// cref="AllowDecimalPoint"/> and <see cref="AllowThousands"/> styles are used. This is a composite monetary format style.
    /// </summary>
    CurrencyCodeOrUnambiguousSymbols = CurrencyCode | AllowUnambiguousSymbols,

    /// <summary>
    /// Indicates that all styles are used. This is a composite monetary format style.
    /// </summary>
    Any = CurrencyCode | AllowLocalSymbol | AllowUnambiguousSymbols,
}
