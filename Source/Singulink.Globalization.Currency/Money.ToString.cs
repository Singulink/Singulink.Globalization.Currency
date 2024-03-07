using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Singulink.Globalization;

/// <content>
/// Contains the ToString implementation of the <see cref="Money"/> struct.
/// </content>
partial struct Money : IFormattable
{
    private const string PositiveInternationalPattern = "$ n";
    private const string PositiveReverseInternationalPattern = "n $";
    private static readonly ImmutableArray<string> PositiveCurrencyPatterns = ["$n", "n$", "$ n", "n $"];

    private static readonly ImmutableArray<string> NegativeCurrencyPatterns
        = ["($n)",  "-$n",  "$-n",  "$n-",  "(n$)",  "-n$",  "n-$",  "n$-",  "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)"];
    private static readonly ImmutableArray<string> NegativeInternationalPatterns
        = ["$ (n)", "$ -n", "$ -n", "$ n-", "$ (n)", "$ -n", "$ n-", "$ n-", "$ -n", "$ -n", "$ n-", "$ n-", "$ -n", "$ n-", "$ (n)", "$ (n)"];
    private static readonly ImmutableArray<string> NegativeReverseInternationalPatterns
        = ["(n) $", "-n $", "-n $", "n- $", "(n) $", "-n $", "n- $", "n- $", "-n $", "-n $", "n- $", "n- $", "-n $", "n- $", "(n) $", "(n) $"];

    private static readonly ConditionalWeakTable<CultureInfo, RegionInfo?> _regionInfoLookup = [];
    private static readonly ConditionalWeakTable<NumberFormatInfo, NumberFormatInfo> _absNumberFormatInfoLookup = [];

    /// <summary>
    /// Returns a culture-dependent international string representation of this value's currency and amount.
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Returns a string representation of this value's currency and amount.
    /// </summary>
    /// <param name="format">The string format to use.</param>
    /// <param name="formatProvider">The format provider that will be used to obtain number format information. This should be a region-specific <see
    /// cref="CultureInfo"/> instance for formats that depend on the culture, otherwise the current culture/region is used.</param>
    /// <remarks>
    /// <para>String format is composed of 3 parts, any of which can be omitted, as long as they are in the following order:</para>
    /// <para>Currency format specifier:</para>
    /// <list type="bullet">
    ///   <item><term>"C"</term><description>Culture-Dependent International: Currency code before the amount for English, Irish, Latvian and Maltese
    ///   language cultures, otherwise currency code after the amount.</description></item>
    ///   <item><term>"I"</term><description>International: Use currency code before the amount (<c>"USD 1.23"</c>).</description></item>
    ///   <item><term>"R"</term><description>Reverse International: Use currency code after the amount (<c>"1.23 USD"</c>).</description></item>
    ///   <item><term>"S"</term><description>Symbol: Use currency symbol with placement dictated by the format provider (<c>$1.23</c>).</description></item>
    ///   <item><term>"L"</term><description>Local: If the currency is local to the culture's region use <c>"S"</c> (symbol) formatting, otherwise use
    ///   <c>"C"</c> (culture-dependent international) formatting.</description></item>
    /// </list>
    /// <para>The default currency format is <c>"C"</c> (culture-dependent international).</para>
    /// <para>Number format specifier:</para>
    /// <list type="bullet">
    ///   <item><term>"N"</term><description>Number with group separators</description></item>
    ///   <item><term>"D"</term><description>Digits with no separators</description></item>
    /// </list>
    /// <para>The default number format is <c>"N"</c> (number with group separators).</para>
    /// <para>Decimals format specifier:</para>
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
        Span<char> resultNumberFormat = stackalloc char[3];
        ParseFormat(format, out char symbolFormat, out char decimalsFormat, out int decimalsFormatNumber, out resultNumberFormat[0]);

        CultureInfo culture = GetCultureAndSetFormatProvider(ref formatProvider);
        RegionInfo? region = null;

