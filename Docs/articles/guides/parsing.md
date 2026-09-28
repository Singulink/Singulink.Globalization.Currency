<div class="article">

# Parsing

Parsing turns text like `USD 1,234.50` or `$1,234.50` back into a <xref:Singulink.Globalization.MonetaryValue>. This guide covers the parsing methods, the <xref:Singulink.Globalization.MonetaryStyles> flags that control what is accepted, and how the currency is identified from a code or symbol.

### Where parsing happens

Every parse needs a registry to resolve the currency. <xref:Singulink.Globalization.MonetaryValue.Parse*> and <xref:Singulink.Globalization.MonetaryValue.TryParse*> use <xref:Singulink.Globalization.CurrencyRegistry.Default>, and <xref:Singulink.Globalization.CurrencyRegistry.ParseMonetaryValue*> and <xref:Singulink.Globalization.CurrencyRegistry.TryParseMonetaryValue*> parse against a specific registry. The registry overloads can also return the reason a parse failed, which is useful for validation messages.

## Parsing Methods

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");

var a = MonetaryValue.Parse("USD 1,234.50", provider: enUS);
var b = MonetaryValue.Parse("$1,234.50", MonetaryStyles.LocalSymbol, enUS);

if (MonetaryValue.TryParse(input, MonetaryStyles.Any, enUS, out var value))
{
    // ...
}

if (!registry.TryParseMonetaryValue(input, MonetaryStyles.Any, enUS, out value, out string error))
{
    ShowValidationError(error);
}
```

All methods accept a `string` or a <xref:System.ReadOnlySpan`1> of characters. The provider supplies the number format and, for symbol parsing, the culture and region; passing `null` uses the current culture. On .NET the type also implements <xref:System.IParsable`1> and <xref:System.ISpanParsable`1>, so it works with generic parsing code.

## Styles

<xref:Singulink.Globalization.MonetaryStyles> combines the familiar number flags from <xref:System.Globalization.NumberStyles>, such as <xref:Singulink.Globalization.MonetaryStyles.AllowThousands> and <xref:Singulink.Globalization.MonetaryStyles.AllowParentheses>, with three flags that control how the currency is identified. At least one of the three must be set:

- <xref:Singulink.Globalization.MonetaryStyles.AllowCurrencyCode> accepts a currency code from the registry, such as `USD`. Codes are unambiguous and culture-independent, which makes this the safest choice for data exchange.
- <xref:Singulink.Globalization.MonetaryStyles.AllowLocalSymbol> accepts the local currency symbol of the culture's region, so `$` means Canadian dollars when parsing with `en-CA` and US dollars with `en-US`. It requires a region-specific culture.
- <xref:Singulink.Globalization.MonetaryStyles.AllowUnambiguousSymbols> accepts any symbol in the registry that maps to exactly one currency for the culture, such as `€` or `£`. A symbol shared by several currencies, like `$` in most cultures, is rejected under this flag alone.

Composite values bundle each currency flag with all the number flags: <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> (the default), <xref:Singulink.Globalization.MonetaryStyles.LocalSymbol>, <xref:Singulink.Globalization.MonetaryStyles.UnambiguousSymbols>, and combinations such as <xref:Singulink.Globalization.MonetaryStyles.CurrencyCodeOrLocalSymbol> and <xref:Singulink.Globalization.MonetaryStyles.Any>.

When several currency flags are set, the text is matched as a code first, then as the local symbol, then as an unambiguous symbol. This order means `USD` always parses as US dollars even in a culture whose local symbol happens to look like a code.

> [!TIP]
> For user input, <xref:Singulink.Globalization.MonetaryStyles.Any> with the user's culture gives the most forgiving behavior. For machine-generated data, stick to <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> and a fixed culture.

## What the Parser Accepts

The currency indicator can appear before or after the number, with or without a space, following the same patterns that formatting produces. Whitespace, signs, parentheses for negatives, group separators and the decimal point are each governed by their flag, exactly as with <xref:System.Decimal.Parse*>. Anything the default format can produce for a culture parses back with <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> and the same culture, and anything the `C` format produces parses back with <xref:Singulink.Globalization.MonetaryStyles.LocalSymbol> or <xref:Singulink.Globalization.MonetaryStyles.UnambiguousSymbols>, as appropriate.

Parsing does not round. The parsed value carries whatever precision the text had.

## Registries That Cannot Parse

Symbol and code matching relies on being able to separate the indicator from the number. A registry that contains a currency whose code or symbol includes digits, whitespace, signs or parentheses cannot support the corresponding kind of parsing, and the parse methods throw <xref:System.InvalidOperationException> that names the offending requirement. This only affects parsing; such registries format and look up currencies normally. Check codes and symbols with <xref:Singulink.Globalization.Currency.IsSymbolOrCodeParsable*> when accepting them from users. Symbol parsability is evaluated per culture, since symbols are localized.

## Next Steps

Continue with these related articles:

- [Formatting](formatting.md) - The formats that parsing mirrors.
- [Currencies and Registries](currencies-and-registries.md) - Custom currencies and the parsability requirements.
- [Localization](../concepts/localization.md) - How the local and unambiguous symbols are determined for a culture.

</div>
