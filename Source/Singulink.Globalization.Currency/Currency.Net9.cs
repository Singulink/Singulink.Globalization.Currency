#if NET9_0_OR_GREATER

namespace Singulink.Globalization;

/// <content>
/// Contains .NET 9+ implementations for Currency.
/// </content>
partial class Currency
{
    // TargetDependentStringKey is ROS<char> for .NET 9+ so add overloads for strings.

    /// <inheritdoc cref="GetCurrency(TargetDependentStringKey)"/>
    public static Currency Get(string currencyCode) => CurrencyRegistry.Default[currencyCode.AsSpan()];

    /// <inheritdoc cref="TryGetCurrency(TargetDependentStringKey, out Currency)"/>
    public static bool TryGet(string currencyCode, [MaybeNullWhen(false)] out Currency currency)
        => CurrencyRegistry.Default.TryGetCurrency(currencyCode.AsSpan(), out currency);
}

#endif