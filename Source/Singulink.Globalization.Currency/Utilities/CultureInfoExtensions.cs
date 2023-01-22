using System;
using System.Globalization;

namespace Singulink.Globalization.Utilities;

internal static class CultureInfoExtensions
{
    /// <summary>
    /// Gets the neutral culture associated with this culture. If the given culture is already neutral it is simply returned. If there
    /// is no neutral culture in the parent chain then null is returned.
    /// </summary>
    public static CultureInfo? GetNeutralCulture(this CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        while (true)
        {
            if (culture.IsNeutralCulture)
                return culture;

            culture = culture.Parent;

            if (culture == null)
                return null;
        }
    }
}