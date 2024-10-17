using System.ComponentModel;

namespace Singulink.Globalization.CompilerServices;

/// <summary>
/// Factory for creating <see cref="MoneyBag"/> instances. This class is not visible to intellisense and is only used only for collection expression support on
/// <see cref="IMoneyBag"/> and <see cref="IReadOnlyMoneyBag"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class MoneyBagBuilder
{
    /// <summary>
    /// Factory method for creating a new empty <see cref="MoneyBag"/>.
    /// </summary>
    public static MoneyBag Create() => [];

    /// <summary>
    /// Factory method for creating a new <see cref="MoneyBag"/> with the specified values.
    /// </summary>
    public static MoneyBag Create(ReadOnlySpan<MonetaryValue> values) => new MoneyBag(values);
}
