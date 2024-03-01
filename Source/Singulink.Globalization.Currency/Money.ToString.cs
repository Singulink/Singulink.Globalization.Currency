using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <content>
/// Contains the ToString implementation of the <see cref="Money"/> struct.
/// </content>
partial struct Money : IFormattable
{
    private const string PositiveInternationalPattern = "$ n";
    private const string PositiveReverseInternationalPattern = "n $";
    private static readonly ImmutableArray<string> PositiveCurrencyPatterns = ImmutableArray.Create("$n", "n$", "$ n", "n $");

    private static readonly ImmutableArray<string> NegativeInternationalPatterns = ImmutableArray.Create("$ (n)", "$ -n", "$ -n", "$ n-", "$ (n)", "$ -n", "$ n-", "$ n-", "$ -n", "$ -n", "$ n-", "$ n-", "$ -n", "$ n-", "$ (n)", "$ (n)");
    private static readonly ImmutableArray<string> NegativeReverseInternationalPatterns = ImmutableArray.Create("(n) $", "-n $", "-n $", "n- $", "(n) $", "-n $", "n- $", "n- $", "-n $", "-n $", "n- $", "n- $", "-n $", "n- $", "(n) $", "(n) $");
    private static readonly ImmutableArray<string> NegativeCurrencyPatterns = ImmutableArray.Create("($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)");

    private static readonly ConditionalWeakTable<CultureInfo, RegionInfo?> s_regionInfoLookup = new();
    private static readonly ConditionalWeakTable<NumberFormatInfo, NumberFormatInfo> s_absNumberFormatInfoLookup = new();

    /// <summary>
    /// Returns a string representation of this value's currency and amount.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string representation of this value's currency and amount.
    /// </summary>
    /// <param name="format">The string format to use.</param>
    /// <param name="formatProvider">The format provider that will be used to obtain number format information. This should be a region-specific <see
    /// cref="CultureInfo"/> instance for formats that depend on the culture, otherwise the current culture/region is used.</param>
    /// <remarks>
    /// <para>String format is composed of 3 parts, in the following order:</para>
    /// <para>Currency format specifier (required):</para>
    /// <list type="bullet">
    ///   <item><term>"C"</term><description>Culture-Dependent International: Currency code before the amount for English, Irish, Latvian and Maltese
    ///   language cultures, otherwise currency code after the amount.</description></item>
    ///   <item><term>"I"</term><description>International: Use currency code before the amount (<c>"USD 1.23"</c>).</description></item>
    ///   <item><term>"R"</term><description>Reverse International: Use currency code after the amount (<c>"1.23 USD"</c>).</description></item>
    ///   <item><term>"S"</term><description>Symbol: Use currency symbol with placement dictated by the format provider (<c>$1.23</c>).</description></item>
    ///   <item><term>"L"</term><description>Local: If the currency is local to the culture's region use <c>"S"</c> (symbol) formatting, otherwise use
    ///   <c>"C"</c> (culture-dependent international) formatting.</description></item>
    /// </list>
    /// <para>The default currency format if no format string is provided is <c>"C"</c> (culture-dependent international).</para>
    /// <para>Number format specifier (required):</para>
    /// <list type="bullet">
    ///   <item><term>"N"</term><description>Number with group separators</description></item>
    ///   <item><term>"D"</term><description>Digits with no separators</description></item>
    /// </list>
    /// <para>The default number format if no format string is provided is <c>"N"</c> (with group separators).</para>
    /// <para>Decimals format specifier (optional):</para>
    /// <list type="bullet">
    ///   <item><term>"*"</term><description>Use no decimal places if possible, otherwise currency's decimal places if possible, otherwise as many decimals as
    ///   needed to represent the value.</description></item>
    ///   <item><term>"$"</term><description>Use the currency's decimal places (or a custom fixed number of decimal places following <c>"$"</c>) with banker's
    ///   "to even" rounding</description></item>
    ///   <item><term>"F"</term><description>Use the currency's decimal places (or a custom fixed number of decimal places following <c>"F"</c>) with "away from
    ///   zero" rounding</description></item>
    /// </list>
    /// <para>The default if no mode is specified uses the currency's decimal places if possible, otherwise as many decimals as needed to represent the value.
    /// If a fixed number of decimal places is specified after <c>"$"</c> or <c>"F"</c>, it must be between 0 and 28.</para>
    /// </remarks>
    /// <exception cref="FormatException">Format specifier was invalid.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        CultureInfo culture;

        if (formatProvider == null)
            formatProvider = culture = CultureInfo.CurrentCulture;
        else
            culture = formatProvider as CultureInfo ?? CultureInfo.CurrentCulture;

        char symbolFormat = default;
        char decimalsFormat = default;
        ReadOnlySpan<char> decimalsFormatNumber = default;
        Span<char> resultNumberFormat = stackalloc char[3];

        if (string.IsNullOrEmpty(format))
        {
            symbolFormat = 'C';
            resultNumberFormat[0] = 'N';
        }
        else if (format.Length is >= 2)
        {
            symbolFormat = char.ToUpperInvariant(format[0]);
            char numberFormat = char.ToUpperInvariant(format[1]);

            if (format.Length >= 3)
            {
                decimalsFormat = char.ToUpperInvariant(format[2]);
                decimalsFormatNumber = format.AsSpan()[3..];
            }

            if (symbolFormat == 'L')
                symbolFormat = GetRegion(culture).ISOCurrencySymbol == _currency?.CurrencyCode ? 'S' : 'C';

            if (numberFormat == 'N')
                resultNumberFormat[0] = 'N';
            else if (numberFormat == 'D')
                resultNumberFormat[0] = 'F';
            else
                Throw.FormatEx();
        }
        else
        {
            Throw.FormatEx();
        }

