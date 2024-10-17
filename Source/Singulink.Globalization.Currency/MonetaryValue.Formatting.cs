using System.Collections.Immutable;
using System.Diagnostics;

namespace Singulink.Globalization;

/// <content>
/// Contains the ToString implementation of the <see cref="MonetaryValue"/> struct.
/// </content>
partial struct MonetaryValue
#if NET
    : ISpanFormattable
#else
    : IFormattable
#endif
{
    /// <summary>
    /// Enough capacity for number (73) + symbol (20) + other pattern elements (3: spaces/sign/parenthesis).
    /// </summary>
    internal const int FormatBufferSize = 96; // Enough for all possible monetary values.

    private const string PositiveInternationalPattern = "$ n";
    private const string PositiveReverseInternationalPattern = "n $";
    private const string PositiveEmptySymbolCurrencyPattern = "n";
    private static readonly ImmutableArray<string> PositiveCurrencyPatterns = ["$n", "n$", "$ n", "n $"];

    private static readonly ImmutableArray<string> NegativeCurrencyPatterns
        = ["($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)", "$- n"];
    private static readonly ImmutableArray<string> NegativeEmptySymbolCurrencyPatterns
        = ["(n)", "-n", "-n", "n-", "(n)", "-n", "n-", "n-", "-n", "- n", "n -", "n-", "-n", "n-", "(n)", "(n)", "- n"];
    private static readonly ImmutableArray<string> NegativeInternationalPatterns
        = ["$ (n)", "$ -n", "$ -n", "$ n-", "$ (n)", "$ -n", "$ n-", "$ n-", "$ -n", "$ -n", "$ n-", "$ n-", "$ -n", "$ n-", "$ (n)", "$ (n)", "$ -n"];
    private static readonly ImmutableArray<string> NegativeReverseInternationalPatterns
        = ["(n) $", "-n $", "-n $", "n- $", "(n) $", "-n $", "n- $", "n- $", "-n $", "-n $", "n- $", "n- $", "-n $", "n- $", "(n) $", "(n) $", "-n $"];

    /// <summary>
    /// Returns a culture-dependent international string representation of this value's currency and amount.
    /// </summary>
    public override string ToString() => ToString((string)null, null);

    /// <summary>
    /// Returns a string representation of this value's currency and amount.
    /// </summary>
    /// <param name="format">The string format to use.</param>
    /// <param name="provider">The format provider that will be used to obtain number format and culture/region information. If a <see cref="CultureInfo"/>
    /// object is provided as the <paramref name="provider"/> parameter and the format specifies region-specific behavior then this must be a region-specific
    /// culture, or if it is not a <see cref="CultureInfo"/> object then <see cref="CultureInfo.CurrentCulture"/> is used as a fallback and must be
    /// region-specific.</param>
    /// <remarks>
    /// <para>String format is composed of 3 parts, any of which can be omitted as long as the included specifiers appear in the following order:</para>
    /// <para><b>1. Currency format specifier:</b></para>
    /// <list type="bullet">
    ///   <item><term>"G"</term><description> General: Use currency code before the amount for English, Irish, Latvian and Maltese language cultures; otherwise
    ///   currency code after the amount.</description></item>
    ///   <item><term>"I"</term><description> International: Use currency code before the amount (<c>"USD 1.23"</c>).</description></item>
    ///   <item><term>"R"</term><description> Reverse International / Round-trip: Use currency code after the amount (<c>"1.23 USD"</c>).</description></item>
    ///   <item><term>"L"</term><description> Local: If the currency is local to the culture's region use <c>"C"</c> (currency symbol) formatting, otherwise
    ///   use <c>"G"</c> (general) formatting. If the culture is not region-specific then general formatting is always used.</description></item>
    ///   <item><term>"C"</term><description>Currency symbol: Use currency symbol with placement dictated by the format provider
    ///   (<c>$1.23</c>).</description></item>
    /// </list>
    /// <para>The default currency format is <c>"G"</c> (general).</para>
    /// <para><b>2. Number format specifier:</b></para>
    /// <list type="bullet">
    ///   <item><term>"N"</term><description>Number: Use group separators</description></item>
    ///   <item><term>"D"</term><description>Digits: Do not use group separators</description></item>
    /// </list>
    /// <para>The default number format is <c>"N"</c> (number with group separators).</para>
    /// <para><b>3. Decimals format specifier:</b></para>
    /// <list type="bullet">
    ///   <item><term>"*"</term><description> Use no decimal places if possible, otherwise currency's decimal places if possible, otherwise as many decimals as
    ///   needed to represent the value.</description></item>
    ///   <item><term>"B"</term><description> Use the currency's decimal places (or a custom fixed number of decimal places following <c>"B"</c>) with banker's
    ///   "to even" rounding</description></item>
    ///   <item><term>"A"</term><description> Use the currency's decimal places (or a custom fixed number of decimal places following <c>"A"</c>) with "away
    ///   from zero" rounding</description></item>
    /// </list>
    /// <para>The default uses the currency's decimal places if possible, otherwise as many decimals as needed to represent the value. If a fixed number of
    /// decimal places is specified after <c>"B"</c> or <c>"A"</c>, it must be between 0 and 28.</para>
    /// </remarks>
    /// <exception cref="FormatException">Format specifier was invalid.</exception>
    public string ToString(string? format, IFormatProvider? provider = null) => ToString(format.AsSpan(), provider);

    internal string ToString(ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        Span<char> buffer = stackalloc char[FormatBufferSize];
        int charsWritten;

        while (!TryFormat(buffer, out charsWritten, format, provider))
        {
            // Debug.Fail("Buffer was not large enough to format value.");
            buffer = new char[buffer.Length * 2];
        }

        return buffer[..charsWritten].ToString();
    }

    /// <summary>
    /// Formats this value's currency and amount into a span of characters.
    /// </summary>
    /// <remarks>
    /// <para>A destination buffer length of 96 characters is recommended to ensure all possible <see cref="MonetaryValue"/> values can be formatted.</para>
    /// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
    /// </remarks>
    /// <param name="destination">The span of characters into which this instance will be written.</param>
    /// <param name="charsWritten">When the method returns, contains the length of the span in number of characters.</param>
    /// <param name="format">The string format to use.</param>
    /// <param name="provider">The format provider that will be used to obtain number format and culture/region information. If a <see
    /// cref="CultureInfo"/> object is used as the format provider and the format specifies region-specific behavior then this must be a region-specific
    /// culture, or if it is not a <see cref="CultureInfo"/> object then <see cref="CultureInfo.CurrentCulture"/> is used as a fallback and must be
    /// region-specific.</param>
    /// <returns><see langword="true"/> if the formatting operation succeeds; otherwise <see langword="false"/>.</returns>
    /// <exception cref="FormatException">Format specifier was invalid.</exception>
    /// <exception cref="ArgumentException">A region-specific culture is required for the format specified.</exception>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
#if NET
        Span<char> absAmountFormat = stackalloc char[3];
        (char symbolFormat, char decimalsFormat, int decimalsFormatNumber, absAmountFormat[0]) = ParseFormat(format);
#else
        string absAmountFormat;
        (char symbolFormat, char decimalsFormat, int decimalsFormatNumber, char resultNumberFormatType) = ParseFormat(format);
#endif

        if (IsDefault)
        {
            charsWritten = 0;
            return true;
        }

        var culture = GetCultureAndSetFormatProvider(ref provider);

        decimal absAmount = GetFormatAmountAndDecimalPlaces(Math.Abs(_amount), _currency, decimalsFormat, decimalsFormatNumber, out int formatDecimalPlaces);
        var (originalFormatInfo, alteredNumberFormatInfo) = provider.GetCurrencyNumberFormatInfos();

#if NET
        formatDecimalPlaces.TryFormat(absAmountFormat[1..], out int formatDecimalPlacesLength, null, CultureInfo.InvariantCulture);
        absAmountFormat = absAmountFormat[..(1 + formatDecimalPlacesLength)];
#else
        absAmountFormat = $"{resultNumberFormatType}{formatDecimalPlaces}";
#endif
        string currencySymbol;
        string pattern;
        NumberFormatInfo formatInfo;

        if (symbolFormat is 'L')
        {
            var region = culture.GetRegionInfo();
            symbolFormat = region?.ISOCurrencySymbol == _currency.CurrencyCode ? 'C' : 'G';
        }

        if (symbolFormat is 'C')
        {
            string symbol = _currency.GetLocalizedSymbol(culture);

            if (symbol == originalFormatInfo.CurrencyDecimalSeparator)
            {
                formatInfo = alteredNumberFormatInfo;
                currencySymbol = string.Empty;
                pattern = _amount >= 0 ? PositiveEmptySymbolCurrencyPattern : NegativeEmptySymbolCurrencyPatterns[originalFormatInfo.CurrencyNegativePattern];

                // Always force decimal places if the decimal separator is the currency symbol

                if (formatDecimalPlaces is 0)
                    formatDecimalPlaces = _currency.DecimalDigits > 0 ? _currency.DecimalDigits : 2;
            }
            else
            {
                // Formatting with an outside currency symbol so use original number format info if the altered decimal separator is a local currency symbol

                formatInfo = alteredNumberFormatInfo.CurrencyDecimalSeparator is "$" ? originalFormatInfo : alteredNumberFormatInfo;
                currencySymbol = symbol;

                pattern = _amount >= 0 ?
                    PositiveCurrencyPatterns[originalFormatInfo.CurrencyPositivePattern] :
                    NegativeCurrencyPatterns[originalFormatInfo.CurrencyNegativePattern];
            }
        }
        else
        {
            Debug.Assert(symbolFormat is 'I' or 'R' or 'G', "Unexpected symbol format.");

            // Formatting with an outside currency code so use original number format info if the altered decimal separator is a local currency symbol

            formatInfo = alteredNumberFormatInfo.CurrencyDecimalSeparator is "$" ? originalFormatInfo : alteredNumberFormatInfo;
            currencySymbol = _currency.CurrencyCode;

            bool useReverseFormat = symbolFormat switch {
                'I' => false,
                'R' => true,
                _ => culture.TwoLetterISOLanguageName is not "en" or "ga" or "lv" or "mt",
            };

            if (useReverseFormat)
                pattern = _amount >= 0 ? PositiveReverseInternationalPattern : NegativeReverseInternationalPatterns[originalFormatInfo.CurrencyNegativePattern];
            else
                pattern = _amount >= 0 ? PositiveInternationalPattern : NegativeInternationalPatterns[originalFormatInfo.CurrencyNegativePattern];
        }

        charsWritten = ApplyPattern(destination, pattern, absAmount, currencySymbol, originalFormatInfo.NegativeSign, absAmountFormat, formatInfo);
        return charsWritten > 0;
    }

    internal static CultureInfo GetCultureAndSetFormatProvider([NotNull] ref IFormatProvider? provider)
    {
        CultureInfo culture;

        if (provider is null)
            provider = culture = CultureInfo.CurrentCulture;
        else
            culture = provider as CultureInfo ?? CultureInfo.CurrentCulture;

        return culture;
    }

    private static int ApplyPattern(
        Span<char> destination,
        string pattern,
        decimal absAmount,
        string currencySymbol,
        string negativeSign,
        TargetDependentString absAmountFormat,
        NumberFormatInfo absAmountFormatInfo)
    {
        Span<char> remaining = destination;

        foreach (char c in pattern)
        {
            switch (c)
            {
                case 'n':
#if NET
                    if (!absAmount.TryFormat(remaining, out int charsWritten, absAmountFormat, absAmountFormatInfo))
                        return 0;

                    remaining = remaining[charsWritten..];
#else
                    string absNumberString = absAmount.ToString(absAmountFormat, absAmountFormatInfo);

                    if (!Append(ref remaining, absNumberString.AsSpan()))
                        return 0;
#endif

                    break;

                case '$':
                    if (!Append(ref remaining, currencySymbol.AsSpan()))
                        return 0;

                    break;

                case '-':
                    if (!Append(ref remaining, negativeSign.AsSpan()))
                        return 0;

                    break;

                case ' ':
                    const char NoBreakSpace = '\u00A0';

                    if (!AppendChar(ref remaining, NoBreakSpace))
                        return 0;

                    break;

                default:
                    if (!AppendChar(ref remaining, c))
                        return 0;

                    break;
            }
        }

        return destination.Length - remaining.Length; // Return the number of characters written

        static bool Append(ref Span<char> span, ReadOnlySpan<char> s)
        {
            if (!s.TryCopyTo(span))
                return false;

            span = span[s.Length..];
            return true;
        }

        static bool AppendChar(ref Span<char> span, char c)
        {
            if (span.Length == 0)
                return false;

            span[0] = c;
            span = span[1..];
            return true;
        }
    }

    private static decimal GetFormatAmountAndDecimalPlaces(decimal amount, Currency? currency, char decimalsFormat, int decimalsFormatNumber, out int formatDecimalPlaces)
    {
        if (decimalsFormat == default || decimalsFormat == '*')
        {
            int amountDecimalDigits = amount.GetDecimalDigits();
            formatDecimalPlaces = decimalsFormat == '*' && amountDecimalDigits is 0 ? 0 : Math.Max(amountDecimalDigits, currency?.DecimalDigits ?? 0);
        }
        else
        {
            Debug.Assert(decimalsFormat is 'B' or 'A', "Unexpected decimals format");
            formatDecimalPlaces = decimalsFormatNumber >= 0 ? decimalsFormatNumber : currency?.DecimalDigits ?? 0;

            var rounding = decimalsFormat is 'B' ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero;
            amount = decimal.Round(amount, formatDecimalPlaces, rounding);
        }

        return amount;
    }

    private static (char SymbolFormat, char DecimalsFormat, int DecimalsFormatNumber, char ResultNumberFormat) ParseFormat(ReadOnlySpan<char> format)
    {
        format = format.TrimEnd('\0');

        if (format.Length is 0)
            return ('G', default, -1, 'N');

        char current = char.ToUpperInvariant(format[0]);
        int currentIndex = 0;

        static char Next(ReadOnlySpan<char> format, ref int currentIndex) =>
            ++currentIndex >= format.Length ? default : char.ToUpperInvariant(format[currentIndex]);

        char symbolFormat;
        char decimalsFormat;
        int decimalsFormatNumber;
        char resultNumberFormat;

        // Currency format specifier

        if (current is 'G' or 'I' or 'R' or 'C' or 'L')
        {
            symbolFormat = current;
            current = Next(format, ref currentIndex);
        }
        else
        {
            symbolFormat = 'G';
        }

        // Number format specifier

        if (current is 'D')
        {
            resultNumberFormat = 'F';
            current = Next(format, ref currentIndex);
        }
        else
        {
            resultNumberFormat = 'N';

            if (current is 'N')
                current = Next(format, ref currentIndex);
        }

        // Decimals format specifier

        if (current is '*' or 'B' or 'A')
        {
            decimalsFormat = current;
            current = Next(format, ref currentIndex);

            if (current is not default(char) && decimalsFormat is not '*')
            {
                var decimalsFormatNumberSpan = format[currentIndex..];

                if (decimalsFormatNumberSpan.Length > 2 ||
#if NET
                    !int.TryParse(decimalsFormatNumberSpan, NumberStyles.None, CultureInfo.InvariantCulture, out decimalsFormatNumber) ||
#else
                    !int.TryParse(decimalsFormatNumberSpan.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out decimalsFormatNumber) ||
#endif
                    decimalsFormatNumber is < 0 or > 28)
                {
                    ThrowFormatEx();
                }

                // Processed format to the end, return early to skip the validation/throw at the end.
                goto Return;
            }
            else
            {
                decimalsFormatNumber = -1;
            }
        }
        else
        {
            decimalsFormat = default;
            decimalsFormatNumber = -1;
        }

        if (current is not default(char))
            ThrowFormatEx();

        Return:

        return (symbolFormat, decimalsFormat, decimalsFormatNumber, resultNumberFormat);

        [DoesNotReturn]
        static void ThrowFormatEx() => throw new FormatException("Format specifier was invalid.");
    }
}