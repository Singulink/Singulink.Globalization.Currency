using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Singulink.Globalization;

/// <content>
/// Contains the ToString implementation of the <see cref="MonetaryValue"/> struct.
/// </content>
partial struct MonetaryValue
#if NET
    : ISpanFormattable, IUtf8SpanFormattable
#else
    : IFormattable
#endif
{
    /// <summary>
    /// Enough capacity for number (73) + symbol (20) + other pattern elements (3: spaces/sign/parenthesis).
    /// </summary>
    internal const int FormatBufferSize = 96; // Enough for all possible monetary values.

    /// <summary>
    /// The currency format specifier of the round-trip format, which ignores the format provider.
    /// </summary>
    private const char RoundTripFormat = 'O';

    private const string PositiveInternationalPattern = "$ n";
    private const string PositiveReverseInternationalPattern = "n $";
    private const string PositiveEmptySymbolCurrencyPattern = "n";
    private static readonly ImmutableArray<string> PositiveCurrencyPatterns = ["$n", "n$", "$ n", "n $"];

    private static readonly ImmutableArray<string> NegativeCurrencyPatterns
        = ["($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)", "$- n"];
    private static readonly ImmutableArray<string> NegativeEmptySymbolCurrencyPatterns
        = ["(n)", "-n", "-n", "n-", "(n)", "-n", "n-", "n-", "-n", "-n", "n-", "n-", "-n", "n-", "(n)", "(n)", "-n"];
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
    ///   <item><term>"G"</term><description> General: Use currency code before the amount for English, Irish and Maltese language cultures and the invariant culture; otherwise
    ///   currency code after the amount.</description></item>
    ///   <item><term>"I"</term><description> International: Use currency code before the amount (<c>"USD 1.23"</c>).</description></item>
    ///   <item><term>"R"</term><description> Reverse International: Use currency code after the amount (<c>"1.23 USD"</c>).</description></item>
    ///   <item><term>"L"</term><description> Local: If the currency is local to the culture's region use <c>"C"</c> (currency symbol) formatting, otherwise
    ///   use <c>"G"</c> (general) formatting. If the culture is not region-specific then general formatting is always used.</description></item>
    ///   <item><term>"C"</term><description>Currency symbol: Use currency symbol with placement dictated by the format provider
    ///   (<c>$1.23</c>).</description></item>
    ///   <item><term>"V"</term><description> Value only: Use no currency code and no symbol (<c>"1.23"</c>). The amount is written as the format provider
    ///   writes amounts of money: with its currency separators, the sign or parentheses of its currency pattern and the currency's decimal
    ///   places.</description></item>
    ///   <item><term>"O"</term><description> Round-trip: Use the currency code, a space and the amount in the invariant culture without group separators,
    ///   with the currency's decimal digits or more when the amount needs them (<c>"USD 12.30"</c>, <c>"USD -0.50"</c>, <c>"JPY 1250"</c>). The format
    ///   provider is ignored, and the default value is written as <c>"0"</c>. The text reads back as an equal value with the <see
    ///   cref="CultureInfo.InvariantCulture"/> provider, and equal values write identical text. This specifier cannot be combined with a number or decimals
    ///   specifier.</description></item>
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
    /// <para><b>The default value:</b></para>
    /// <para>The <see cref="Default"/> value has no currency, so it is written as a zero with only the decimals specifier applied: <c>"0"</c>, or with a
    /// fixed number of decimal places the zero with that many decimals and the provider's decimal separator (<c>"0.00"</c> for <c>"B2"</c> in en-US,
    /// <c>"0,00"</c> in de-DE). <c>"B"</c> and <c>"A"</c> without a number use the currency's decimal places, which the default value does not have, so
    /// they write <c>"0"</c>. The text parses back as the default value with the same provider.</para>
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

#if NET

    /// <summary>
    /// Formats this value's currency and amount into a span of UTF-8 bytes. The bytes are the UTF-8 encoding of the text that the <see langword="char"/>
    /// overload writes, for every format and culture.
    /// </summary>
    /// <param name="utf8Destination">The span of bytes into which this instance will be written.</param>
    /// <param name="bytesWritten">When the method returns, contains the number of bytes written, or <c>0</c> if the destination was too small.</param>
    /// <param name="format">The string format to use.</param>
    /// <param name="provider">The format provider that will be used to obtain number format and culture/region information.</param>
    /// <returns><see langword="true"/> if the formatting operation succeeds; otherwise <see langword="false"/>, with nothing written.</returns>
    /// <remarks>
    /// <inheritdoc cref="ToString(string?, IFormatProvider?)"/>
    /// </remarks>
    /// <exception cref="FormatException">Format specifier was invalid.</exception>
    /// <exception cref="ArgumentException">A region-specific culture is required for the format specified.</exception>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        Span<char> buffer = stackalloc char[FormatBufferSize];

        // Only a currency with an unusually long symbol or code needs more than the buffer, so that case allocates.
        ReadOnlySpan<char> text = TryFormat(buffer, out int charsWritten, format, provider) ? buffer[..charsWritten] : ToString(format, provider).AsSpan();

        // Counting first means that a destination that is too small is left untouched, so no partial character is ever written.
        if (Encoding.UTF8.GetByteCount(text) > utf8Destination.Length)
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = Encoding.UTF8.GetBytes(text, utf8Destination);
        return true;
    }

