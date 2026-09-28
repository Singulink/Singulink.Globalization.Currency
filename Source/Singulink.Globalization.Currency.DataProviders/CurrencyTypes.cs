namespace Singulink.Globalization;

/// <summary>
/// Specifies types of currencies. A <see cref="CurrencyDataEntry"/> has a single type, and registries are created from a combination of types.
/// </summary>
[Flags]
public enum CurrencyTypes
{
    /// <summary>
    /// Currencies that are currently legal tender in at least one region, i.e. <c>USD</c>, <c>EUR</c> or <c>JPY</c>.
    /// </summary>
    CurrentTender = 1,

    /// <summary>
    /// Currencies and currency-like codes that are in current use but are not legal tender, such as precious metals (<c>XAU</c>, <c>XAG</c>), special
    /// drawing rights (<c>XDR</c>), funds codes (<c>CHE</c>, <c>USN</c>) and the testing and "no currency" codes (<c>XTS</c>, <c>XXX</c>).
    /// </summary>
    CurrentNonTender = 2,

    /// <summary>
    /// Currencies that are no longer in use anywhere, i.e. <c>DEM</c> (German mark) or <c>FRF</c> (French franc).
    /// </summary>
    Historical = 4,

    /// <summary>
    /// All currency types.
    /// </summary>
    All = CurrentTender | CurrentNonTender | Historical,
}
