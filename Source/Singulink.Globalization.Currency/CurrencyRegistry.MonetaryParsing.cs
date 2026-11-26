using System.Diagnostics;
using Singulink.Enums;

namespace Singulink.Globalization;

/// <content>
/// Contains the <see cref="MonetaryValue"/> parsing functionality for <see cref="CurrencyRegistry"/>.
/// </content>
partial class CurrencyRegistry
{
    /// <inheritdoc cref="TryParseMonetaryValue(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue, out string)"/>
    public bool TryParseMonetaryValue(ReadOnlySpan<char> s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result)
        => TryParseMonetaryValue(s, style, provider, out result, false, out _);

    /// <summary>
    /// Converts the string representation of a monetary value to its <see cref="MonetaryValue"/> equivalent using this currency registry.
    /// </summary>
    /// <param name="s">The string representation of the monetary value to convert.</param>
    /// <param name="style">A combination of <see cref="MonetaryStyles"/> values that indicate the styles that can be parsed.</param>
    /// <param name="provider">A format provider that supplies culture-specific parsing information.</param>
    /// <param name="result">The parsed monetary value if parsing was successful; otherwise a default monetary value.</param>
    /// <param name="error">An error message if parsing failed; otherwise an empty string.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A zero amount without a currency is read as the <see cref="MonetaryValue.Default"/> value, which has no currency. The round-trip format <c>"O"</c>
    /// writes the default value that way.
    /// </remarks>
    public bool TryParseMonetaryValue(ReadOnlySpan<char> s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result, out string error)
        => TryParseMonetaryValue(s, style, provider, out result, true, out error);

    /// <inheritdoc cref="TryParseMonetaryValue(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue, out string)"/>
    public bool TryParseMonetaryValue([NotNullWhen(true)] string? s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result)
        => TryParseMonetaryValue(s.AsSpan(), style, provider, out result, false, out _);

    /// <inheritdoc cref="TryParseMonetaryValue(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?, out MonetaryValue, out string)"/>
    public bool TryParseMonetaryValue([NotNullWhen(true)] string? s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result, out string error)
        => TryParseMonetaryValue(s.AsSpan(), style, provider, out result, true, out error);

    /// <summary>
    /// Converts the string representation of a monetary value to its <see cref="MonetaryValue"/> equivalent using this currency registry.
    /// </summary>
    /// <param name="s">The string representation of the monetary value to convert.</param>
    /// <param name="style">A combination of <see cref="MonetaryStyles"/> values that indicate the styles that can be parsed. Defaults to
    /// <see cref="MonetaryStyles.CurrencyCode"/>.</param>
    /// <param name="provider">A format provider that supplies culture-specific parsing information, or <see langword="null"/> to use the current
    /// culture.</param>
    /// <returns>The parsed monetary value.</returns>
    /// <exception cref="FormatException">The string is not in a valid format.</exception>
    /// <remarks>
    /// A zero amount without a currency is read as the <see cref="MonetaryValue.Default"/> value, which has no currency. The round-trip format <c>"O"</c>
    /// writes the default value that way.
    /// </remarks>
    public MonetaryValue ParseMonetaryValue(ReadOnlySpan<char> s, MonetaryStyles style = MonetaryStyles.CurrencyCode, IFormatProvider? provider = null)
    {
        if (!TryParseMonetaryValue(s, style, provider, out var result, true, out string error))
            throw new FormatException(error);

        return result;
    }

    /// <inheritdoc cref="ParseMonetaryValue(ReadOnlySpan{char}, MonetaryStyles, IFormatProvider?)"/>
    public MonetaryValue ParseMonetaryValue(string s, MonetaryStyles style = MonetaryStyles.CurrencyCode, IFormatProvider? provider = null)
        => ParseMonetaryValue(s.AsSpan(), style, provider);

