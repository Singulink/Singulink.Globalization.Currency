<div class="article">

# Formatting

Monetary values format through the standard <xref:System.IFormattable> and <xref:System.ISpanFormattable> interfaces, so they work with string interpolation, `string.Format` and span-based APIs. This guide explains the format string grammar, how the culture influences the output, and how currencies and money bags format.

### Default format

With no format string, a value is written as its currency code and amount, using the culture's digits, group separator and decimal separator:

```csharp
var value = new MonetaryValue(1234.5m, "USD");

value.ToString();                                       // "USD 1,234.50" in en-US
value.ToString(null, CultureInfo.GetCultureInfo("de-DE")); // "1.234,50 USD"
$"Total: {value}";
```

The currency code goes before the amount in English, Irish and Maltese cultures and in the invariant culture, and after it everywhere else. This is the placement rule of the EU's [Interinstitutional Style Guide](https://style-guide.europa.eu/en/content/-/isg/topic?identifier=7.3.3-rules-for-expressing-monetary-units) for the languages of the EU, and the library applies it to every other language as well. The separator between code and amount is a non-breaking space, so the two never wrap onto different lines in text.

> [!NOTE]
> The default format is meant for display. It depends on the culture's number formatting, so text written with it can read back as a different amount. For text that a program reads again, use the [round-trip format](#round-trip-text).

## Format Strings

A format string has up to three parts, in this order: a currency specifier, a number specifier and a decimals specifier. Any part may be left out, so `"C"`, `"D"`, `"B1"` and `"CDB1"` are all valid. The round-trip specifier `O` stands alone.

#### Currency specifiers