#endif

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

        if (symbolFormat is RoundTripFormat)
            return TryFormatRoundTrip(destination, out charsWritten);

        var culture = GetCultureAndSetFormatProvider(ref provider);

        decimal absAmount = GetFormatAmountAndDecimalPlaces(Math.Abs(_amount), _currency, decimalsFormat, decimalsFormatNumber, out int formatDecimalPlaces);
        var (originalFormatInfo, alteredNumberFormatInfo) = provider.GetCurrencyNumberFormatInfos();

        string currencySymbol;
        string pattern;
        NumberFormatInfo formatInfo;

        if (symbolFormat is 'L' && _currency is not null)
        {
            var region = culture.GetRegionInfo();
            symbolFormat = region?.ISOCurrencySymbol == _currency.CurrencyCode ? 'C' : 'G';
        }

        if (_currency is null)
        {
            // The default value has no currency, so it is written as a zero with only the decimals part of the format applied, which parses back as the
            // default value with the same provider.

            // The literal "$" here and below is a deliberate whitelist: it is the one currency symbol that the runtime's cultures (kea-CV, pt-CV) use
            // as a decimal separator. Add another separator only together with tests for it, as in parsing.
            formatInfo = alteredNumberFormatInfo.CurrencyDecimalSeparator is "$" ? originalFormatInfo : alteredNumberFormatInfo;
            currencySymbol = string.Empty;
            pattern = "n";
        }
        else if (symbolFormat is 'V')
        {
            // The amount alone, as the culture writes amounts of money, with no code and no symbol. A culture whose currency decimal separator is "$"
            // writes its number separators here, as it does next to a currency code, since that "$" is the sign of its own currency.

            formatInfo = alteredNumberFormatInfo.CurrencyDecimalSeparator is "$" ? originalFormatInfo : alteredNumberFormatInfo;
            currencySymbol = string.Empty;
            pattern = _amount >= 0 ? PositiveEmptySymbolCurrencyPattern : NegativeEmptySymbolCurrencyPatterns[originalFormatInfo.CurrencyNegativePattern];
        }
        else if (symbolFormat is 'C')
        {
            string symbol = _currency.GetLocalizedSymbol(culture);

            // A symbol that is the culture's decimal separator is written in place of the separator only for the culture's own currency, as in "1 234$56"
            // for the Cape Verdean escudo: CLDR defines that separator for the currency and not for the locale. Any other currency with the same symbol
            // is written like every other foreign currency, with the symbol where the culture's pattern puts it. The separator has to be the
            // culture's own as well: a NumberFormatInfo of such a culture that is passed while another culture is current says nothing of that
            // culture's currency.

            if (symbol == originalFormatInfo.CurrencyDecimalSeparator &&
                symbol == culture.NumberFormat.CurrencyDecimalSeparator &&
                culture.GetRegionInfo()?.ISOCurrencySymbol == _currency.CurrencyCode)
            {
                formatInfo = alteredNumberFormatInfo;
                currencySymbol = string.Empty;
                pattern = _amount >= 0 ? PositiveEmptySymbolCurrencyPattern : NegativeEmptySymbolCurrencyPatterns[originalFormatInfo.CurrencyNegativePattern];

                // The decimal separator is the only sign of the currency here, so the decimals are always written. The amount was already rounded to the
                // decimals of the format, so a format with no decimals writes zeros.

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

            // The general format follows the rule of the EU's Interinstitutional Style Guide (7.3.3): the code goes before the amount in English, Irish
            // and Maltese and after it in every other language. Latvian wrote the code first until the end of 2017. The invariant culture ("iv") is based
            // on English, so it writes the code first as well.

            bool useReverseFormat = symbolFormat switch {
                'I' => false,
                'R' => true,
                _ => culture.TwoLetterISOLanguageName is not ("en" or "ga" or "mt" or "iv"),
            };

            if (useReverseFormat)
                pattern = _amount >= 0 ? PositiveReverseInternationalPattern : NegativeReverseInternationalPatterns[originalFormatInfo.CurrencyNegativePattern];
            else
                pattern = _amount >= 0 ? PositiveInternationalPattern : NegativeInternationalPatterns[originalFormatInfo.CurrencyNegativePattern];
        }

        // Currency spacing applies only to a symbol placed into the culture's currency pattern. The patterns for currency codes always have a space between
        // the code and the number.

        bool applyCurrencySpacing = symbolFormat is 'C' && _currency is not null;

#if NET
        formatDecimalPlaces.TryFormat(absAmountFormat[1..], out int formatDecimalPlacesLength, null, CultureInfo.InvariantCulture);
        absAmountFormat = absAmountFormat[..(1 + formatDecimalPlacesLength)];
#else
        absAmountFormat = $"{resultNumberFormatType}{formatDecimalPlaces}";
#endif

        charsWritten = ApplyPattern(destination, pattern, absAmount, currencySymbol, applyCurrencySpacing, originalFormatInfo.NegativeSign, absAmountFormat, formatInfo);
        return charsWritten > 0;
    }

    /// <summary>
    /// Writes the round-trip form: the currency code, a space and the canonical amount (see <see cref="TryFormatCanonicalAmount(Span{char}, out int)"/>).
    /// The default value, which has no currency, is written as <c>"0"</c>, which parsing reads as the default value.
    /// </summary>
    internal bool TryFormatRoundTrip(Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;

        if (_currency is null)
        {
            if (destination.Length is 0)
                return false;

            destination[0] = '0';
            charsWritten = 1;
            return true;
        }

        string code = _currency.CurrencyCode;

        if (destination.Length <= code.Length)
            return false;

        code.AsSpan().CopyTo(destination);
        destination[code.Length] = ' ';

        if (!TryFormatCanonicalAmount(destination[(code.Length + 1)..], out int amountLength))
            return false;

        charsWritten = code.Length + 1 + amountLength;
        return true;
    }

    /// <summary>
    /// Writes the amount of a value that has a currency in its canonical text: the number of decimals that the default format uses (the currency's decimal
    /// digits, or more when the amount needs them), the invariant culture, no group separators and a leading <c>"-"</c> for a negative amount. Equal values
    /// give identical text. The round-trip format, the JSON number, the JSON string amount and the JSON property name all use this text.
    /// </summary>
    internal string ToRoundTripString()
    {
        Span<char> buffer = stackalloc char[FormatBufferSize];

        if (TryFormatRoundTrip(buffer, out int charsWritten))
            return buffer[..charsWritten].ToString();

        // Only a custom currency with a very long code gets here.
        char[] largeBuffer = new char[_currency!.CurrencyCode.Length + FormatBufferSize];
        TryFormatRoundTrip(largeBuffer, out charsWritten);
        return new string(largeBuffer, 0, charsWritten);
    }

    internal bool TryFormatCanonicalAmount(Span<char> destination, out int charsWritten)
    {
        Debug.Assert(_currency is not null, "Default values have no canonical amount.");

        GetFormatAmountAndDecimalPlaces(_amount, _currency, default, -1, out int decimalPlaces);

#if NET
        Span<char> format = stackalloc char[3];
        format[0] = 'F';
        decimalPlaces.TryFormat(format[1..], out int decimalPlacesLength, default, CultureInfo.InvariantCulture);

        return _amount.TryFormat(destination, out charsWritten, format[..(1 + decimalPlacesLength)], CultureInfo.InvariantCulture);
#else
        string amountString = _amount.ToString("F" + decimalPlaces.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        if (!amountString.AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = amountString.Length;
        return true;
#endif
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
        bool applyCurrencySpacing,
        string negativeSign,
        TargetDependentString absAmountFormat,
        NumberFormatInfo absAmountFormatInfo)
    {
        const char NoBreakSpace = ' ';

        Span<char> remaining = destination;

        // Currency spacing (UTS #35, root currencySpacing): a no-break space goes between the symbol and the number where the pattern puts them directly
        // against each other and the symbol's character next to the number is neither a symbol character nor a separator. The number's character next to
        // the symbol is always a digit, so the rule's [:digit:] match on that side always holds.

        bool spaceAfterSymbol = applyCurrencySpacing && pattern.Contains("$n") && CurrencySpacingApplies(currencySymbol, atEnd: true);
        bool spaceBeforeSymbol = applyCurrencySpacing && pattern.Contains("n$") && CurrencySpacingApplies(currencySymbol, atEnd: false);

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
                    if (spaceBeforeSymbol && !AppendChar(ref remaining, NoBreakSpace))
                        return 0;

                    if (!Append(ref remaining, currencySymbol.AsSpan()))
                        return 0;

                    if (spaceAfterSymbol && !AppendChar(ref remaining, NoBreakSpace))
                        return 0;

                    break;

                case '-':
                    if (!Append(ref remaining, negativeSign.AsSpan()))
                        return 0;

                    break;

                case ' ':
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

    /// <summary>
    /// Determines whether the symbol's character at the specified edge is in the currency spacing rule's <c>[[:^S:]&amp;[:^Z:]]</c> set, i.e. it is neither a
    /// symbol character nor a separator.
    /// </summary>
    private static bool CurrencySpacingApplies(string symbol, bool atEnd)
    {
        if (symbol.Length is 0)
            return false;

        int index = 0;

        if (atEnd)
        {
            index = symbol.Length - 1;

            if (index > 0 && char.IsLowSurrogate(symbol[index]) && char.IsHighSurrogate(symbol[index - 1]))
                index--;
        }

        return CharUnicodeInfo.GetUnicodeCategory(symbol, index) is not (
            UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol or
            UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator);
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

        if (current is RoundTripFormat)
        {
            // The round-trip format does not combine with the number or decimals specifiers.

            if (format.Length is not 1)
                ThrowFormatEx();

            return (RoundTripFormat, default, -1, 'N');
        }

        if (current is 'G' or 'I' or 'R' or 'C' or 'L' or 'V')
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