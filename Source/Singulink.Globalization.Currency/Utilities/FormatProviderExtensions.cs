using System.Runtime.CompilerServices;

namespace Singulink.Globalization.Utilities;

internal static class FormatProviderExtensions
{
#pragma warning disable IDE0028 // Simplify collection initialization (not supported on NS2)
    private static readonly ConditionalWeakTable<NumberFormatInfo, NumberFormatInfo> _numberFormatInfoLookup = new();
#pragma warning restore IDE0028

    internal static (NumberFormatInfo OriginalFormatInfo, NumberFormatInfo AlteredNumberFormatInfo) GetCurrencyNumberFormatInfos(this IFormatProvider provider)
    {
        var originalFormatInfo = NumberFormatInfo.GetInstance(provider);

        if (_numberFormatInfoLookup.TryGetValue(originalFormatInfo, out var alteredFormatInfo))
        {
            if (!originalFormatInfo.IsReadOnly)
                CopyNumberInfo(originalFormatInfo, alteredFormatInfo);
        }
        else
        {
            alteredFormatInfo = _numberFormatInfoLookup.GetValue(originalFormatInfo, static ofi => {
                var afi = (NumberFormatInfo)ofi.Clone();
                CopyNumberInfo(ofi, afi);
                return afi;
            });
        }

        return (originalFormatInfo, alteredFormatInfo);

        static void CopyNumberInfo(NumberFormatInfo originalFormatInfo, NumberFormatInfo alteredFormatInfo)
        {
            alteredFormatInfo.NumberDecimalDigits = originalFormatInfo.CurrencyDecimalDigits;
            alteredFormatInfo.NumberDecimalSeparator = originalFormatInfo.CurrencyDecimalSeparator;

            // Workaround for cultures where group and decimal separators are the same, as they otherwise cannot be parsed correctly.
            // Appears to happen on the "mn-Mong-MN" culture on some net9.0 + Windows 11 26100.4061 systems.
            // Better to just drop the group separator in that case rather than risk failing.

            alteredFormatInfo.NumberGroupSeparator = originalFormatInfo.CurrencyGroupSeparator != originalFormatInfo.CurrencyDecimalSeparator ?
                                                     originalFormatInfo.CurrencyGroupSeparator : string.Empty;
            alteredFormatInfo.NumberGroupSizes = originalFormatInfo.CurrencyGroupSizes;
            alteredFormatInfo.PositiveSign = originalFormatInfo.PositiveSign;
            alteredFormatInfo.NegativeSign = originalFormatInfo.NegativeSign;
        }
    }
}