| Specifier | Meaning | Example (en-US) |
| --- | --- | --- |
| `G` | General: currency code, positioned by the culture's language. This is the default. | `USD 1,234.50` |
| `I` | International: currency code before the amount. | `USD 1,234.50` |
| `R` | Reverse international: currency code after the amount. | `1,234.50 USD` |
| `C` | Currency symbol, positioned by the culture's currency pattern. | `$1,234.50` |
| `L` | Local: `C` if the currency is the local currency of the culture's region, otherwise `G`. | `$1,234.50` for USD, `CAD 1,234.50` for CAD |
| `V` | Value only: the amount with no code and no symbol. See [Amounts Without a Currency](#amounts-without-a-currency). | `1,234.50` |
| `O` | Round-trip: currency code and amount, in the same form in every culture. See [Round-Trip Text](#round-trip-text). | `USD 1234.50` |

The symbol used by `C` is the localized symbol for the culture, so with CLDR data a Canadian dollar formats as `$` in `en-CA` and `CA$` in `en-US`. Placement follows the culture's <xref:System.Globalization.NumberFormatInfo.CurrencyPositivePattern> and <xref:System.Globalization.NumberFormatInfo.CurrencyNegativePattern>, so `fr-FR` produces `1 234,50 €` with the symbol after the amount. If the culture's currency symbol doubles as the decimal separator, the value is written that way. The Cape Verdean escudo is an example: `1 123$45`.

When the culture's pattern puts the symbol directly against the number and the symbol's character next to the number is not a symbol character, such as a letter, `C` writes a no-break space between them. This follows the currency spacing rule of the Unicode CLDR:

```csharp
// With CLDR data, whose en-AU symbol for CAD is "CAD":
var enAU = CultureInfo.GetCultureInfo("en-AU");
var enUS = CultureInfo.GetCultureInfo("en-US");

new MonetaryValue(1234.5m, "CAD").ToString("C", enAU);    // "CAD 1,234.50"
new MonetaryValue(-1234.5m, "CAD").ToString("C", enAU);   // "-CAD 1,234.50"
new MonetaryValue(1234.5m, "CAD").ToString("C", enUS);    // "CA$1,234.50", the "$" is next to the number
```

The rule goes by the symbol's text, so it applies with any currency data. A pattern that already has a space there, or a sign between the symbol and the number, is left as it is.

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

## Amounts Without a Currency

The `V` specifier writes the amount alone, as the culture writes amounts of money. Use it where the currency is shown once for many amounts, such as the lines of an invoice whose total carries the symbol:

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
var line = new MonetaryValue(1234.5m, "USD");
var total = new MonetaryValue(5678.9m, "USD");

line.ToString("V", enUS);                               // "1,234.50"
total.ToString("C", enUS);                              // "$5,678.90"

new MonetaryValue(1234m, "JPY").ToString("V", enUS);    // "1,234", the yen has no decimals
new MonetaryValue(-1234.5m, "USD").ToString("V", enUS); // "-1,234.50"
line.ToString("VD", enUS);                              // "1234.50"
```

`V` differs from formatting <xref:Singulink.Globalization.MonetaryValue.Amount> as a number in three ways:

- The decimals come from the currency, not from the culture.
- The separators and the form of a negative amount come from the culture's currency format, which in some cultures differs from its number format.
- `V` combines with the number and decimals specifiers, as every other currency specifier does.

> [!NOTE]
> Text written with `V` has no currency, so it does not parse back into a monetary value. Read it as a `decimal` and create the value with the currency that you know it has.

## Negative Amounts

Negative amounts follow the culture's <xref:System.Globalization.NumberFormatInfo.CurrencyNegativePattern>, translated to the chosen currency specifier:

```csharp
var refund = new MonetaryValue(-1234.5m, "USD");
var deDE = CultureInfo.GetCultureInfo("de-DE");

refund.ToString(null, enUS);   // "USD -1,234.50", en-US uses a leading minus sign on .NET with ICU data
refund.ToString("C", enUS);    // "-$1,234.50"
refund.ToString(null, deDE);   // "-1.234,50 USD"
```

A culture whose pattern uses parentheses produces `USD (1,234.50)` by default, `($1,234.50)` with `C` and `(1,234.50)` with `V`.

## Default Value

<xref:Singulink.Globalization.MonetaryValue.Default> has no currency, so it has no code, no symbol and no decimals of its own. It formats as a plain zero, unless the format gives a fixed number of decimals:

```csharp
var enUS = CultureInfo.GetCultureInfo("en-US");
var deDE = CultureInfo.GetCultureInfo("de-DE");

MonetaryValue.Default.ToString(null, enUS);   // "0"
MonetaryValue.Default.ToString("C", enUS);    // "0"
MonetaryValue.Default.ToString("B2", enUS);   // "0.00"
MonetaryValue.Default.ToString("B2", deDE);   // "0,00"
```

`B` and `A` without a number use the currency's decimals, and the default value has none, so they write `0` too. Each of these strings parses back as the default value when you read it with the same culture.

## Round-Trip Text

The `O` format writes text that reads back as an equal value. It ignores the current culture and any provider that you pass:

```csharp
var price = new MonetaryValue(12.345m, "USD");

string text = price.ToString("O");                                       // "USD 12.345", in every culture
var restored = MonetaryValue.Parse(text, CultureInfo.InvariantCulture);  // USD 12.345, equal to price
```

The text is the currency code, an ordinary space and the amount in the invariant culture, with no group separators and a minus sign before a negative amount. The number of decimals is the currency's, or more when the amount needs them, so equal values always write the same text:

| Value | Text |
| --- | --- |
| `new MonetaryValue(12.3m, "USD")` | `USD 12.30` |
| `new MonetaryValue(-0.5m, "USD")` | `USD -0.50` |
| `new MonetaryValue(1250m, "JPY")` | `JPY 1250` |
| <xref:Singulink.Globalization.MonetaryValue.Default> | `0` |

Read the text with <xref:System.Globalization.CultureInfo.InvariantCulture> as the provider. Another culture can read it differently: with `de-DE`, the `.` is a group separator, so `USD 12.30` reads as `USD 1230`. A zero without a currency, such as the `0` that the default value writes, reads as the default value in every culture.

Use this format for logs, configuration files and any other text that a program reads again. For databases and JSON, see [Storing Monetary Values](storing-values.md).

`O` cannot be combined with a number or decimals specifier, so `"O2"` throws <xref:System.FormatException>. A registry that has a currency code with a digit or whitespace in it [cannot parse monetary strings](parsing.md#registries-that-cannot-parse), so it cannot read this text back either.

## Span Formatting

<xref:Singulink.Globalization.MonetaryValue.TryFormat*> writes into a caller-supplied span without allocating. A buffer of 96 characters is enough for any value in any culture. Interpolated string handlers on .NET use it automatically, so `$"{value}"` inside a `DefaultInterpolatedStringHandler` does not allocate an intermediate string.

On .NET, the value also implements <xref:System.IUtf8SpanFormattable>, so `Utf8.TryWrite` and other UTF-8 writers write its text as UTF-8 bytes without a string in between. The bytes are the same text that <xref:Singulink.Globalization.MonetaryValue.TryFormat*> writes into a `char` span, encoded as UTF-8.

## Formatting Currencies

<xref:Singulink.Globalization.Currency.ToString*> on a currency returns its localized name. The `"L"` format (the default) appends the code, and `"S"` returns just the name:

```csharp
var usd = Currency.GetCurrency("USD");

usd.ToString();                                  // "US Dollar (USD)"
usd.ToString("S", CultureInfo.GetCultureInfo("fr")); // "dollar des États-Unis"
```

## Formatting Money Bags

A bag formats as its values separated by commas, each formatted with the same format string. A `!` prefix leaves out zero amounts:

```csharp
var bag = new SortedMoneyBag { new(100m, "USD"), new(0m, "EUR"), new(50m, "CAD") };

bag.ToString();          // "CAD 50.00, EUR 0.00, USD 100.00"
bag.ToString("!C");      // "CA$50.00, $100.00" in en-US with CLDR data
bag.ToString("O");       // "CAD 50.00, EUR 0.00, USD 100.00", each value in the round-trip form
```

The library has no parser for a bag's text, so store a bag as its values; see [Storing Monetary Values](storing-values.md#money-bags-in-json).

## Next Steps

Continue with these related articles:

- [Parsing](parsing.md) - Reading values back from strings.
- [How Names and Symbols Are Localized](../concepts/localization.md) - Where the localized symbols come from.
- [Money Bags](money-bags.md) - The bag types and their operations.

</div>
