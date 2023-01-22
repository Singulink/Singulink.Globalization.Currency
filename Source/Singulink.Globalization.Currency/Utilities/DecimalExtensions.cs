using System;
using System.Collections.Generic;
using System.Text;

namespace Singulink.Globalization.Utilities;

internal static class DecimalExtensions
{
    public static int GetDecimalDigits(this decimal value)
    {
        int decimalPlaces = 0;

        while (Math.Truncate(value) != value)
        {
            value *= 10;
            decimalPlaces++;
        }

        return decimalPlaces;
    }
}