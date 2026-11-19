<div class="article">

# Storing Monetary Values

Store a monetary value as its amount and its currency code, never as formatted text. For JSON, the library's System.Text.Json converters do this with no setup. For a database, store two columns and rebuild the value when you read it back.

## JSON

<xref:Singulink.Globalization.MonetaryValue> and <xref:Singulink.Globalization.Currency> serialize with <xref:System.Text.Json.JsonSerializer> as they are, with no options to set:

```csharp
public record Product(string Name, MonetaryValue Price);

var product = new Product("Coffee", new MonetaryValue(4.50m, "USD"));

string json = JsonSerializer.Serialize(product);              // {"Name":"Coffee","Price":{"amount":4.50,"currency":"USD"}}
var restored = JsonSerializer.Deserialize<Product>(json);     // the code "USD" is looked up in the default registry

restored.Price == product.Price;                              // true
```

Both types carry a <xref:System.Text.Json.Serialization.JsonConverterAttribute> for <xref:Singulink.Globalization.MonetaryValueJsonConverter> and <xref:Singulink.Globalization.CurrencyJsonConverter>, so this works for properties, arrays, lists and dictionary values alike. Reading looks each code up in <xref:Singulink.Globalization.CurrencyRegistry.Default>.

> [!TIP]
> The converters use no reflection. They work with a source-generated <xref:System.Text.Json.Serialization.JsonSerializerContext> and in apps published with native AOT.

#### Reading From Your Own Registry

To read codes from another registry, add converters for it to <xref:System.Text.Json.JsonSerializerOptions.Converters>. A converter in the options takes precedence over the one on the type:

```csharp
var options = new JsonSerializerOptions {
    Converters = {
        new MonetaryValueJsonConverter(registry),    // monetary values read their codes from registry
        new CurrencyJsonConverter(registry),         // so do currencies, as values and as dictionary keys
    },
};

var restored = JsonSerializer.Deserialize<Product>(json, options);

restored.Price.Currency == registry["USD"];          // true
```

The registry matters only when reading. A currency belongs to the registry it came from, and the `USD` of one registry is not the `USD` of another (see [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md)). Writing never looks at the registry: a value always writes its own currency's code.

#### What the JSON Looks Like

| Value | JSON |
| --- | --- |
| `new MonetaryValue(12.30m, "USD")` | `{"amount":12.30,"currency":"USD"}` |
| <xref:Singulink.Globalization.MonetaryValue.Default> | `null` |
| `Currency.GetCurrency("USD")` | `"USD"` |
| A `Dictionary<Currency, decimal>` | `{"USD":1,"EUR":0.92}` |

The amount is a JSON number that keeps the exact <xref:System.Decimal> value and its scale, so `12.30` stays `12.30`. The property names follow <xref:System.Text.Json.JsonSerializerOptions.PropertyNamingPolicy> if you set one, which converts `Amount` and `Currency`. When reading, the properties can come in any order:

```text
{"currency":"USD","amount":12.30}                    reads as USD 12.30
{"amount":12.30,"currency":"USD","note":"sale"}      reads as USD 12.30: unknown properties are skipped
{"Amount":12.30,"Currency":"USD"}                    JsonException, unless PropertyNameCaseInsensitive is set
{"amount":"12.30","currency":"USD"}                  JsonException: the amount must be a number
{"amount":12.30}                                     JsonException: the currency is missing
```

<xref:System.Text.Json.JsonSerializerDefaults.Web?displayProperty=nameWithType>, which ASP.NET Core uses, sets a camel case naming policy and <xref:System.Text.Json.JsonSerializerOptions.PropertyNameCaseInsensitive>, so the JSON looks the same and either case reads.

#### Default Value

<xref:Singulink.Globalization.MonetaryValue.Default> has no currency, so it is written as `null`, and `null` reads back as the default value:

```csharp
JsonSerializer.Serialize(MonetaryValue.Default);        // "null"
JsonSerializer.Deserialize<MonetaryValue>("null");      // MonetaryValue.Default
JsonSerializer.Deserialize<MonetaryValue?>("null");     // null
```

A `MonetaryValue?` that holds the default value is also written as `null`, so it reads back as `null` rather than as the default value.

#### Codes the Registry Does Not Have

