#if NET9_0_OR_GREATER

namespace Singulink.Globalization;

/// <content>
/// Contains .NET 9+ implementations for <see cref="ImmutableMoneyBag"/>.
/// </content>
partial class ImmutableMoneyBag
{
    // TargetDependentStringKey is ROS<char> for .NET 9+ so add overloads for strings.

    /// <inheritdoc cref="this[TargetDependentStringKey]"/>"
    public MonetaryValue this[string currencyCode] => this[currencyCode.AsSpan()];

    /// <inheritdoc cref="Add(decimal, TargetDependentStringKey)"/>
    public ImmutableMoneyBag Add(decimal amount, string currencyCode) => Add(amount, currencyCode.AsSpan());

    /// <inheritdoc cref="Contains(decimal, TargetDependentStringKey)"/>
    public bool Contains(decimal amount, string currencyCode) => Contains(amount, currencyCode.AsSpan());

    /// <inheritdoc cref="ContainsCurrency(TargetDependentStringKey)"/>
    public bool ContainsCurrency(string currencyCode) => ContainsCurrency(currencyCode.AsSpan());

    /// <inheritdoc cref="Remove(TargetDependentStringKey)"/>
    public ImmutableMoneyBag Remove(string currencyCode) => Remove(currencyCode.AsSpan());

    /// <inheritdoc cref="SetAmount(decimal, TargetDependentStringKey)"/>
    public ImmutableMoneyBag SetAmount(decimal amount, string currencyCode) => SetAmount(amount, currencyCode.AsSpan());

    /// <inheritdoc cref="Subtract(decimal, TargetDependentStringKey)"/>
    public ImmutableMoneyBag Subtract(decimal amount, string currencyCode) => Subtract(amount, currencyCode.AsSpan());

    /// <inheritdoc cref="TryGetAmount(TargetDependentStringKey, out decimal)"/>
    public bool TryGetAmount(string currencyCode, out decimal amount) => TryGetAmount(currencyCode.AsSpan(), out amount);

    /// <inheritdoc cref="TryGetValue(TargetDependentStringKey, out MonetaryValue)"/>
    public bool TryGetValue(string currencyCode, out MonetaryValue value) => TryGetValue(currencyCode.AsSpan(), out value);
}

#endif