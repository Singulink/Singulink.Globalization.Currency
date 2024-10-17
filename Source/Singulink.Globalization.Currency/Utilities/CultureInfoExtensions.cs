using System.Runtime.CompilerServices;

namespace Singulink.Globalization.Utilities;

internal static class CultureInfoExtensions
{
#pragma warning disable IDE0028 // Simplify collection initialization (not supported on NS2)
    private static readonly ConditionalWeakTable<CultureInfo, RegionInfo?> _regionInfoLookup = new();
#pragma warning restore IDE0028

    public static RegionInfo? GetRegionInfo(this CultureInfo culture)
    {
        RegionInfo region = null;

        if ((culture.CultureTypes & CultureTypes.SpecificCultures) is not 0 && !_regionInfoLookup.TryGetValue(culture, out region))
        {
            try
            {
                region = new RegionInfo(culture.Name);
#if NETSTANDARD
                _regionInfoLookup.Add(culture, region);
#endif
            }
            catch { }

#if NET
            _regionInfoLookup.AddOrUpdate(culture, region);
#endif
        }

        return region;
    }
}