using System.ComponentModel;

namespace Singulink.Globalization.Internal;

/// <summary>
/// Factory for creating <see cref="MoneySet"/> instances. This class is not visible to intellisense and is only used only for collection expression support on
/// <see cref="IMoneySet"/> and <see cref="IReadOnlyMoneySet"/>.
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