        decimal absFormatAmount = GetFormatAmountAndDecimalPlaces(Math.Abs(_amount), _currency, decimalsFormat, decimalsFormatNumber, out int formatDecimalPlaces);
        formatDecimalPlaces.TryFormat(resultNumberFormat[1..], out int formatDecimalPlacesLength, null, CultureInfo.InvariantCulture);
        resultNumberFormat = resultNumberFormat[..(1 + formatDecimalPlacesLength)];

        // Enough capacity for whole number (29) + group separators (14) + decimal separator (1) + decimals (29)
        Span<char> number = stackalloc char[73];
        var absNumberFormatInfo = GetAbsNumberFormatInfo(formatProvider);

        if (!absFormatAmount.TryFormat(number, out int numberLength, resultNumberFormat, absNumberFormatInfo))
            throw new UnreachableException($"Unexpected number formatting failure (resultNumberFormat: '{resultNumberFormat}').");

        number = number[..numberLength];

        if (_currency is null)
            return number.ToString(); // number must be zero with no currency so we can stop formatting here

        string currencySymbol;
        string pattern;
        bool placeCurrencySymbolAsDecimalSeparator = false;

        if (symbolFormat is 'L')
        {
            region = GetRegionForCulture(culture);
            symbolFormat = region.ISOCurrencySymbol == _currency.CurrencyCode ? 'S' : 'C';
        }

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
        else
        {
            Debug.Assert(symbolFormat is 'S', "Unexpected symbol format.");

            currencySymbol = _currency.Symbol;

            if (absNumberFormatInfo.NumberDecimalSeparator is "$" && (region ?? GetRegionForCulture(culture)).CurrencySymbol == Constants.ZeroWidthSpace)
            {
                if (currencySymbol is "$")
                {
                    currencySymbol = string.Empty;
                }
                else
                {
                    placeCurrencySymbolAsDecimalSeparator = true;
                }
            }

            pattern = _amount >= 0 ?
                PositiveCurrencyPatterns[absNumberFormatInfo.CurrencyPositivePattern] :
                NegativeCurrencyPatterns[absNumberFormatInfo.CurrencyNegativePattern];
        }

        if (currencySymbol == absNumberFormatInfo.CurrencyDecimalSeparator)
            currencySymbol = string.Empty;

