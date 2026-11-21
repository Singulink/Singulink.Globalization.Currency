<div class="article">

# Why Did This Throw?

The library throws instead of guessing whenever money could come out wrong. Find the exception or the message in front of you below: each section shows the code that throws, why, and the fix, and links to the page that has the full rule.

## Cannot Mix Currencies

```csharp
var dollars = new MonetaryValue(10m, "USD");
var euros = new MonetaryValue(10m, "EUR");

var sum = dollars + euros;      // ArgumentException: "Currencies must match to add or subtract monetary values
                                // (or one of the values can be a default value that has no currency associated with it)."
var ratio = dollars / euros;    // ArgumentException: "Currencies must match to divide monetary values."
```

Adding, subtracting or dividing values in two currencies has no right answer without an exchange rate, and the library has no rates. Convert one value first, as shown in [Converting Between Currencies](monetary-values.md#converting-between-currencies), or keep the values apart in a [money bag](money-bags.md), which holds one value per currency.

## Comparing Values in Different Currencies

```csharp
dollars < euros;                                    // ArgumentException: "Currencies must match in order to compare
                                                    // monetary values."
MonetaryValue.Max(dollars, euros);                  // the same
new[] { dollars, euros }.OrderBy(v => v).ToList();  // InvalidOperationException, with the ArgumentException inside

dollars == euros;                                   // false, never throws
```

The relational operators, <xref:Singulink.Globalization.MonetaryValue.CompareTo*>, <xref:Singulink.Globalization.MonetaryValue.Min*> and <xref:Singulink.Globalization.MonetaryValue.Max*> need one currency, and so does anything that sorts by them. Equality never throws. Group by currency before sorting; see [Comparison and Equality](monetary-values.md#comparison-and-equality).

## A Value With No Currency

```csharp
var total = MonetaryValue.Default;

total.Currency;                // InvalidOperationException: "Default monetary values do not have a currency associated with them."
var more = total + 5m;         // ArgumentException: "Non-zero amount monetary values must have a currency associated with them."
```

<xref:Singulink.Globalization.MonetaryValue.Default> is a zero with no currency. Read <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> or check <xref:Singulink.Globalization.MonetaryValue.IsDefault> instead of <xref:Singulink.Globalization.MonetaryValue.Currency>, and add monetary values to it rather than plain amounts. See [Default Value](monetary-values.md#default-value).

## Currency Not in the Registry

```csharp
Currency.GetCurrency("XAU");             // ArgumentException: "Currency code 'XAU' was not found in the 'System' currency registry."
new MonetaryValue(1m, "XAU");            // the same

var storeRegistry = new CurrencyRegistry("Store", [Currency.GetCurrency("USD")]);
var bag = new MoneyBag(storeRegistry);
bag.Add(new MonetaryValue(10m, "EUR"));  // ArgumentException: "The currency 'Euro (EUR)' is not present in the bag's
                                         // currency registry."
```

The library looked the code up in the default registry, or in the money bag's own registry, and the currency is not there. The fixes:

| Cause | Fix |
| --- | --- |
| The code comes from input or stored data and may be unknown | Use <xref:Singulink.Globalization.Currency.TryGetCurrency*> and handle `false` |
| The currency is not legal tender, such as `XAU`, or is historical, such as `DEM` | With CLDR data, pass <xref:Singulink.Globalization.CurrencyTypes> to <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*>; see [CLDR Currency Data](cldr-data.md#registering-the-data) |
| The system data does not have the currency | Use the [CLDR package](cldr-data.md) or a [custom currency](currencies-and-registries.md#custom-currencies) |
| A money bag is bound to a smaller registry | Add only currencies of that registry; see [Custom Registries](currencies-and-registries.md#custom-registries) |

## Every Currency Code Is Missing

```csharp
new MonetaryValue(10m, "USD");          // ArgumentException: "Currency code 'USD' was not found in the 'System' currency registry.
                                        // The system provides no currency data in invariant globalization mode. Install the
                                        // Singulink.Globalization.Currency.Cldr package and register its data at startup with
                                        // CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider)."
CurrencyRegistry.Default.Count;         // 0
```

The app runs with invariant globalization, as many container images do, so the runtime has no region data and the system registry is empty. Register the CLDR data provider at startup:

```csharp
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);   // the first line of startup
```

Parsing, the JSON converters and the type converters give the same explanation. See [Invariant Globalization Mode](cldr-data.md#invariant-globalization-mode).

## Currency Code Not Found in JSON

```csharp
JsonSerializer.Deserialize<MonetaryValue>("""{"amount":10,"currency":"XAU"}""");
// JsonException: "Currency code 'XAU' was not found in the 'System' currency registry."

JsonSerializer.Deserialize<SortedMoneyBag>("""[{"amount":10,"currency":"XAU"}]""");
// the same, for a value in a money bag
```

The JSON converters look each code up in their registry. That is <xref:Singulink.Globalization.CurrencyRegistry.Default>, unless you added a <xref:Singulink.Globalization.MonetaryValueJsonConverter>, <xref:Singulink.Globalization.CurrencyJsonConverter> or <xref:Singulink.Globalization.MoneyBagJsonConverterFactory> for another registry to the options. Money bags use the factory's registry, not the value converter's. The exception's <xref:System.Text.Json.JsonException.Path> tells you which value or bag had the code.

To fix it, do one of these:

- Read with the registry that has the code.
- Include the currency in the default registry, as in the table above.
- Catch the exception, if the data may hold codes that you do not support.

See [Codes the Registry Does Not Have](storing-values.md#codes-the-registry-does-not-have) and [Money Bags in JSON](storing-values.md#money-bags-in-json).

## Amount Has Too Many Digits for a JSON Number

```csharp
JsonSerializer.Serialize(new MonetaryValue(10m, "USD") / 3);
// ArgumentException: "The amount of USD 3.3333333333333333333333333333 has 29 significant digits, but a JSON reader that uses
// double-precision numbers, such as JavaScript, keeps only 15, so it would read a different amount. ..."
```

The object form writes the amount as a JSON number, and a reader such as JavaScript would read a different number without an error.

To fix it, do one of these:

- Round the value before you write it: to the currency with `Round()`, or to a number of decimals with `Round(4)`.
- Write amounts as strings with `JsonNumberHandling.WriteAsString`.
- Use the text form.
- If every reader keeps all the digits of a number, use <xref:Singulink.Globalization.MonetaryJsonFormat.LossyNumberObject?displayProperty=nameWithType>.

See [Amounts With Many Digits](storing-values.md#amounts-with-many-digits).

## JSON String Is Not a Monetary Value

```csharp
JsonSerializer.Deserialize<MonetaryValue>("\"$12.30\"");
// JsonException: "The string '$12.30' is not a monetary value in the round-trip format, such as "USD 12.34"."
```

The JSON converters read a string only in the round-trip format, such as `"USD 12.30"`. Text formatted for a culture can read back as a different amount or currency, so the converters do not guess. Write the value with the round-trip format or the object form; see [Object or Text](storing-values.md#object-or-text).

## Text That Does Not Parse

```csharp
MonetaryValue.Parse("10");                        // FormatException: "The input string '10' does not contain a currency code."
MonetaryValue.Parse("$10");                       // FormatException: "... could not be resolved. The following lookups were
                                                  // attempted but failed: Currency code: CodeNotFound"
```

The default style accepts currency codes only. Pass a <xref:Singulink.Globalization.MonetaryStyles> value that accepts symbols, use <xref:Singulink.Globalization.MonetaryValue.TryParse*> for input that may be invalid, or use <xref:Singulink.Globalization.CurrencyRegistry.TryParseMonetaryValue*> to get the reason as a string. See [Parsing](parsing.md#styles).

## No Cash Rounding Rules

```csharp
new MonetaryValue(10.03m, "CAD").RoundToCash();   // NotSupportedException with system data
```

The message says what to do: "Cash rounding rules are not available for currency 'CAD'. Cash rounding information is not included in system globalization data. Install the Singulink.Globalization.Currency.Cldr package and register its data provider as the default currency registry, or initialize the CashRoundingPolicy property when creating custom currencies."

<xref:Singulink.Globalization.MonetaryValue.RoundToCash*>, <xref:Singulink.Globalization.MonetaryValue.AllocateToCash*> and a money bag's <xref:Singulink.Globalization.IMoneyBag.RoundToCash*> throw this exception. See [Cash rounding](rounding-and-allocation.md#cash-rounding) and [CLDR Currency Data](cldr-data.md).

## The Default Registry Can Only Be Set Once

```csharp
var price = new MonetaryValue(10m, "USD");                       // creates the default registry from system data
CurrencyRegistry.SetDefaultProvider(CldrCurrencyData.Provider);  // InvalidOperationException: "The default currency
                                                                 // registry has already been created and can no longer be changed."
```

Something used the default registry before the call. Move <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*> or <xref:Singulink.Globalization.CurrencyRegistry.SetDefault*> to the first line of startup. See [Set once, before first use](currencies-and-registries.md#set-once-before-first-use).

> [!TIP]
> Look for static field initializers that create monetary values from codes. In a test project, also look for a test that ran first. Both count as first use.

## Values in the Same Currency That Don't Match

```csharp
var a = new MonetaryValue(10m, Currency.GetCurrency("USD"));                                // system data
var b = new MonetaryValue(10m, CurrencyData.Load(CldrCurrencyData.Provider).Registry["USD"]); // CLDR data

a == b;            // false, although both are USD 10.00
var sum = a + b;   // ArgumentException: "Currencies must match to add or subtract monetary values ..."
```

The two values come from two data sources, and each data source has its own `USD`. Register one data provider at startup and take every currency from it. See [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md).

## Registry Cannot Parse

```csharp
var registry = new CurrencyRegistry("Points", [new Currency("PT1", "Points")]);
registry.ParseMonetaryValue("PT1 5");   // InvalidOperationException: "This currency registry cannot parse currency codes
                                        // because one of the currency codes violates the following requirement: ..."
```

A currency in the registry has a code or symbol with a digit, whitespace, a sign or a parenthesis, so the parser cannot tell the code or symbol apart from the number. When a symbol is the problem, the message starts "This currency registry cannot parse currency symbols for culture ...". Change the code or symbol, or do not parse with that registry. See [Registries That Cannot Parse](parsing.md#registries-that-cannot-parse).

## Invalid Currency Data

<xref:Singulink.Globalization.CurrencyData.Load*> throws <xref:System.IO.InvalidDataException> when a data provider returns data that cannot be used. After <xref:Singulink.Globalization.CurrencyRegistry.SetDefaultProvider*>, the library loads the data at the first use of the default registry, so that is where the exception is thrown. The message names the provider and the currency:

| Message contains | Fix in the provider |
| --- | --- |
| "contains multiple currencies with currency code" | Return each code once |
| "contains a localization for unknown currency" | Return a currency entry for every code that has localizations |
| "specifies an invalid type for currency" | Set a single <xref:Singulink.Globalization.CurrencyTypes> value |
| "specifies invalid rounding information for currency" | Keep decimal digits between 0 and 28 and units at zero or above |

See [Validation](data-providers.md#validation).

</div>
