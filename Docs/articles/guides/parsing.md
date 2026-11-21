<div class="article">

# Parsing

Parsing turns text like `USD 1,234.50` or `$1,234.50` back into a <xref:Singulink.Globalization.MonetaryValue>. This guide covers the parsing methods, the <xref:Singulink.Globalization.MonetaryStyles> flags that control what is accepted, and how the currency is identified from a code or symbol.

### Where parsing happens

Every parse needs a registry to resolve the currency. <xref:Singulink.Globalization.MonetaryValue.Parse*> and <xref:Singulink.Globalization.MonetaryValue.TryParse*> use <xref:Singulink.Globalization.CurrencyRegistry.Default>. <xref:Singulink.Globalization.CurrencyRegistry.ParseMonetaryValue*> and <xref:Singulink.Globalization.CurrencyRegistry.TryParseMonetaryValue*> parse against a specific registry. The registry overloads can also return the reason a parse failed, which is useful for validation messages.

## Parsing Methods

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
string input = "USD 1,234.50";   // text from the user

var a = MonetaryValue.Parse("USD 1,234.50", provider: enUS);
var b = MonetaryValue.Parse("$1,234.50", MonetaryStyles.LocalSymbol, enUS);

if (MonetaryValue.TryParse(input, MonetaryStyles.Any, enUS, out var value))
{
    // ...
}

// registry: any CurrencyRegistry
if (!registry.TryParseMonetaryValue(input, MonetaryStyles.Any, enUS, out value, out string error))
{
    ShowValidationError(error);
}
```

All methods accept a `string` or a <xref:System.ReadOnlySpan`1> of characters. The provider supplies the number format, and for symbol parsing also the culture and region. Passing `null` uses the current culture. On .NET the type also implements <xref:System.IParsable`1> and <xref:System.ISpanParsable`1>, so it works with generic parsing code.

## Styles

<xref:Singulink.Globalization.MonetaryStyles> combines the familiar number flags from <xref:System.Globalization.NumberStyles>, such as <xref:Singulink.Globalization.MonetaryStyles.AllowThousands> and <xref:Singulink.Globalization.MonetaryStyles.AllowParentheses>, with three flags that control how the currency is identified. At least one of the three must be set:

| Flag | Accepts | Depends on the culture |
| --- | --- | --- |
| <xref:Singulink.Globalization.MonetaryStyles.AllowCurrencyCode> | A currency code from the registry, such as `USD` | No |
| <xref:Singulink.Globalization.MonetaryStyles.AllowLocalSymbol> | The local currency symbol of the culture's region | Yes, and it needs a region-specific culture |
| <xref:Singulink.Globalization.MonetaryStyles.AllowUnambiguousSymbols> | Any symbol in the registry that maps to exactly one currency for the culture, such as `€` | Yes |

Codes are unambiguous and culture-independent, which makes <xref:Singulink.Globalization.MonetaryStyles.AllowCurrencyCode> the safest choice for data exchange. The other two read the same symbol differently in different cultures:

```csharp
var enCA = CultureInfo.GetCultureInfo("en-CA");

MonetaryValue.Parse("$5", MonetaryStyles.LocalSymbol, enCA);         // CAD 5
MonetaryValue.Parse("$5", MonetaryStyles.LocalSymbol, enUS);         // USD 5
MonetaryValue.Parse("€5", MonetaryStyles.UnambiguousSymbols, enUS);  // EUR 5
MonetaryValue.Parse("$5", MonetaryStyles.UnambiguousSymbols, enUS);  // throws FormatException with system data, where many
                                                                     // currencies use "$"; USD 5 with CLDR data
```

Composite values bundle each currency flag with all the number flags: <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> (the default), <xref:Singulink.Globalization.MonetaryStyles.LocalSymbol>, <xref:Singulink.Globalization.MonetaryStyles.UnambiguousSymbols>, and combinations such as <xref:Singulink.Globalization.MonetaryStyles.CurrencyCodeOrLocalSymbol> and <xref:Singulink.Globalization.MonetaryStyles.Any>.

