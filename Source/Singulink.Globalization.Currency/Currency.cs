using System.Diagnostics;
using Singulink.Globalization.Utilities;

namespace Singulink.Globalization;

/// <summary>
/// Provides information about a currency, such as the currency code, localized names, symbol and decimal digits.
/// </summary>
public class Currency : IFormattable
{
    private Dictionary<string, string>? _localizedNameLookup;

    /// <summary>
    /// Gets the currency code assigned to the currency, which is the three letter ISO currency code for system currencies.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Gets the standard number of decimal digits for monetary amounts of this currency.
    /// </summary>
    public int DecimalDigits { get; }

    /// <summary>
    /// Gets the name of this currency. Returns the English name for system currencies.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the symbol used by this currency.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets a monetary amount representing the minor unit of the currency based on the number of decimal digits it has (i.e. USD will return <c>USD 0.01</c>).
    /// </summary>
    public Money MinorUnit => new Money(this, new decimal(1, 0, 0, false, (byte)DecimalDigits));

    /// <summary>
    /// Gets a list containing language identifers and currency names.
    /// </summary>
    public IEnumerable<(string CultureName, string Name)> LocalizedNames => _localizedNameLookup?.Select(kvp => (kvp.Key, kvp.Value)) ?? Array.Empty<(string, string)>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class.
    /// </summary>
    /// <param name="name">The name of the currency, i.e. <c>"Dollars"</c>.</param>
    /// <param name="currencyCode">The unique currency code of the currency, typically the three letter ISO code, i.e. <c>"USD"</c>.</param>
    /// <param name="symbol">The currency symbol, i.e. <c>"$"</c>.</param>
    /// <param name="decimalDigits">The number of decimal digits that is standard for the currency.</param>
    /// <param name="localizedNames">A collection of culture and currency name pairs for localizing the currency name.</param>
    /// <remarks>
    /// The culture names in the <paramref name="localizedNames"/> collection should match the <see cref="CultureInfo.Name"/> property for the cultures they
    /// target. For example, <c>"en"</c> sets a default name for English cultures and <c>"en-US"</c> sets a US English specific variant of the name. It is not
    /// necessary to add entries for localized names that match the <paramref name="name"/> value since that is the default fallback if a culture-specific name
    /// is not found.
    /// </remarks>
    public Currency(string name, string currencyCode, string symbol, int decimalDigits, params (string CultureName, string Name)[] localizedNames)
        : this(name, currencyCode, symbol, decimalDigits, localizedNames.Length == 0 ? null : localizedNames.AsEnumerable()) { }

    /// <inheritdoc cref="Currency(string, string, string, int, ValueTuple{string, string}[])"/>
    public Currency(string name, string currencyCode, string symbol, int decimalDigits, IEnumerable<(string CultureName, string Name)>? localizedNames = null)
    {
        currencyCode = currencyCode.Trim();
        name = name.Trim();
        symbol = symbol.Trim();

        if (currencyCode.Length == 0)
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        if (name.Length == 0)
            throw new ArgumentException("Name is required.", nameof(name));

        if (symbol.Length == 0)
            throw new ArgumentException("Symbol is required.", nameof(symbol));

        if (decimalDigits < 0)
            throw new ArgumentOutOfRangeException(nameof(decimalDigits));

        CurrencyCode = currencyCode;
        Name = name;
        Symbol = symbol;
        DecimalDigits = decimalDigits;

        if (localizedNames != null)
        {
            foreach (var (cultureName, localName) in localizedNames)
            {
                _localizedNameLookup ??= new(StringComparer.OrdinalIgnoreCase);

                if (!_localizedNameLookup.TryAdd(CoerceCultureName(cultureName), CoerceCurrencyName(localName)))
                    throw new ArgumentException($"Duplicate culture name '{cultureName}' in localized names.", nameof(localizedNames));
            }
        }

        static string CoerceCultureName(string? language)
        {
            language = language?.Trim();
            return !string.IsNullOrEmpty(language) ? language : throw new ArgumentException("Localized names contained a culture name that is null or empty", nameof(localizedNames));
        }

        static string CoerceCurrencyName(string? localName)
        {
            localName = localName?.Trim();
            return !string.IsNullOrEmpty(localName) ? localName : throw new ArgumentException("Localized names contained a currency name that is null or empty", nameof(localizedNames));
        }
    }