    private bool TryParseMonetaryValue(ReadOnlySpan<char> s, MonetaryStyles style, IFormatProvider? provider, out MonetaryValue result, bool buildError, out string error)
    {
        const StringComparison Ordinal = StringComparison.Ordinal;

        if (!style.IsValid())
            throw new ArgumentException($"An undefined {nameof(MonetaryStyles)} value is being used.", nameof(style));

        bool allowLeadingWhite = style.HasAllFlags(MonetaryStyles.AllowLeadingWhite);
        bool allowLeadingSign = style.HasAllFlags(MonetaryStyles.AllowLeadingSign);
        bool allowTrailingWhite = style.HasAllFlags(MonetaryStyles.AllowTrailingWhite);
        bool allowTrailingSign = style.HasAllFlags(MonetaryStyles.AllowTrailingSign);
        bool allowParenthesis = style.HasAllFlags(MonetaryStyles.AllowParentheses);
        bool allowDecimalPoint = style.HasAllFlags(MonetaryStyles.AllowDecimalPoint);
        bool allowThousands = style.HasAllFlags(MonetaryStyles.AllowThousands);

        bool allowCurrencyCode = style.HasAllFlags(MonetaryStyles.AllowCurrencyCode);
        bool allowLocalSymbol = style.HasAllFlags(MonetaryStyles.AllowLocalSymbol);
        bool allowUnambiguousSymbols = style.HasAllFlags(MonetaryStyles.AllowUnambiguousSymbols);

        if (!allowCurrencyCode && !allowLocalSymbol && !allowUnambiguousSymbols)
            throw new ArgumentException($"The style must have at least one of the following flags set: {MonetaryStyles.AllowCurrencyCode}, {MonetaryStyles.AllowLocalSymbol} or {MonetaryStyles.AllowUnambiguousSymbols}.", nameof(style));

        if (allowCurrencyCode && _parseCurrencyCodesError is not null)
        {
            string message = "This currency registry cannot parse currency codes because one of the currency codes violates the following requirement: " +
                           _parseCurrencyCodesError;

            throw new InvalidOperationException(message);
        }

        var culture = MonetaryValue.GetCultureAndSetFormatProvider(ref provider);
        var (originalFormatInfo, alteredNumberFormatInfo) = provider.GetCurrencyNumberFormatInfos();

        // If decimal separator is local currency symbol then it will always be buried in the amount, so just look for the regular number decimal separator
        // if it leads/trails the amount
        string searchDecimalSeparator = alteredNumberFormatInfo.NumberDecimalSeparator is "$"
            ? originalFormatInfo.NumberDecimalSeparator
            : alteredNumberFormatInfo.NumberDecimalSeparator;

        s = s.TrimEnd('\0');
        var original = s; // Keep the original for error messages
        TrimIfAllowed(ref s);

        ReadOnlySpan<char> currencyIndicator = default;
        bool? currencyIndicatorIsSymbol = null;
        int sign = 0;

        if (TryParseParenthesisAndOutsideCurrencyCode(ref s, ref currencyIndicator) &&
            TryParseStart(ref s, ref currencyIndicator) &&
            TryParseEnd(ref s, ref currencyIndicator) &&
            TryParseAmount(s, ref currencyIndicator, out decimal amount))
        {
            if (sign < 0)
                amount = -amount; // Apply sign to the amount

            Currency currency = null;

            if (currencyIndicator.Length is 0)
            {
                // A value without a currency can only be the default value, which has a zero amount. The round-trip format writes it as "0".

                if (amount is 0)
                {
                    error = string.Empty;
                    result = default;
                    return true;
                }

                error = buildError ? $"The input string '{original.ToString()}' does not contain a currency code." : string.Empty;
                goto Error;
            }
            else
            {
                Debug.Assert(currencyIndicator.Trim().Length == currencyIndicator.Length, "Currency indicator should not contain leading/trailing whitespace.");

                var codeError = CurrencyCodeError.None;
                var localSymbolError = LocalCurrencySymbolError.None;
                var unambiguousSymbolError = UnambiguousCurrencySymbolError.None;

#if NET9_0_OR_GREATER
                var currencyIndicatorLookupValue = currencyIndicator;
#else
                string currencyIndicatorLookupValue = currencyIndicator.ToString();
#endif

                // A registry that cannot parse the culture's symbols throws whenever local symbols are allowed, even if the indicator is a currency code.

                if (allowLocalSymbol)
                    EnsureSymbolsParsable();

                // Currency Code: tried first because codes are unique in the registry and do not depend on the culture, so text written with a code always
                // reads back as the same currency.

                if (allowCurrencyCode && TryParseCurrencyCode(currencyIndicatorLookupValue, out currency, out codeError))
                    goto Success;

                // Local

                if (allowLocalSymbol && TryParseLocalSymbol(currencyIndicator, out currency, out localSymbolError))
                    goto Success;

                // Unambiguous Symbols

                if (allowUnambiguousSymbols && TryParseUnambiguousSymbol(currencyIndicatorLookupValue, out currency, out unambiguousSymbolError))
                    goto Success;

                if (!buildError)
                {
                    error = string.Empty;
                    goto Error;
                }

                error = $"The input string '{original.ToString()}' had a currency indicator '{currencyIndicatorLookupValue}' that could not be resolved. " +
                         "The following lookups were attempted but failed: ";

                List<string> errors = [];

                if (codeError is not CurrencyCodeError.None)
                    errors.Add($"Currency code: {codeError}");

                if (localSymbolError is not LocalCurrencySymbolError.None)
                    errors.Add($"Local currency symbol: {localSymbolError}");

                if (unambiguousSymbolError is not UnambiguousCurrencySymbolError.None)
                    errors.Add($"Unambiguous currency symbol: {unambiguousSymbolError}");

                error += string.Join(", ", errors);

                if (GetNoCurrencyDataHint() is string hint)
                    error += $". {hint}";

                goto Error;
            }

            Success:

            error = string.Empty;
            result = new MonetaryValue(amount, currency);
            return true;
        }
        else
        {
            error = buildError ? $"The input string '{original.ToString()}' was not in a correct format." : string.Empty;
        }

        Error:

        result = default;
        return false;

        bool TryParseParenthesisAndOutsideCurrencyCode(ref ReadOnlySpan<char> s, ref ReadOnlySpan<char> currencyIndicator)
        {
            if (!allowParenthesis || s.Length < 3)
                return true;

            if (s[0] is '(')
            {
                int closingIndex = s.LastIndexOf(')');

                if (closingIndex < 0)
                    return false;

                var after = s[(closingIndex + 1)..];
                s = s[1..closingIndex];

                if (after.Length is 0)
                {
                    currencyIndicatorIsSymbol = true;
                }
                else
                {
                    int afterLengthPreTrim = after.Length;
                    after = after.TrimStart();

                    if (after.Length is 0 || after.Length == afterLengthPreTrim)
                        return false;

                    currencyIndicator = after;
                    currencyIndicatorIsSymbol = false;
                }

                sign = -1;
                return true;
            }

            if (s[^1] is ')')
            {
                int openingIndex = s.IndexOf('(');

                if (openingIndex < 0)
                    return false;

                var before = s[..openingIndex];
                s = s[(openingIndex + 1)..^1];

                if (before.Length is 0)
                {
                    currencyIndicatorIsSymbol = true;
                }
                else
                {
                    int beforeLengthPreTrim = before.Length;
                    before = before.TrimEnd();

                    if (before.Length is 0 || before.Length == beforeLengthPreTrim)
                        return false;

                    currencyIndicator = before;
                    currencyIndicatorIsSymbol = false;
                }

                sign = -1;
                return true;
            }

            return true;
        }

        bool TryParseStart(ref ReadOnlySpan<char> s, ref ReadOnlySpan<char> currencyIndicator)
        {
            bool onlySymbolsAllowed = false;

            while (s.Length > 0 && s[0] is char c && !IsAsciiDigit(c) && !StartsWithDecimalSeparator(s))
            {
                if (char.IsWhiteSpace(c))
                {
                    return false;
                }
                else if (StartsWithSign(s, out int parsedSign, out int signLength))
                {
                    if (!allowLeadingSign || sign is not 0)
                        return false;

                    onlySymbolsAllowed = true;
                    sign = parsedSign;
                    s = s[signLength..];
                }
                else
                {
                    if (currencyIndicator.Length is not 0)
                        return false;

                    if (onlySymbolsAllowed)
                        currencyIndicatorIsSymbol = true;

                    var original = s;

                    do
                    {
                        s = s[1..];
                    }
                    while (s.Length > 0 && s[0] is char c2 &&
                           !IsAsciiDigit(c2) &&
                           (!char.IsWhiteSpace(c2) || c2 is '\u202F') &&
                           !StartsWithSign(s, out _, out _));

                    currencyIndicator = original[..(original.Length - s.Length)];
                }

                s = s.TrimStart();
            }

            return true;

            bool StartsWithDecimalSeparator(ReadOnlySpan<char> s) =>
                s.StartsWith(searchDecimalSeparator.AsSpan(), Ordinal) &&
                s.Length > searchDecimalSeparator.Length && IsAsciiDigit(s[searchDecimalSeparator.Length]);

            bool StartsWithSign(ReadOnlySpan<char> s, out int sign, out int signLength)
            {
                if (originalFormatInfo.PositiveSign.Length > 0 && s.StartsWith(originalFormatInfo.PositiveSign.AsSpan(), Ordinal))
                {
                    sign = 1;
                    signLength = originalFormatInfo.PositiveSign.Length;
                    return true;
                }
                else if (originalFormatInfo.NegativeSign.Length > 0 && s.StartsWith(originalFormatInfo.NegativeSign.AsSpan(), Ordinal))
                {
                    sign = -1;
                    signLength = originalFormatInfo.NegativeSign.Length;
                    return true;
                }

                sign = 0;
                signLength = 0;
                return false;
            }
        }

        bool TryParseEnd(ref ReadOnlySpan<char> s, ref ReadOnlySpan<char> currencyIndicator)
        {
            bool onlySymbolsAllowed = false;

            while (s.Length > 0 && s[^1] is char c && !IsAsciiDigit(c) && !EndsWithDecimalSeparator(s))
            {
                if (char.IsWhiteSpace(c))
                {
                    return false;
                }
                else if (EndsWithSign(s, out int parsedSign, out int signLength))
                {
                    if (!allowTrailingSign || sign is not 0)
                        return false;

                    onlySymbolsAllowed = true;
                    sign = parsedSign;
                    s = s[..^signLength];
                }
                else
                {
                    if (currencyIndicator.Length is not 0)
                        return false;

                    if (onlySymbolsAllowed)
                        currencyIndicatorIsSymbol = true;

                    var original = s;

                    do
                    {
                        s = s[..^1];
                    }
                    while (s.Length > 0 && s[^1] is char c2 &&
                           !IsAsciiDigit(c2) &&
                           (!char.IsWhiteSpace(c2) || c2 is '\u202F') &&
                           !EndsWithSign(s, out _, out _));

                    currencyIndicator = original[s.Length..];
                }

                s = s.TrimEnd();
            }

            return true;

            bool EndsWithDecimalSeparator(ReadOnlySpan<char> s) =>
                s.EndsWith(searchDecimalSeparator.AsSpan(), Ordinal) &&
                s.Length > searchDecimalSeparator.Length && IsAsciiDigit(s[s.Length - searchDecimalSeparator.Length - 1]);

            bool EndsWithSign(ReadOnlySpan<char> s, out int sign, out int signLength)
            {
                if (originalFormatInfo.PositiveSign.Length > 0 && s.EndsWith(originalFormatInfo.PositiveSign.AsSpan(), Ordinal))
                {
                    sign = 1;
                    signLength = originalFormatInfo.PositiveSign.Length;
                    return true;
                }

                if (originalFormatInfo.NegativeSign.Length > 0 && s.EndsWith(originalFormatInfo.NegativeSign.AsSpan(), Ordinal))
                {
                    sign = -1;
                    signLength = originalFormatInfo.NegativeSign.Length;
                    return true;
                }

                sign = 0;
                signLength = 0;
                return false;
            }
        }

        bool TryParseAmount(ReadOnlySpan<char> s, ref ReadOnlySpan<char> currencyIndicator, out decimal result)
        {
            if (s.Length == 0)
            {
                result = 0;
                return false;
            }

            Debug.Assert(s.Trim().Length == s.Length, "Amount should not contain leading/trailing whitespace.");

            // AllowLeadingWhite = 1 (discard, handled separately)
            // AllowTrailingWhite = 2 (discard, handled separately)
            // AllowLeadingSign = 4 (discard, handled separately)
            // AllowTrailingSign = 8 (discard, handled separately)
            // AllowParentheses = 16 (discard, handled separately)
            // AllowDecimalPoint = 32 (keep)
            // AllowThousands = 64 (keep)

            var absNumberStyle = (NumberStyles)((int)style & 0b1100000);

            NumberFormatInfo formatInfo;

            // The literal "$" is a deliberate whitelist: it is the one currency symbol that the runtime's cultures (kea-CV, pt-CV) use as a decimal
            // separator. Add another separator here only together with tests for it.
            if (alteredNumberFormatInfo.NumberDecimalSeparator is "$")
            {
                if (currencyIndicator.Length is 0)
                {
                    if (currencyIndicatorIsSymbol is not false && s.IndexOf('$') >= 0)
                    {
                        currencyIndicator = "$".AsSpan();
                        currencyIndicatorIsSymbol = true;
                        formatInfo = alteredNumberFormatInfo;
                    }
                    else
                    {
                        // No currency at all: read the number as it is read next to a currency code, so that a zero parses as the default value and any
                        // other amount fails for its missing currency, as in every other culture.
                        formatInfo = originalFormatInfo;
                    }
                }
                else
                {
                    formatInfo = originalFormatInfo;
                }
            }
            else
            {
                formatInfo = alteredNumberFormatInfo;
            }

#if NET
            return decimal.TryParse(s, absNumberStyle, formatInfo, out result);
#else
            return decimal.TryParse(s.ToString(), absNumberStyle, formatInfo, out result);
#endif
        }

        void EnsureSymbolsParsable()
        {
            var (_, parseError) = GetOrBuildCurrenciesBySymbolLookup(culture);

            if (parseError is not null)
            {
                string message = $"This currency registry cannot parse currency symbols for culture '{culture.Name}' because one of the symbols violates the " +
                    "following requirement: " + parseError;

                throw new InvalidOperationException(message);
            }
        }

        bool TryParseLocalSymbol(ReadOnlySpan<char> currencyIndicator, [MaybeNullWhen(false)] out Currency currency, out LocalCurrencySymbolError error)
        {
            // The caller has already checked that the culture's symbols can be parsed.

            if (currencyIndicatorIsSymbol is false)
            {
                error = LocalCurrencySymbolError.IndicatorIsNotSymbol;
                goto SymbolError;
            }

            var region = culture.GetRegionInfo();

            if (region is null)
            {
                error = LocalCurrencySymbolError.NoRegionInfo;
                goto SymbolError;
            }
            else if (!TryGetLocalCurrency(region, out currency))
            {
                error = LocalCurrencySymbolError.LocalCurrencyNotFound;
                goto SymbolError;
            }
            else if (!currencyIndicator.Equals(currency.GetLocalizedSymbol(culture).AsSpan(), StringComparison.Ordinal))
            {
                error = LocalCurrencySymbolError.SymbolDoesNotMatch;
                goto SymbolError;
            }

            error = LocalCurrencySymbolError.None;
            return true;

            SymbolError:

            currency = null;
            return false;
        }

        bool TryParseCurrencyCode(TargetDependentStringKey currencyIndicator, [MaybeNullWhen(false)] out Currency currency, out CurrencyCodeError error)
        {
            if (currencyIndicatorIsSymbol is true)
            {
                error = CurrencyCodeError.IndicatorIsNotCode;
                goto CodeError;
            }

            if (!TryGetCurrency(currencyIndicator, out currency))
            {
                error = CurrencyCodeError.CodeNotFound;
                goto CodeError;
            }

            error = CurrencyCodeError.None;
            return true;

            CodeError:

            currency = null;
            return false;
        }

        bool TryParseUnambiguousSymbol(TargetDependentStringKey currencyIndicator, [MaybeNullWhen(false)] out Currency currency, out UnambiguousCurrencySymbolError error)
        {
            var (lookup, parseError) = GetOrBuildCurrenciesBySymbolLookup(culture);

            if (parseError is not null)
            {
                string message = $"This currency registry cannot parse currency symbols for culture '{culture.Name}' because one of the symbols violates the " +
                    "following requirement: " + parseError;

                throw new InvalidOperationException(message);
            }

            if (currencyIndicatorIsSymbol is false)
            {
                error = UnambiguousCurrencySymbolError.IndicatorIsNotSymbol;
                goto SymbolError;
            }

            if (!lookup.TargetDependent.TryGetValues(currencyIndicator, out var currencies))
            {
                error = UnambiguousCurrencySymbolError.CodeNotFound;
                goto SymbolError;
            }

            if (currencies.Count is not 1)
            {
                error = UnambiguousCurrencySymbolError.MultipleMatchesFound;
                goto SymbolError;
            }

            currency = currencies[0];
            error = UnambiguousCurrencySymbolError.None;
            return true;

            SymbolError:

            currency = null;
            return false;
        }

        void TrimIfAllowed(ref ReadOnlySpan<char> s)
        {
            if (allowLeadingWhite)
                s = s.TrimStart();

            if (allowTrailingWhite)
                s = s.TrimEnd();
        }

        static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';
    }

    private enum LocalCurrencySymbolError
    {
        None,
        IndicatorIsNotSymbol,
        NoRegionInfo,
        LocalCurrencyNotFound,
        SymbolDoesNotMatch,
    }

    private enum CurrencyCodeError
    {
        None,
        IndicatorIsNotCode,
        CodeNotFound,
    }

    private enum UnambiguousCurrencySymbolError
    {
        None,
        IndicatorIsNotSymbol,
        CodeNotFound,
        MultipleMatchesFound,
    }
}