using System.Diagnostics;

namespace Singulink.Globalization;

/// <summary>
/// Provides information about a currency, such as the currency code, localized names, symbol and rounding rules.
/// </summary>
[DebuggerDisplay("{CurrencyCode,nq}")]
public partial class Currency : IFormattable
{
    private readonly ICurrencyLocalizer _localizer;
    private readonly RoundingPolicy _roundingPolicy = RoundingPolicy.Default;
    private readonly RoundingPolicy? _cashRoundingPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class without localization support.
    /// </summary>
    /// <param name="currencyCode">The unique currency code of the currency, typically the three letter ISO code, i.e. <c>"USD"</c>.</param>
    /// <param name="name">The name of the currency, i.e. <c>"US Dollars"</c>.</param>
    /// <param name="symbol">The currency symbol, i.e. <c>"$"</c>. The currency code is used as the symbol if the symbol is not provided.</param>
    /// <remarks>
    /// NOTE: Currency codes and symbols are not validated as parsable to allow this library to be used for a wide range of usages (such as defining
    /// cryptocurrencies, some of which do conform to ISO 4217 standards that ensure values can be reliably parsed). If you create a currency that contains
    /// unparsable symbols/codes, then any parsing operations that depend on the offending symbol/code will throw an <see cref="InvalidOperationException"/>.
    /// See <see cref="IsSymbolOrCodeParsable(string, out string?)"/> for more information on this topic.
    /// </remarks>
    public Currency(string currencyCode, string name, string? symbol = null)
        : this(currencyCode, new InvariantCurrencyLocalizer(name, symbol ?? currencyCode)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class using the specified localizer to provide localized currency names and symbols.
    /// </summary>
    /// <param name="currencyCode">The unique currency code of the currency, typically the three letter ISO code, i.e. <c>"USD"</c>.</param>
    /// <param name="localizer">The currency localizer implementation that provides localized names and symbols for the currency.</param>
    /// <remarks>
    /// NOTE: Currency codes and symbols are not validated as parsable to allow this library to be used for a wide range of usages (such as defining
    /// cryptocurrencies, some of which do conform to ISO 4217 standards that ensure values can be reliably parsed). If you create a currency that contains
    /// unparsable symbols/codes, then any parsing operations that depend on the offending symbol/code will throw an <see cref="InvalidOperationException"/>.
    /// See <see cref="IsSymbolOrCodeParsable(string, out string?)"/> for more information on this topic.
    /// </remarks>
    public Currency(string currencyCode, ICurrencyLocalizer localizer)
    {
        CurrencyCode = currencyCode;
        _localizer = localizer;
    }

    /// <summary>
    /// Gets the currency code assigned to the currency. This is typically a three-letter ISO code, such as <c>"USD"</c> for US Dollars.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Gets the invariant name of this currency. Returns the English name for system currencies. To get the localized name use <see
    /// cref="GetLocalizedName(CultureInfo)"/>.
    /// </summary>
    public string Name => _localizer.GetName(this, CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets or initializes the rounding rules for monetary amounts of this currency, which consist of the number of decimal digits and any additional
    /// rounding increment. Defaults to <see cref="RoundingPolicy.Default"/> (<c>2</c> decimal digits) if not initialized.
    /// </summary>
    public RoundingPolicy RoundingPolicy
    {
        get => _roundingPolicy;
        init => _roundingPolicy = value;
    }

    /// <summary>
    /// Gets or initializes the rounding rules for cash amounts of this currency, which may differ from <see cref="RoundingPolicy"/> because of the
    /// physical denominations that are in circulation, i.e. Canadian dollar cash amounts are rounded to the nearest <c>0.05</c> since the penny was
    /// withdrawn. Returns <see langword="null"/> if cash rounding rules are not known for this currency.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cash rounding information is not available from the system globalization data that the <see cref="CurrencyRegistry.Default"/> registry is built
    /// from by default. Install the <c>Singulink.Globalization.Currency.Cldr</c> package and register its data provider as the default registry with <see
    /// cref="CurrencyRegistry.SetDefault(ICurrencyDataProvider, CurrencyTypes)"/> to get cash rounding rules sourced from the Unicode Common Locale Data
    /// Repository (CLDR), or initialize this property when creating custom currencies.
    /// </para>
    /// </remarks>
    public RoundingPolicy? CashRoundingPolicy
    {
        get => _cashRoundingPolicy;
        init => _cashRoundingPolicy = value;
    }

    /// <summary>
    /// Gets the number of decimal digits that monetary amounts of this currency are rounded to. This is a convenience property that returns the
    /// <see cref="RoundingPolicy.DecimalDigits"/> value of the currency's <see cref="RoundingPolicy"/>.
    /// </summary>
    public int DecimalDigits => _roundingPolicy.DecimalDigits;

    /// <summary>
    /// Gets the invariant symbol of this currency. To get the localized symbol use <see cref="GetLocalizedSymbol(CultureInfo)"/>.
    /// </summary>
    public string Symbol => _localizer.GetSymbol(this, CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the currency associated with the specified currency code from the <see cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <param name="currencyCode">The currency code of the currency to get.</param>
    public static Currency GetCurrency(TargetDependentStringKey currencyCode) => CurrencyRegistry.Default[currencyCode];

    /// <summary>
    /// Gets the localized symbol for the currency in the specified culture (or the current culture if no culture is provided).
    /// </summary>
    public string GetLocalizedSymbol(CultureInfo? culture = null) => _localizer.GetSymbol(this, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Gets the localized name for the currency in the specified culture (or the current culture if no culture is provided).
    /// </summary>
    public string GetLocalizedName(CultureInfo? culture = null) => _localizer.GetName(this, culture ?? CultureInfo.CurrentCulture);

    /// <summary>
    /// Checks whether the specified symbol/code can be parsed. If this method returns <see langword="false"/>, then adding a currency that uses the symbol/code
    /// to a currency registry will cause parsing operations that depend on the offending symbol/code to throw an `<see cref="InvalidOperationException"/>, as
    /// it may otherwise result in unpredictable parsing errors or unreliable/incorrect results.
    /// </summary>
    /// <param name="symbolOrCode">The symbol or code to check.</param>
    /// <param name="error">When this method returns <see langword="false"/>, this will contain an error message that describes why the symbol or code is not
    /// parsable.</param>
    /// <remarks>
    /// <para>
    /// In order for a symbol or code to be considered parsable, it must not contain any of the following characters: <c>'+'</c>, <c>'-'</c> (minus sign),
    /// <c>'−'</c> (minus-hyphen), <c>'('</c> or <c>')'</c>, numbers or whitespace (except for the non-breaking space character <c>'\u202F'</c>).</para>
    /// <para>
    /// You should use this method to pre-validate custom or user-provided symbols/codes that may be added to a registry that will be used for parsing to avoid
    /// ending up with a registry that will throw errors when parsing is attempted. Currencies and registries that contain unparsable symbols/codes can still be
    /// used for formatting monetary values and doing any other operations that do not involve parsing (including any lookup operations like <see
    /// cref="CurrencyRegistry.TryGetCurrency(string, out Currency)"/> and <see cref="CurrencyRegistry.TryGetCurrenciesBySymbol(string, out
    /// IReadOnlyList{Currency})"/>.</para>
    /// </remarks>
    public static bool IsSymbolOrCodeParsable(string symbolOrCode, [NotNullWhen(false)] out string? error)
    {
        if (symbolOrCode.Length is 0)
        {
            error = "Symbol/code cannot be empty.";
            return false;
        }

        if (symbolOrCode.Any(c => char.IsNumber(c) || (char.IsWhiteSpace(c) && c is not '\u202F')))
        {
            error = "Symbol/code cannot contain any numbers or whitespace.";
            return false;
        }

        // Note: the two hyphens below are different characters, don't remove one of them
        if (symbolOrCode.Any(c => c is '+' or '-' or '−' or '(' or ')'))
        {
            error = "Symbol/code cannot contain any of the following characters: + - ( )";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Returns the localized string representation of the currency containing the currency's name and currency code.
    /// </summary>
    public override string ToString() => ToString(null);

    /// <summary>
    /// Returns a localized string representation of the currency using the specified format and format provider. If the format provider is not a <see
    /// cref="CultureInfo"/> instance, it is ignored and the current culture is used instead.
    /// </summary>
    string IFormattable.ToString(string? format, IFormatProvider? provider) => ToString(format, provider as CultureInfo);

    /// <summary>
    /// Returns a localized string representation of the currency using the specified format and culture.
    /// </summary>
    /// <param name="format">Valid values are <c>"L"</c> (long) for the name and currency code or <c>"S"</c> (short) for just the name of the currency. The
    /// default format if none is provided is <c>"L"</c>.</param>
    /// <param name="culture">The culture that should be used to localize the name of the currency (the current culture is used if this parameter is not
    /// provided.</param>
    /// <exception cref="FormatException">Format string was invalid.</exception>
    public string ToString(string? format, CultureInfo? culture = null)
    {
        char f = default;

        if (string.IsNullOrEmpty(format))
            f = 'L';
        else if (format.Length == 1)
            f = char.ToUpperInvariant(format[0]);

        culture ??= CultureInfo.CurrentCulture;

        if (f == 'L')
            return $"{GetLocalizedName(culture)} ({CurrencyCode})";
        if (f == 'S')
            return GetLocalizedName(culture);

        throw new FormatException("Invalid format string.");
    }

    /// <summary>
    /// Gets the currency associated with the specified currency code from the <see cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <param name="currencyCode">The currency code of the currency to get.</param>
    /// <param name="currency">The singleton currency object with the specified currency code.</param>
    /// <returns><see langword="true"/> if the currency was found, otherwise <see langword="false"/>.</returns>
    public static bool TryGetCurrency(TargetDependentStringKey currencyCode, [MaybeNullWhen(false)] out Currency currency)
        => CurrencyRegistry.Default.TryGetCurrency(currencyCode, out currency);

    /// <summary>
    /// Gets the local currency for the current culture using the <see cref="CurrencyRegistry.Default"/> currency registry. Culture must be region-specific for
    /// this method to succeed.
    /// </summary>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the culture was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetLocalCurrency([MaybeNullWhen(false)] out Currency currency) =>
        CurrencyRegistry.Default.TryGetLocalCurrency(CultureInfo.CurrentCulture, out currency);

    /// <summary>
    /// Gets the local currency for the specified culture using the <see cref="CurrencyRegistry.Default"/> currency registry. Culture must be region-specific
    /// for this method to succeed.
    /// </summary>
    /// <param name="culture">The culture to get a local currency for.</param>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the culture was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetLocalCurrency(CultureInfo culture, [MaybeNullWhen(false)] out Currency currency) =>
        CurrencyRegistry.Default.TryGetLocalCurrency(culture, out currency);

    /// <summary>
    /// Gets the local currency for the specified region using the <see cref="CurrencyRegistry.Default"/> currency registry.
    /// </summary>
    /// <param name="region">The region to get a local currency for.</param>
    /// <param name="currency">When the method returns, contains the currency if it was found; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a local currency for the region was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetLocalCurrency(RegionInfo region, [MaybeNullWhen(false)] out Currency currency)
        => CurrencyRegistry.Default.TryGetLocalCurrency(region, out currency);

    internal static string GetSystemSymbol(CultureInfo culture, RegionInfo region)
        => region.CurrencySymbol != Constants.ZeroWidthSpace ? region.CurrencySymbol : culture.NumberFormat.CurrencyDecimalSeparator;

    internal RoundingPolicy GetRequiredCashRoundingPolicy()
    {
        return _cashRoundingPolicy ?? Throw();

        [DoesNotReturn]
        RoundingPolicy Throw()
        {
            throw new NotSupportedException(
                $"Cash rounding rules are not available for currency '{CurrencyCode}'. Cash rounding information is not included in system globalization " +
                "data. Install the Singulink.Globalization.Currency.Cldr package and register its data provider as the default currency registry, or " +
                $"initialize the {nameof(CashRoundingPolicy)} property when creating custom currencies.");
        }
    }
}