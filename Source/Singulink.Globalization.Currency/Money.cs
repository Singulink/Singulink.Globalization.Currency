using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Singulink.Globalization;

#pragma warning disable SA1203 // Constants should appear before fields

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
public readonly struct Money : IFormattable, IComparable<Money>, IEquatable<Money>
{
    /// <summary>
    /// Gets the default <see cref="Money"/> value, which is not associated with any currency and has a zero amount.
    /// </summary>
    public static Money Default => default;

    private readonly Currency? _currency;
    private readonly decimal _amount;

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct.
    /// </summary>
    public Money(decimal amount, string? currencyCode) : this(amount, currencyCode == null ? null : Currency.Get(currencyCode))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> struct.
    /// </summary>
    public Money(decimal amount, Currency? currency)
    {
        if (currency == null && amount != 0)
            Throw();

        _amount = amount;
        _currency = currency;

        static void Throw() => throw new ArgumentException("Currency must be specified for non-zero amounts.");
    }

    /// <summary>
    /// Gets the currency associated with this value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Attempted to get the currency on a default value which has no currency associated with it.
    /// </exception>
    public Currency Currency => _currency ?? throw new InvalidOperationException("Default money values do not have a currency associated with them.");

    /// <summary>
    /// Gets the currency associated with this value or <see langword="null"/> if this is a default money value with no currency associated with it.
    /// </summary>
    public Currency? CurrencyOrDefault => _currency;

    /// <summary>
    /// Gets the amount this value represents in its currency.
    /// </summary>
    public decimal Amount => _amount;

    /// <summary>
    /// Gets a value indicating whether this is a default value. Default values have a <c>0</c> amount and do not have a currency associated with them.
    /// </summary>
    [MemberNotNullWhen(false, nameof(CurrencyOrDefault))]
    [MemberNotNullWhen(false, nameof(_currency))]
    public bool IsDefault => _currency == null;

    #region Operators

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static bool operator ==(Money x, Money y) => x.Equals(y);

    public static bool operator !=(Money x, Money y) => !x.Equals(y);

    public static bool operator <(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount < y._amount;
    }

    public static bool operator >(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount > y._amount;
    }

    public static bool operator <=(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount <= y._amount;
    }

    public static bool operator >=(Money x, Money y)
    {
        EnsureSameCurrencyForCompare(x._currency, y._currency);
        return x._amount >= y._amount;
    }

    public static Money operator +(Money x, Money y) => new(x._amount + y._amount, CombineCurrencies(x._currency, y._currency));

    public static Money operator +(Money x, decimal y) => new(x._amount + y, x._currency);

    public static Money operator +(decimal x, Money y) => y + x;

    public static Money operator -(Money x, Money y) => new(x._amount - y._amount, CombineCurrencies(x._currency, y._currency));

    public static Money operator -(Money x, decimal y) => new(x._amount - y, x._currency);

    public static Money operator -(decimal x, Money y) => new(x - y._amount, y._currency);

    public static Money operator *(Money x, decimal y) => new(x._amount * y, x._currency);

    public static Money operator *(decimal x, Money y) => y * x;

    public static Money operator /(Money x, decimal y) => new(x._amount / y, x._currency);

    public static Money operator /(decimal x, Money y) => new(x / y._amount, y._currency);

    public static Money operator ++(Money value) => new(value.Amount + 1, value._currency);

    public static Money operator --(Money value) => new(value.Amount - 1, value._currency);

    public static Money operator +(Money value) => value;

    public static Money operator -(Money value) => new(-value.Amount, value._currency);

#pragma warning restore CS1591

    #endregion

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using <see cref="MidpointRounding.ToEven"/> midpoint rounding ("banker's
    /// rounding").
    /// </summary>
    public Money RoundToCurrencyDigits() => RoundToCurrencyDigits(MidpointRounding.ToEven);

    /// <summary>
    /// Returns a value rounded to the currency's number of decimal digits using the specified midpoint rounding mode.
    /// </summary>
    public Money RoundToCurrencyDigits(MidpointRounding mode)
    {
        return _currency == null ? this : new Money(Math.Round(_amount, _currency.DecimalDigits, mode), Currency);
    }

    /// <summary>
    /// Returns <see cref="Default"/> if this value's <see cref="Amount"/> is <c>0</c>, otherwise returns this value.
    /// </summary>
    public Money ToDefaultIfZero() => _amount == 0 ? default : this;

    #region Equality and Comparison

    /// <summary>
    /// Compares this value to the specified value. Values in different currencies are ordered by their currencies first.
    /// </summary>
    /// <exception cref="ArgumentException">Invalid comparison between values that have different currencies.</exception>
    public int CompareTo(Money other)
    {
        EnsureSameCurrencyForCompare(_currency, other._currency);
        return _amount.CompareTo(other._amount);
    }

    /// <summary>
    /// Determines whether the two specified values are equal.
    /// </summary>
    public static bool Equals(Money x, Money y) => x.Equals(y);

    /// <summary>
    /// Determines whether this value is equal to the specifiied object.
    /// </summary>
    public override bool Equals(object? obj) => obj is Money value && Equals(value);

    /// <summary>
    /// Determines whether this value is equal to the specified value.
    /// </summary>
    public bool Equals(Money value) => _currency == value._currency && _amount == value._amount;

    /// <summary>
    /// Gets the hash code for this value.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(_currency, _amount);

    private static void EnsureSameCurrencyForCompare(Currency? x, Currency? y)
    {
        if (x != y)
            Throw();

        static void Throw() => throw new ArgumentException("Currencies must match in order to compare money values.");
    }

    private static Currency? CombineCurrencies(Currency? x, Currency? y)
    {
        if (x == y)
            return x;

        if (x is null)
            return y;

        if (y is not null)
            Throw();

        return x;

        static void Throw() => throw new ArgumentException("Currencies must match (or one of the values can be a default value that has no currency associated with it).");
    }

    #endregion

    #region String Formatting

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
    ///   <item><term>"N"</term><description>Number with group seperators</description></item>
    ///   <item><term>"D"</term><description>Digits with no seperators</description></item>
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

        if (!absAmount.TryFormat(number, out var numberLength, MemoryExtensions.TrimEnd(resultNumberFormat, default(char)), absNumberFormatInfo))
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
                        Append(ref buffer, number);
                        break;
                    case '$':
                        Append(ref buffer, currencySymbol);
                        break;
                    case '-':
                        Append(ref buffer, negativeSign);
                        break;
                    case ' ':
                        AppendChar(ref buffer, '\u00A0');
                        break;
                    default:
                        AppendChar(ref buffer, c);
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

    #endregion
}