Stored data can outlive a currency. Reading a code that the converter's registry does not have throws <xref:System.Text.Json.JsonException>, with the path of the value:

```csharp
JsonSerializer.Deserialize<Product>("""{"Name":"Old stock","Price":{"amount":10,"currency":"DEM"}}""");
// JsonException: "Currency code 'DEM' was not found in the 'System' currency registry." Its Path is "$.Price".
```

`DEM` is in the CLDR data only when historical currencies are included; [CLDR Currency Data](cldr-data.md#registering-the-data) shows how to include them in the default registry. If the data may hold codes that no registry of yours has, catch <xref:System.Text.Json.JsonException> where you read it.

## Database Columns

Keep the amount as a <xref:System.Decimal> and the currency as its code, in two database columns or two properties of the type you store:

```csharp
// The stored form: two plain values that any database can hold.
public record StoredPrice(decimal Amount, string CurrencyCode);

public static StoredPrice ToStored(MonetaryValue value) =>
    new(value.Amount, value.Currency.CurrencyCode);

public static MonetaryValue FromStored(StoredPrice stored) =>
    new(stored.Amount, stored.CurrencyCode);   // the code is looked up in the default registry
```

When the value is rebuilt, the constructor looks the code up in <xref:Singulink.Globalization.CurrencyRegistry.Default>, so the value gets its currency from whatever data the application registered at startup. To rebuild against another registry, look the code up there and pass the currency:

```csharp
var value = new MonetaryValue(stored.Amount, registry[stored.CurrencyCode]);   // registry: the registry to rebuild against
```

The amount keeps all the precision it had, since nothing is rounded until you ask for it. Give the amount column enough decimal places for the values you store, or round the value with <xref:Singulink.Globalization.MonetaryValue.Round*> before storing it.

#### Default Values and Unknown Codes

<xref:Singulink.Globalization.MonetaryValue.Default> has no currency, so <xref:Singulink.Globalization.MonetaryValue.Currency> throws on it. If a stored value can be the default value, store a `null` code and rebuild with <xref:Singulink.Globalization.MonetaryValue.CreateOrDefault*>:

```csharp
// The same stored form, with a code that may be null:
public record StoredPrice(decimal Amount, string? CurrencyCode);

public static StoredPrice ToStored(MonetaryValue value) =>
    new(value.Amount, value.CurrencyOrDefault?.CurrencyCode);    // null for the default value

public static MonetaryValue FromStored(StoredPrice stored) =>
    MonetaryValue.CreateOrDefault(stored.Amount, stored.CurrencyCode);  // the default value for a null code and a zero amount
```

<xref:Singulink.Globalization.MonetaryValue.CreateOrDefault*> throws <xref:System.ArgumentException> for a `null` code with an amount that is not zero.

If the default registry does not have a stored code, such as the code of a historical currency, the constructor throws <xref:System.ArgumentException>. When the data may hold such a code, check it first:

```csharp
if (Currency.TryGetCurrency(stored.CurrencyCode, out var currency))
    value = new MonetaryValue(stored.Amount, currency);
else
    ReportUnknownCurrency(stored);   // such as "DEM", which is in the CLDR data only when historical currencies are included
```

#### Money Bags

Store a money bag as one row per currency, and rebuild it from the rows:

```csharp
List<StoredPrice> rows = bag.Select(ToStored).ToList();      // one row per currency in the bag

var restored = new MoneyBag(rows.Select(FromStored));
```

## Why Not Store the Formatted String

A formatted value looks like a complete record, but its text depends on the culture that formatted it:

```csharp
var value = new MonetaryValue(1234.5m, "USD");

value.ToString(null, CultureInfo.GetCultureInfo("en-US"));   // "USD 1,234.50"
value.ToString(null, CultureInfo.GetCultureInfo("de-DE"));   // "1.234,50 USD", the same value
```

> [!WARNING]
> The default format is meant for display. It depends on the culture's number formatting, so it is not a stable round-trip format for storage. See [Formatting](formatting.md).

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - The default value and how values are created.
- [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md) - Why the registry you read with matters.
- [Money Bags](money-bags.md) - The bag types and their operations.
- [Why Did This Throw?](why-did-this-throw.md) - The exceptions a lookup, a constructor or a converter can throw.

</div>
