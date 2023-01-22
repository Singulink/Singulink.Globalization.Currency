using System;
using System.Collections.Generic;
using System.Text;

namespace Singulink.Globalization.Utilities;

internal static class Throw
{
    [DoesNotReturn]
    public static void FormatEx() => throw new FormatException("Format specifier was invalid.");
}