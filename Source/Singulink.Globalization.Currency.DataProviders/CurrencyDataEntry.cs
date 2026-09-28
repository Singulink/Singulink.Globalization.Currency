namespace Singulink.Globalization;

/// <summary>
/// Describes a currency provided by an <see cref="ICurrencyDataProvider"/>.
/// </summary>
public sealed class CurrencyDataEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyDataEntry"/> class.
    /// </summary>
    /// <param name="currencyCode">The unique currency code of the currency, typically the three letter ISO 4217 code, i.e. <c>"USD"</c>.</param>
    /// <param name="name">The invariant (English) name of the currency, i.e. <c>"US Dollar"</c>.</param>
    public CurrencyDataEntry(string currencyCode, string name)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required.", nameof(currencyCode));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        CurrencyCode = currencyCode;
        Name = name;
    }

    /// <summary>
    /// Gets the unique currency code of the currency, typically the three letter ISO 4217 code, i.e. <c>"USD"</c>.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Gets the invariant (English) name of the currency, i.e. <c>"US Dollar"</c>. This is the name used when no localized name is available for a culture.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or initializes the invariant symbol of the currency, i.e. <c>"$"</c>. This is the symbol used when no localized symbol is available for a
    /// culture. If <see langword="null"/>, the currency code is used as the symbol.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Gets or initializes the number of decimal digits that monetary amounts of the currency are rounded to. Defaults to <c>2</c>.
    /// </summary>
    public int DecimalDigits { get; init; } = 2;

    /// <summary>
    /// Gets or initializes the rounding increment for monetary amounts in units of the last decimal digit, or <c>0</c> for no additional rounding beyond
    /// the number of decimal digits. Defaults to <c>0</c>.
    /// </summary>
    public int DecimalUnits { get; init; }

    /// <summary>
    /// Gets or initializes the number of decimal digits that cash amounts of the currency are rounded to, or <see langword="null"/> to use
    /// <see cref="DecimalDigits"/>.
    /// </summary>
    public int? CashDecimalDigits { get; init; }

    /// <summary>
    /// Gets or initializes the rounding increment for cash amounts in units of the last cash decimal digit, or <see langword="null"/> to use
    /// <see cref="DecimalUnits"/>. For example, a value of <c>5</c> with <c>2</c> cash decimal digits rounds cash amounts to the nearest <c>0.05</c>.
    /// </summary>
    public int? CashDecimalUnits { get; init; }

    /// <summary>
    /// Gets or initializes the type of the currency, which determines which registries it is included in. Must be a single type. Defaults to
    /// <see cref="CurrencyTypes.CurrentTender"/>.
    /// </summary>
    public CurrencyTypes Type { get; init; } = CurrencyTypes.CurrentTender;
}
