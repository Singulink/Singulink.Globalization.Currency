namespace Singulink.Globalization.Utilities;

internal static class RegionInfoExtensions
{
    public static bool DecimalSeparatorIsCurrencySymbol(this RegionInfo regionInfo)
    {
        string symbol = regionInfo.CurrencySymbol;
        return symbol.Length == 1 && char.GetUnicodeCategory(symbol[0]) == UnicodeCategory.Format;
    }
}