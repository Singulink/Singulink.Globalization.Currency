using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singulink.Globalization;

/// <summary>
/// Factory for creating <see cref="MoneySet"/> instances.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class MoneySetBuilder
{
    /// <summary>
    /// Factory method for creating a new empty <see cref="MoneySet"/>.
    /// </summary>
    public static MoneySet Create() => [];

    /// <summary>
    /// Factory method for creating a new empty <see cref="MoneySet"/>.
    /// </summary>
    public static MoneySet Create(ReadOnlySpan<Money> values) => new MoneySet(values);
}