    /// <summary>
    /// Gets the currency associated with the specified currency code from the default registry.
    /// </summary>
    /// <param name="currencyCode">The currency code of the currency to get.</param>
    /// <param name="currency">The singleton currency object with the specified currency code.</param>
    /// <returns>True if the currency was found, otherwise false.</returns>
    public static bool TryGet(string currencyCode, [MaybeNullWhen(false)] out Currency currency) => CurrencyRegistry.Default.TryGetCurrency(currencyCode, out currency);

    /// <summary>
    /// Gets the currency associated with the specified currency code from the default registry.
    /// </summary>
    /// <param name="currencyCode">The currency code of the currency to get.</param>
    public static Currency Get(string currencyCode) => CurrencyRegistry.Default[currencyCode];

    #region String Formatting

    /// <summary>
    /// Returns the localized string representation of the currency containing the currency's name and currency code.
    /// </summary>
    /// <remarks>
    /// If a localized currency name is available for the current culture or its neutral parent culture then that is used, otherwise the name falls
    /// back to the <see cref="Name"/> property.
    /// </remarks>
    public override string ToString() => ToString(null);

    /// <summary>
    /// Returns a localized string representation of the currency using the specified format and culture.
    /// </summary>
    /// <remarks>
    /// If a localized currency name is available for the specified culture or its neutral parent culture then that is used, otherwise the name falls
    /// back to the <see cref="Name"/> property. If a culture is not provided then the current culture is used.
    /// </remarks>
    /// <param name="format">Valid values are <c>"N"</c> for the name of the currency or <c>"F"</c> for the name and currency code. The default format if none
    /// is provided is <c>"F"</c>.</param>
    /// <param name="culture">The culture that should be used to localize the name of the currency.</param>
    /// <exception cref="FormatException">Format string was invalid.</exception>
    public string ToString(string? format, CultureInfo? culture = null)
    {
        char f = default;

        if (string.IsNullOrEmpty(format))
            f = 'F';
        else if (format.Length == 1)
            f = char.ToUpperInvariant(format[0]);

        culture ??= CultureInfo.CurrentCulture;

        string name;

        if (_localizedNameLookup == null)
        {
            name = Name;
        }
        else if (!_localizedNameLookup.TryGetValue(culture.Name, out name))
        {
            var neutralCulture = culture.GetNeutralCulture();

            if (neutralCulture == null || !_localizedNameLookup.TryGetValue(neutralCulture.Name, out name))
                name = Name;
        }

        if (f == 'N')
            return name;
        else if (f == 'F')
            return $"{name} ({CurrencyCode})";

        throw new FormatException("Invalid format string.");
    }

    /// <summary>
    /// Returns a localized string representation of the currency using the specified format and format provider.
    /// </summary>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(format, formatProvider as CultureInfo);

    #endregion

    internal static CurrencyRegistry CreateSystemRegistry()
    {
        var lookup = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            var region = new RegionInfo(culture.Name);

            if (!lookup.TryGetValue(region.ISOCurrencySymbol, out var currency))
            {
                const string ZeroWidthSpace = "\u200B";
                string symbol = region.CurrencySymbol != ZeroWidthSpace ? region.CurrencySymbol : culture.NumberFormat.CurrencyDecimalSeparator;

                currency = new Currency(region.CurrencyEnglishName, region.ISOCurrencySymbol, symbol, culture.NumberFormat.CurrencyDecimalDigits);
                lookup[currency.CurrencyCode] = currency;
            }

            string localizedName = region.CurrencyNativeName;

            if (localizedName != currency.Name)
            {
                // English cultures will already use the Name property as a default fallback English name, so if the name doesn't match then add the variant to
                // the specific culture. Other cultures should all have the same name for their neutral culture so we can just add the name to the neutral
                // culture to save memory.

                string localizationCultureName = (culture.TwoLetterISOLanguageName == "en" ? culture : culture.GetNeutralCulture()!).Name;
                var localizedNameLookup = currency._localizedNameLookup ??= new(StringComparer.OrdinalIgnoreCase);

                if (localizedNameLookup.TryGetValue(localizationCultureName, out string existingLocalizedName) && existingLocalizedName != localizedName)
                {
                    // This shouldn't happen, but if the data changes and it does then make this future-proof by adding the localized name to the specific
                    // culture instead to override the different neutral culture name that was set.
                    Debug.Fail("Neutral localization culture name was already set to a different name.");
                    localizedNameLookup.Add(culture.Name, localizedName);
                }
                else
                {
                    localizedNameLookup[localizationCultureName] = localizedName;
                }
            }
        }

        CultureInfo.InvariantCulture.ClearCachedData(); // Clears all cached data, not just the culture it is called on.

        return new CurrencyRegistry("System", lookup.Values);
    }
}