        decimal absAmount = Math.Abs(_amount);

        if (decimalsFormat == default || decimalsFormat == '*')
        {
            if (decimalsFormatNumber.Length != 0)
                Throw.FormatEx();

            int numDecimalDigits = _amount.GetDecimalDigits();

            if (decimalsFormat == default)
                Math.Max(numDecimalDigits, _currency?.DecimalDigits ?? 0).TryFormat(resultNumberFormat[1..], out _);
            else
                (numDecimalDigits == 0 ? 0 : Math.Max(numDecimalDigits, _currency?.DecimalDigits ?? 0)).TryFormat(resultNumberFormat[1..], out _);
        }
        else if (decimalsFormat == '$' || decimalsFormat == 'F')
        {
            int decimalPlaces = 0;

            if (decimalsFormatNumber.Length == 0)
            {
                decimalPlaces = _currency?.DecimalDigits ?? 0;
                decimalPlaces.TryFormat(resultNumberFormat[1..], out _);
            }
            else if (decimalsFormatNumber.Length <= 2 &&
                int.TryParse(decimalsFormatNumber, NumberStyles.None, CultureInfo.InvariantCulture, out decimalPlaces) &&
                decimalPlaces is > 0 and <= 28)
            {
                decimalPlaces = int.Parse(decimalsFormatNumber);
                decimalsFormatNumber.CopyTo(resultNumberFormat[1..]);
            }
            else
            {
                Throw.FormatEx();
            }

            var rounding = decimalsFormat == '$' ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero;
            absAmount = decimal.Round(absAmount, decimalPlaces, rounding);
        }
        else
        {
            Throw.FormatEx();
        }

        // Enough capacity for whole number (29) + group separators (14) + decimal separator (1) + decimals (29)
        Span<char> number = stackalloc char[73];
        var absNumberFormatInfo = GetAbsNumberFormatInfo(formatProvider);

        if (!absAmount.TryFormat(number, out int numberLength, MemoryExtensions.TrimEnd(resultNumberFormat, default(char)), absNumberFormatInfo))
            Throw.FormatEx();

        number = number[..numberLength];

        if (_currency == null)
        {
            if (symbolFormat is not 'I' or 'R' or 'C' or 'S')
                Throw.FormatEx();

            return number.ToString(); // number must be zero with no currency so we can stop formatting here
        }

        string currencySymbol = null;
        string pattern = null;

        if (symbolFormat is 'I' or 'R' or 'C')
        {
            currencySymbol = _currency.CurrencyCode;

            bool useReverseFormat = symbolFormat switch {
                'I' => false,
                'R' => true,
                _ => culture.TwoLetterISOLanguageName is not "en" or "ga" or "lv" or "mt",
            };

            if (useReverseFormat)
                pattern = _amount >= 0 ? PositiveReverseInternationalPattern : NegativeReverseInternationalPatterns[absNumberFormatInfo.CurrencyNegativePattern];
            else
                pattern = _amount >= 0 ? PositiveInternationalPattern : NegativeInternationalPatterns[absNumberFormatInfo.CurrencyNegativePattern];
        }
        else if (symbolFormat == 'S')
        {
            currencySymbol = _currency.Symbol;
            pattern = _amount >= 0 ? PositiveCurrencyPatterns[absNumberFormatInfo.CurrencyPositivePattern] : NegativeCurrencyPatterns[absNumberFormatInfo.CurrencyNegativePattern];
        }
        else
        {
            Throw.FormatEx();
        }

        return ApplyPattern(number, pattern, currencySymbol, absNumberFormatInfo.NegativeSign);

        static NumberFormatInfo GetAbsNumberFormatInfo(IFormatProvider? formatProvider)
        {
            var formatInfo = NumberFormatInfo.GetInstance(formatProvider);

            return s_absNumberFormatInfoLookup.GetValue(formatInfo, static fi => new NumberFormatInfo {
                NumberDecimalDigits = fi.CurrencyDecimalDigits,
                NumberDecimalSeparator = fi.CurrencyDecimalSeparator,
                NumberGroupSeparator = fi.CurrencyGroupSeparator,
                NumberGroupSizes = fi.CurrencyGroupSizes,
            });
        }

        static RegionInfo GetRegion(CultureInfo culture)
        {
            RegionInfo region = null;

            if ((culture.CultureTypes & CultureTypes.SpecificCultures) != 0 && !s_regionInfoLookup.TryGetValue(culture, out region))
            {
                try
                {
                    region = new RegionInfo(culture.Name);
                }
                catch { }

                s_regionInfoLookup.AddOrUpdate(culture, region);
            }

            return region ?? RegionInfo.CurrentRegion;
        }

        static string ApplyPattern(ReadOnlySpan<char> number, string pattern, string currencySymbol, string negativeSign)
        {
            // 73 for number + extra for spaces, symbol, sign/parenthesis, etc
            Span<char> buffer = stackalloc char[120];
            Span<char> remaining = buffer;

            foreach (char c in pattern)
            {
                switch (c)
                {
                    case 'n':
                        Append(ref remaining, number);
                        break;
                    case '$':
                        Append(ref remaining, currencySymbol);
                        break;
                    case '-':
                        Append(ref remaining, negativeSign);
                        break;
                    case ' ':
                        AppendChar(ref remaining, '\u00A0');
                        break;
                    default:
                        AppendChar(ref remaining, c);
                        break;
                }
            }

            return buffer[..^remaining.Length].ToString();

            static void Append(ref Span<char> span, ReadOnlySpan<char> s)
            {
                s.CopyTo(span);
                span = span[s.Length..];
            }

            static void AppendChar(ref Span<char> span, char c)
            {
                span[0] = c;
                span = span[1..];
            }
        }
    }
}