When several currency flags are set, the text is matched as a code first, then as the local symbol, then as an unambiguous symbol. A code is unique in the registry and does not depend on the culture, so `USD` always parses as US dollars, even in a culture whose local symbol is spelled like a code. For the same reason, text written with the default format reads back as the same currency in every culture.

> [!TIP]
> For user input, <xref:Singulink.Globalization.MonetaryStyles.Any> with the user's culture gives the most forgiving behavior. For machine-generated data, stick to <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> and a fixed culture.

## What the Parser Accepts

The currency indicator can appear before or after the number, with or without a space, following the same patterns that formatting produces. The space can be an ordinary space or the no-break space that formatting writes. Each of these is allowed by its own flag, exactly as with <xref:System.Decimal.Parse*>: whitespace, signs, parentheses for negatives, group separators and the decimal point.

With <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> in `en-US`:

```csharp
"USD 1,234.50"     // USD 1234.50
"1,234.50 USD"     // USD 1234.50, the code may follow the amount
"USD1,234.50"      // USD 1234.50, the space is optional
"usd 5"            // USD 5, codes are matched case-insensitively
"USD -1,234.50"    // USD -1234.50
"USD (1,234.50)"   // USD -1234.50, parentheses mean negative
"USD 1,234.567"    // USD 1234.567, parsing does not round
"1234.50"          // fails: no currency code
"0"                // MonetaryValue.Default: a zero without a currency is the default value
"$1,234.50"        // fails: "$" is not a code
"USD 1 234.50"     // fails: not the culture's number format
```

Anything the default format can produce for a culture parses back with <xref:Singulink.Globalization.MonetaryStyles.CurrencyCode> and the same culture. Anything the `C` format produces parses back with <xref:Singulink.Globalization.MonetaryStyles.LocalSymbol> or <xref:Singulink.Globalization.MonetaryStyles.UnambiguousSymbols>, as appropriate.

Parsing does not round. The parsed value carries whatever precision the text had.

## Registries That Cannot Parse

To match a code or symbol, the parser has to separate it from the number. Codes and symbols are not validated when a currency is created, so that unusual currencies can still be represented. But a registry with a code that breaks these rules cannot parse codes, and a registry with such a symbol cannot parse symbols:

| A code or symbol must not | Example |
| --- | --- |
| Be empty | `""` |
| Contain a digit | `PT1` |
| Contain whitespace, other than the narrow no-break space (U+202F) | `P T` |
| Contain `+`, `-`, `−` (the minus sign), `(` or `)` | `P-T` |

The parse methods throw an <xref:System.InvalidOperationException> whose message says which kind of parsing the registry cannot do:

```csharp
var registry = new CurrencyRegistry("Points", [new Currency("PT1", "Points")]);

registry.ParseMonetaryValue("PT1 5");   // throws InvalidOperationException: "This currency registry cannot parse currency
                                        // codes because one of the currency codes violates the following requirement:
                                        // Symbol/code cannot contain any numbers or whitespace."
registry["PT1"];                        // fine: lookups and formatting work normally
```

This only affects parsing; such registries format and look up currencies normally. Symbols are localized, so the library checks them separately for each culture.

When users supply codes or symbols, check them with <xref:Singulink.Globalization.Currency.IsSymbolOrCodeParsable*>:

```csharp
string code = "PT1";   // a code entered by a user

if (!Currency.IsSymbolOrCodeParsable(code, out string? error))
    ShowValidationError(error);   // here "Symbol/code cannot contain any numbers or whitespace."
```

## Next Steps

Continue with these related articles:

- [Formatting](formatting.md) - The formats that parsing mirrors.
- [Currencies and Registries](currencies-and-registries.md) - Custom currencies and custom registries.
- [How Names and Symbols Are Localized](../concepts/localization.md) - How the local and unambiguous symbols are determined for a culture.

</div>
