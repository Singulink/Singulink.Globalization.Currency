#if NET9_0_OR_GREATER

namespace Singulink.Globalization;

/// <content>
/// Contains .NET 9+ implementations for CurrencyRegistry.
/// </content>
partial class CurrencyRegistry
{
    // TargetDependentStringKey is ROS<char> for .NET 9+ so add overloads for strings.

    /// <inheritdoc cref="this[TargetDependentStringKey]"/>
    public Currency this[string currencyCode] => this[currencyCode.AsSpan()];

    /// <inheritdoc cref="Contains(TargetDependentStringKey)"/>
    public bool Contains(string currencyCode) => Contains(currencyCode.AsSpan());

    /// <inheritdoc cref="TryGetCurrency(TargetDependentStringKey, out Currency)"/>
    public bool TryGetCurrency(string currencyCode, [MaybeNullWhen(false)] out Currency currency) => TryGetCurrency(currencyCode.AsSpan(), out currency);

    /// <inheritdoc cref="TryGetCurrenciesBySymbol(TargetDependentStringKey, out IReadOnlyList{Currency})"/>
    public bool TryGetCurrenciesBySymbol(string currencySymbol, out IReadOnlyList<Currency> currencies)
        => TryGetCurrenciesBySymbol(currencySymbol.AsSpan(), out currencies);

    /// <inheritdoc cref="TryGetCurrenciesBySymbol(TargetDependentStringKey, CultureInfo?, out IReadOnlyList{Currency})"/>
    public bool TryGetCurrenciesBySymbol(string currencySymbol, CultureInfo? culture, out IReadOnlyList<Currency> currencies)
        => TryGetCurrenciesBySymbol(currencySymbol.AsSpan(), culture, out currencies);
}

#endif