        return ApplyPattern(number, pattern, currencySymbol, absNumberFormatInfo.NegativeSign, placeCurrencySymbolAsDecimalSeparator);
    }

    private static string ApplyPattern(ReadOnlySpan<char> number, string pattern, string currencySymbol, string negativeSign, bool placeCurrencySymbolAsDecimalSeparator)
    {
        // Enough capacity for number (73) + symbol (20) + other pattern elements (3: spaces/sign/parenthesis)
        Span<char> buffer = stackalloc char[96];
        Span<char> remaining = buffer;

        foreach (char c in pattern)
        {
            switch (c)
            {
                case 'n':
                    if (placeCurrencySymbolAsDecimalSeparator)
                    {
                        int decimalIndex = number.IndexOf('$');

                        if (decimalIndex == -1)
                        {
                            Append(ref remaining, number);
                            Append(ref remaining, currencySymbol);
                        }
                        else
                        {
                            Append(ref remaining, number[..decimalIndex]);
                            Append(ref remaining, currencySymbol);
                            Append(ref remaining, number[(decimalIndex + 1)..]);
                        }
                    }
                    else
                    {
                        Append(ref remaining, number);
                    }

                    break;

                case '$':
                    if (!placeCurrencySymbolAsDecimalSeparator && currencySymbol != Constants.ZeroWidthSpace)
                        Append(ref remaining, currencySymbol);
                    break;

                case '-':
                    Append(ref remaining, negativeSign);
                    break;

                case ' ':
                    const char NoBreakSpace = '\u00A0';
                    AppendChar(ref remaining, NoBreakSpace);
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

    private static CultureInfo GetCultureAndSetFormatProvider(ref IFormatProvider? formatProvider)
    {
        CultureInfo culture;

        if (formatProvider == null)
            formatProvider = culture = CultureInfo.CurrentCulture;
        else
            culture = formatProvider as CultureInfo ?? CultureInfo.CurrentCulture;

        return culture;
    }

    private static NumberFormatInfo GetAbsNumberFormatInfo(IFormatProvider? formatProvider)
    {
        var formatInfo = NumberFormatInfo.GetInstance(formatProvider);

        return _absNumberFormatInfoLookup.GetValue(formatInfo, static fi => new NumberFormatInfo {
            NumberDecimalDigits = fi.CurrencyDecimalDigits,
            NumberDecimalSeparator = fi.CurrencyDecimalSeparator,
            NumberGroupSeparator = fi.CurrencyGroupSeparator,
            NumberGroupSizes = fi.CurrencyGroupSizes,
        });
    }

    private static decimal GetFormatAmountAndDecimalPlaces(decimal amount, Currency? currency, char decimalsFormat, int decimalsFormatNumber, out int formatDecimalPlaces)
    {
        if (decimalsFormat == default || decimalsFormat == '*')
        {
            int amountDecimalDigits = amount.GetDecimalDigits();
            formatDecimalPlaces = decimalsFormat == '*' && amountDecimalDigits == 0 ? 0 : Math.Max(amountDecimalDigits, currency?.DecimalDigits ?? 0);
        }
        else
        {
            Debug.Assert(decimalsFormat is '$' or 'F', "Unexpected decimals format");
            formatDecimalPlaces = decimalsFormatNumber >= 0 ? decimalsFormatNumber : currency?.DecimalDigits ?? 0;

            var rounding = decimalsFormat is '$' ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero;
            amount = decimal.Round(amount, formatDecimalPlaces, rounding);
        }

        return amount;
    }

    private static RegionInfo GetRegionForCulture(CultureInfo culture)
    {
        RegionInfo region = null;

        if ((culture.CultureTypes & CultureTypes.SpecificCultures) != 0 && !_regionInfoLookup.TryGetValue(culture, out region))
        {
            try
            {
                region = new RegionInfo(culture.Name);
            }
            catch { }

            _regionInfoLookup.AddOrUpdate(culture, region);
        }

        return region ?? RegionInfo.CurrentRegion;
    }

    private static void ParseFormat(string? format, out char symbolFormat, out char decimalsFormat, out int decimalsFormatNumber, out char resultNumberFormat)
    {
        if (string.IsNullOrEmpty(format))
        {
            symbolFormat = 'C';
            resultNumberFormat = 'N';
            decimalsFormat = default;
            decimalsFormatNumber = -1;
        }
        else
        {
            char current = char.ToUpperInvariant(format[0]);
            int currentIndex = 0;

            void Next()
            {
                current = ++currentIndex >= format.Length ? default : char.ToUpperInvariant(format[currentIndex]);
            }

            // Currency format specifier

            if (current is 'C' or 'I' or 'R' or 'S' or 'L')
            {
                symbolFormat = current;
                Next();
            }
            else
            {
                symbolFormat = 'C';
            }

            // Number format specifier

            if (current is 'D')
            {
                resultNumberFormat = 'F';
                Next();
            }
            else
            {
                resultNumberFormat = 'N';

                if (current is 'N')
                    Next();
            }

            // Decimals format specifier

            if (current is '*' or '$' or 'F')
            {
                decimalsFormat = current;
                Next();

                if (current is not default(char) && decimalsFormat is not '*')
                {
                    var decimalsFormatNumberSpan = format.AsSpan()[currentIndex..];

                    if (decimalsFormatNumberSpan.Length > 2 ||
                        !int.TryParse(decimalsFormatNumberSpan, NumberStyles.None, CultureInfo.InvariantCulture, out decimalsFormatNumber) ||
                        decimalsFormatNumber is < 0 or > 28)
                    {
                        Throw();
                    }

                    // Processed format to the end, return early to skip the validation/throw at the end.
                    return;
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
                Throw();
        }

        [DoesNotReturn]
        static void Throw() => throw new FormatException("Format specifier was invalid.");
    }
}