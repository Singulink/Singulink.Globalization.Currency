<div class="article">

# Formatting

Monetary values format through the standard <xref:System.IFormattable> and <xref:System.ISpanFormattable> interfaces, so they work with string interpolation, `string.Format` and span-based APIs. This guide explains the format string grammar, how the culture influences the output, and how currencies and money bags format.

### The default format

With no format string, a value is written as its currency code and amount, using the culture's digits, group separator and decimal separator:

```csharp
var value = new MonetaryValue(1234.5m, "USD");

value.ToString();                                       // "USD 1,234.50" in en-US
value.ToString(null, CultureInfo.GetCultureInfo("de-DE")); // "1.234,50 USD"
$"Total: {value}";
```

The currency code goes before the amount in English, Irish, Latvian and Maltese cultures and after it everywhere else, which follows the conventions of those languages. The separator between code and amount is a non-breaking space, so the two never wrap onto different lines in text.

> [!NOTE]
> The default format is meant for display. It depends on the culture's number formatting, so it is not a stable round-trip format for storage. For persistence, store the amount and currency code separately.

## Format Strings

A format string has up to three parts, in this order, any of which may be omitted: a currency specifier, a number specifier and a decimals specifier. `"C"`, `"D"`, `"B1"` and `"CDB1"` are all valid.

#### Currency specifiers

| Specifier | Meaning | Example (en-US) |
| --- | --- | --- |
| `G` | General: currency code, positioned by the culture's language. This is the default. | `USD 1,234.50` |
| `I` | International: currency code before the amount. | `USD 1,234.50` |
| `R` | Reverse international: currency code after the amount. | `1,234.50 USD` |
| `C` | Currency symbol, positioned by the culture's currency pattern. | `$1,234.50` |
| `L` | Local: `C` if the currency is the local currency of the culture's region, otherwise `G`. | `$1,234.50` for USD, `CA$1,234.50` for CAD |

The symbol used by `C` is the localized symbol for the culture, so a Canadian dollar formats as `$` in `en-CA` and `CA$` in `en-US`. Placement follows the culture's <xref:System.Globalization.NumberFormatInfo.CurrencyPositivePattern> and <xref:System.Globalization.NumberFormatInfo.CurrencyNegativePattern>, so `fr-FR` produces `1 234,50 €` with the symbol after the amount. When the culture's currency symbol doubles as the decimal separator, as it does for the Cape Verdean escudo (`1 123$45`), the value is written that way.

`L` is the right choice for user interfaces that show a mix of local and foreign amounts: local amounts look familiar and foreign amounts are unambiguous. It needs a region-specific culture like `en-US` to know what "local" means; with a neutral culture it always behaves like `G`.

#### Number specifiers

| Specifier | Meaning |
| --- | --- |
| `N` | Use group separators. This is the default. |
| `D` | Digits only, no group separators. |

#### Decimals specifiers

| Specifier | Meaning |
| --- | --- |
| *(none)* | The currency's decimal digits, or more if needed to show the amount exactly. |
| `*` | No decimals if the amount is whole, otherwise the same as the default. |
| `B` | The currency's decimal digits, rounding with banker's rounding. |
| `A` | The currency's decimal digits, rounding away from zero. |
| `B4`, `A0` | A fixed number of decimal places (0 to 28) with the given rounding. |

The default never hides precision: an amount of 1.005 USD formats as `USD 1.005`, not `USD 1.00` or `USD 1.01`. Use `B` or `A` to format a rounded amount, or round the value first with <xref:Singulink.Globalization.MonetaryValue.Round*>.

```csharp
var value = new MonetaryValue(1234.5678m, "USD");
var enUS = CultureInfo.GetCultureInfo("en-US");

value.ToString("B", enUS);     // "USD 1,234.57"
value.ToString("CDA1", enUS);  // "$1234.6"
value.ToString("R*", enUS);    // "1,234.5678 USD"
new MonetaryValue(1000m, "JPY").ToString("*", enUS); // "JPY 1,000"
new MonetaryValue(1000m, "USD").ToString("*", enUS); // "USD 1,000"
```

## Negative Amounts

Negative amounts follow the culture's <xref:System.Globalization.NumberFormatInfo.CurrencyNegativePattern>, translated to the chosen currency specifier. In `en-US` the pattern is parentheses, so `-1234.5` USD formats as `USD (1,234.50)` by default and `($1,234.50)` with `C`. Cultures that use a leading minus sign produce `-1.234,50 USD` and similar.

## Span Formatting

<xref:Singulink.Globalization.MonetaryValue.TryFormat*> writes into a caller-supplied span without allocating. A buffer of 96 characters is enough for any value in any culture. Interpolated string handlers on .NET use it automatically, so `$"{value}"` inside a `DefaultInterpolatedStringHandler` does not allocate an intermediate string.

## Formatting Currencies

<xref:Singulink.Globalization.Currency.ToString*> on a currency returns its localized name. The `"L"` format (the default) appends the code, and `"S"` returns just the name:

```csharp
var usd = Currency.GetCurrency("USD");

usd.ToString();                                  // "US Dollar (USD)"
usd.ToString("S", CultureInfo.GetCultureInfo("fr")); // "dollar des États-Unis"
```

## Formatting Money Bags

Bags format as their values separated by commas, each with the same format string, and accept a `!` prefix to leave out zero amounts:

```csharp
var bag = new MoneyBag { new(100m, "USD"), new(0m, "EUR"), new(50m, "CAD") };

bag.ToString();          // "CAD 50.00, EUR 0.00, USD 100.00"
bag.ToString("!C");      // "CA$50.00, $100.00" in en-US
```

## Next Steps

Continue with these related articles:

- [Parsing](parsing.md) - Reading values back from strings.
- [Localization](../concepts/localization.md) - Where the localized symbols come from.
- [Money Bags](money-bags.md) - The bag types and their operations.

</div>
