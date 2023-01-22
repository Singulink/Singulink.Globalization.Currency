namespace Singulink.Globalization;

internal sealed class CurrencyByCodeComparer : Comparer<Currency>
{
    public static new CurrencyByCodeComparer Default { get; } = new CurrencyByCodeComparer();

    private CurrencyByCodeComparer()
    {
    }

    /// <summary>
    /// Compares currencies and returns an integer that indicates their relative position in the sort order. Currencies are ordered by <see
    /// cref="Currency.CurrencyCode"/>.
    /// </summary>
    public override int Compare(Currency? x, Currency? y) => string.Compare(x?.CurrencyCode, y?.CurrencyCode, StringComparison.OrdinalIgnoreCase);
}