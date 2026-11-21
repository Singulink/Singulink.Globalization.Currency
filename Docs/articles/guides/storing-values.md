<div class="article">

# Storing Monetary Values

Store a monetary value as its amount and its currency code, never as culture-formatted text. For JSON, the library's System.Text.Json converters do this with no setup, for single values and for money bags. For a database, store two columns and rebuild the value when you read it back. Where a single piece of text is needed, store the value in the [round-trip format](formatting.md#round-trip-text).

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
| <xref:Singulink.Globalization.MonetaryValue.Default> | `{"amount":0,"currency":null}` |
| `Currency.GetCurrency("USD")` | `"USD"` |
| A `Dictionary<Currency, decimal>` | `{"USD":1,"EUR":0.92}` |

The amount is a JSON number with the currency's decimal digits, or more when the amount needs them, so equal values write identical JSON. It reads back as an equal value.

The property names are `amount` and `currency`. If you set a <xref:System.Text.Json.JsonSerializerOptions.PropertyNamingPolicy>, the names are what the policy makes of `Amount` and `Currency` instead. When reading, the properties can come in any order:

```text
{"currency":"USD","amount":12.30}                    reads as USD 12.30
{"amount":12.30,"currency":"USD","note":"sale"}      reads as USD 12.30: unknown properties are skipped
{"Amount":12.30,"Currency":"USD"}                    JsonException, unless PropertyNameCaseInsensitive is set
{"amount":"12.30","currency":"USD"}                  JsonException, unless AllowReadingFromString is set
{"amount":12.30}                                     JsonException: the currency is missing
```

<xref:System.Text.Json.JsonSerializerDefaults.Web?displayProperty=nameWithType>, which ASP.NET Core uses, sets three options. For monetary values they mean:

| Option | Effect on monetary values |
| --- | --- |
| A camel case naming policy | The JSON looks the same: the names are still `amount` and `currency` |
| <xref:System.Text.Json.JsonSerializerOptions.PropertyNameCaseInsensitive> | Names in either case are read |
| <xref:System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString?displayProperty=nameWithType> | An amount that is a string is read too |

#### Object or Text

A value can also be written as a JSON string in the [round-trip format](formatting.md#round-trip-text). Choose the form with <xref:Singulink.Globalization.MonetaryJsonFormat>:

| Form | Looks like | Suits |
| --- | --- | --- |
| <xref:Singulink.Globalization.MonetaryJsonFormat.Object> (the default) | `{"amount":12.30,"currency":"USD"}` | Consumers that do not know this library, and databases that query the amount |
| <xref:Singulink.Globalization.MonetaryJsonFormat.LossyNumberObject> | The same object | Readers that keep every digit of a number; see [Amounts With Many Digits](#amounts-with-many-digits) |
| <xref:Singulink.Globalization.MonetaryJsonFormat.Text> | `"USD 12.30"` | Compact JSON, and places that hold one string, such as a configuration setting, a message header or a database column of JSON strings |

```csharp
var options = new JsonSerializerOptions {
    Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text) },   // with a registry: (registry, MonetaryJsonFormat.Text)
};

JsonSerializer.Serialize(new MonetaryValue(12.3m, "USD"), options);          // "USD 12.30", as a JSON string
```

A converter reads both the object form and the string form, whichever one it writes, so data written before you change the form still reads. A string must hold the round-trip format. Culture-formatted text, such as `"$12.30"` or `"USD 1,234.50"`, throws <xref:System.Text.Json.JsonException>. The default value is `{"amount":0,"currency":null}` in the object form and `"0"` in the text form.

#### Amounts With Many Digits

A reader that holds JSON numbers as doubles, as JavaScript and most JSON parsers do by default, keeps only 15 significant digits. Rounded money stays within that limit, with room for 13 digits before the point and two after it. An unrounded result usually has more:

```csharp
var share = new MonetaryValue(10m, "USD") / 3;   // USD 3.3333333333333333333333333333
// JavaScript reads that amount as 3.3333333333333335
```

The reader would get a different amount without an error. So the object form does not write an amount with more than 15 significant digits as a number. Writing such an amount throws <xref:System.ArgumentException> instead:

```csharp
JsonSerializer.Serialize(share);            // ArgumentException: "The amount of USD 3.3333333333333333333333333333 has 29 significant digits, ..."
JsonSerializer.Serialize(share.Round());    // {"amount":3.33,"currency":"USD"}
JsonSerializer.Serialize(share.Round(4));   // {"amount":3.3333,"currency":"USD"}
```

Choose how to write such an amount:

| Choice | How | Suits |
| --- | --- | --- |
| Round the value first | <xref:Singulink.Globalization.MonetaryValue.Round*>: `Round()` to the currency, or `Round(4)` to a number of decimals | Money in the currency's units, such as prices and totals; with decimals, amounts finer than the currency, such as a unit price |
| Write the amount as a string | `NumberHandling = JsonNumberHandling.WriteAsString` | Readers of the object form that need every digit, such as JavaScript |
| Write the text form | <xref:Singulink.Globalization.MonetaryJsonFormat.Text?displayProperty=nameWithType> | Readers that read the round-trip text |
| Write the number anyway | <xref:Singulink.Globalization.MonetaryJsonFormat.LossyNumberObject?displayProperty=nameWithType> | Readers that keep every digit of a number, such as this library or another reader into a <xref:System.Decimal> |

Reading never limits the digits: every converter reads every digit of a number.

#### Amounts as Strings

<xref:System.Text.Json.JsonSerializerOptions.NumberHandling> applies to the amount in the object form:

```csharp
var options = new JsonSerializerOptions {
    NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString,
};

JsonSerializer.Serialize(new MonetaryValue(12.3m, "USD"), options);   // {"amount":"12.30","currency":"USD"}
```

| Flag | Effect on the amount |
| --- | --- |
| <xref:System.Text.Json.Serialization.JsonNumberHandling.WriteAsString> | Written as a string, with the same decimals as the number and with every digit |
| <xref:System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString> | Read from a string as well, with the invariant culture |

The floating-point literals `NaN` and `Infinity` have no meaning for a <xref:System.Decimal> and throw <xref:System.Text.Json.JsonException>.

Only the setting in the options reaches the converter. A `[JsonNumberHandling]` attribute on a type applies to that type's number properties and not to its monetary values. On a property of type <xref:Singulink.Globalization.MonetaryValue>, System.Text.Json rejects the attribute with <xref:System.InvalidOperationException>.

#### Monetary Values as Dictionary Keys

A dictionary with monetary value keys writes each key in the round-trip format, whichever form the converter writes values in. Equal values write the same key:

```csharp
var counts = new Dictionary<MonetaryValue, int> { [new MonetaryValue(12.3m, "USD")] = 2 };

JsonSerializer.Serialize(counts);   // {"USD 12.30":2}, the key in the round-trip format
```

#### Default Value

<xref:Singulink.Globalization.MonetaryValue.Default> has no currency, so it is written as a zero with a `null` currency:

```csharp
JsonSerializer.Serialize(MonetaryValue.Default);                                // {"amount":0,"currency":null}
JsonSerializer.Deserialize<MonetaryValue>("""{"amount":0,"currency":null}""");   // MonetaryValue.Default
JsonSerializer.Deserialize<MonetaryValue>("""{"amount":5,"currency":null}""");   // JsonException: only a zero can have no currency
JsonSerializer.Deserialize<MonetaryValue>("null");                              // JsonException: null is not a monetary value
```

A `MonetaryValue?` writes `null` when it has no value and a zero with a `null` currency when it holds the default value, so the two read back as they were written.

The text form, a dictionary key and the type converter write the default value as `0`, the same text that formatting gives it.

#### Codes the Registry Does Not Have

Stored data can outlive a currency. Reading a code that the converter's registry does not have throws <xref:System.Text.Json.JsonException>, with the path of the value:

```csharp
JsonSerializer.Deserialize<Product>("""{"Name":"Old stock","Price":{"amount":10,"currency":"DEM"}}""");
// JsonException: "Currency code 'DEM' was not found in the 'System' currency registry." Its Path is "$.Price".
```

`DEM` is in the CLDR data only when historical currencies are included; [CLDR Currency Data](cldr-data.md#registering-the-data) shows how to include them in the default registry. If the data may hold codes that no registry of yours has, catch <xref:System.Text.Json.JsonException> where you read it.

## Money Bags in JSON

All four [money bag](money-bags.md) types serialize as arrays of monetary values, with no options to set:

```csharp
public record Account(string Name, SortedMoneyBag Balance);

var account = new Account("Travel", [new(120m, "USD"), new(80.50m, "EUR")]);

string json = JsonSerializer.Serialize(account);
// {"Name":"Travel","Balance":[{"amount":80.50,"currency":"EUR"},{"amount":120.00,"currency":"USD"}]}

var restored = JsonSerializer.Deserialize<Account>(json);    // a bag bound to the default registry
```

Each bag type carries a <xref:System.Text.Json.Serialization.JsonConverterAttribute> for <xref:Singulink.Globalization.MoneyBagJsonConverterFactory>. The entries of a bag look exactly like single monetary values and follow the same naming policy and case rules.

#### Reading Bags Into Your Own Registry

Register the bag factory with the same registry as the value converters:

```csharp
var options = new JsonSerializerOptions {
    Converters = {
        new MonetaryValueJsonConverter(registry),    // single monetary values
        new CurrencyJsonConverter(registry),         // currencies, as values and as dictionary keys
        new MoneyBagJsonConverterFactory(registry),  // money bags and the values in them
    },
};

var restored = JsonSerializer.Deserialize<Account>(json, options);

restored.Balance.Registry == registry;               // true
restored.Balance["USD"].Currency == registry["USD"]; // true
```

You need the factory as well as the value converters, because a bag reads its values with its own registry and not with the value converter in the options. That keeps a bag and its values in one registry. Bags follow the factory's registry, so with only a value converter registered, bags still read into the default registry.

To choose the form of the values in a bag, pass a <xref:Singulink.Globalization.MonetaryJsonFormat> to the factory as well, such as `new MoneyBagJsonConverterFactory(registry, MonetaryJsonFormat.Text)`.

#### What a Bag Looks Like

| Bag | JSON |
| --- | --- |
| A bag with two values | `[{"amount":80.50,"currency":"EUR"},{"amount":120.00,"currency":"USD"}]` |
| The same bag in the text form | `["EUR 80.50","USD 120.00"]` |
| An empty bag | `[]` |
| A `null` bag | `null` |

Values are written in the bag's own order, so sorted bags are written sorted by currency code. A value with a zero amount is written and read like any other, since a bag keeps a currency at zero until it is removed.

When an array lists a currency more than once, the amounts are added, as they are when a bag is created from a list of values:

```text
[{"amount":10,"currency":"USD"},{"amount":5,"currency":"USD"},{"amount":2,"currency":"EUR"}]    reads as EUR 2.00, USD 15.00
["USD 10.00","0"]                                                                               reads as USD 10.00: the default value adds nothing
[{"amount":10,"currency":"USD"},null]                                                           JsonException: null is not a monetary value
{"amount":10,"currency":"USD"}                                                                  JsonException: not an array
[{"amount":10,"currency":"XAU"}]                                                                JsonException: a code the registry does not have
```

An element that is not a valid monetary value throws the same <xref:System.Text.Json.JsonException> as a single value does. An array can mix the object and text forms.

#### Bag Interfaces

A property typed as a bag interface writes the bag it holds. It reads as the bag type that a collection expression of that interface type creates:

| Property type | Reads as |
| --- | --- |
| <xref:Singulink.Globalization.IReadOnlyMoneyBag> | <xref:Singulink.Globalization.MoneyBag> |
| <xref:Singulink.Globalization.IMoneyBag> | <xref:Singulink.Globalization.MoneyBag> |
| <xref:Singulink.Globalization.IImmutableMoneyBag> | <xref:Singulink.Globalization.ImmutableMoneyBag> |

```csharp
public class Holder { public IReadOnlyMoneyBag Balance { get; set; } }

var holder = new Holder { Balance = new SortedMoneyBag(values) };
string json = JsonSerializer.Serialize(holder);           // written in the sorted bag's order
var restored = JsonSerializer.Deserialize<Holder>(json);  // restored.Balance is a MoneyBag with the same values
```

Declare the property with a concrete bag type when the type that is read matters, such as a sorted bag.

## Newtonsoft.Json and Configuration

<xref:Singulink.Globalization.MonetaryValue> and <xref:Singulink.Globalization.Currency> each have a <xref:System.ComponentModel.TypeConverter>. Newtonsoft.Json, configuration binding, ASP.NET Core model binding and XAML go through it, so they need no setup either. A value becomes a string in the [round-trip format](formatting.md#round-trip-text), and a currency becomes its code:

```csharp
public record Product(string Name, MonetaryValue Price, Currency Currency);

var product = new Product("Coffee", new MonetaryValue(4.5m, "USD"), Currency.GetCurrency("USD"));

JsonConvert.SerializeObject(product);   // {"Name":"Coffee","Price":"USD 4.50","Currency":"USD"}
```

Configuration reads a setting the same way:

```csharp
// appsettings.json: { "Shop": { "ShippingFee": "USD 4.50" } }
var fee = configuration.GetValue<MonetaryValue>("Shop:ShippingFee");   // USD 4.50
```

What the type converter writes and reads depends on the culture that the caller passes:

| Culture | Writes and reads | Used by |
| --- | --- | --- |
| The invariant culture | The round-trip format | Newtonsoft.Json, configuration binding |
| A specific culture | The culture's default format and parsing | Model binding with the request's culture |
| No culture | The current culture's default format and parsing | `ConvertToString` and `ConvertFromString` |

The default value is written as `0` in every case. Currency codes are looked up in <xref:Singulink.Globalization.CurrencyRegistry.Default>, and there is no option for another registry. Text that cannot be read throws <xref:System.ArgumentException>, which Newtonsoft.Json reports as a `JsonSerializationException` and configuration binding as an <xref:System.InvalidOperationException>.

Newtonsoft.Json writes a money bag as an array of these strings and reads one back into a <xref:Singulink.Globalization.MoneyBag> or a <xref:Singulink.Globalization.SortedMoneyBag>. It cannot create the immutable bags.

> [!TIP]
> In an app published with native AOT, call `TypeDescriptor.RegisterType<MonetaryValue>()` and then get the converter with `TypeDescriptor.GetConverterFromRegisteredType`. <xref:System.ComponentModel.TypeDescriptor.GetConverter*> gives a trim warning there.

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

## Entity Framework Core

A <xref:Singulink.Globalization.MonetaryValue> property can be stored in one text column or in two columns. The examples here ran with EF Core 11 and SQLite. There are two mappings to choose from:

| Mapping | Stores | Queries can | Null and default value |
| --- | --- | --- | --- |
| [A value converter](#one-column) | The round-trip text in one column | Test the column for equality with a value | `NULL` for a null `MonetaryValue?`, `0` for the default value |
| [A complex property](#two-columns) | The amount and the currency code in two columns | Add up, compare and filter amounts, and filter by currency | `NULL` in both columns for a null `MonetaryValue?`. You choose whether the column [can hold the default value](#default-value-in-two-columns) |

#### One column

A value converter stores the [round-trip format](formatting.md#round-trip-text):

```csharp
modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion(
    value => value.ToString("O", null),                                   // "USD 12.30" in a TEXT column
    text => MonetaryValue.Parse(text, CultureInfo.InvariantCulture));
```

To store every monetary value property that way, declare the converter once:

```csharp
public sealed class MonetaryValueToStringConverter() : ValueConverter<MonetaryValue, string>(
    value => value.ToString("O", null),
    text => MonetaryValue.Parse(text, CultureInfo.InvariantCulture));

protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.Properties<MonetaryValue>().HaveConversion<MonetaryValueToStringConverter>();
}
```

A `MonetaryValue?` that is null is stored as `NULL`, and the default value as `0`.

A query can test the column for equality with a value, as in `Where(p => p.Price == price)`. It cannot add up amounts or tell which is larger, and sorting by the column sorts the text. Use two columns when the database works with the amount.

#### Two columns

A complex property stores the amount and the currency code in columns of their own:

```csharp
modelBuilder.Entity<Order>().ComplexProperty(o => o.Total, total => {
    total.Property(m => m.Amount);                                        // the Total_Amount column
    total.Property(m => m.Currency).HasConversion(                        // the Total_Currency column, with the code
        currency => currency.CurrencyCode,
        code => Currency.GetCurrency(code));
});
```

Queries can use both parts:

```csharp
db.Orders.Sum(o => o.Total.Amount);                                       // added up in the database
db.Orders.Where(o => o.Total.Amount > 15);
db.Orders.Count(o => o.Total.Currency == Currency.GetCurrency("USD"));
```

A `MonetaryValue?` complex property works the same way, and stores `NULL` in both columns when it is null.

#### Default value in two columns

The mapping above makes the currency column `NOT NULL`. The default value has no currency, so the database rejects it, and every row is certain to hold a value with a currency.

To store the default value as well, map <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> in place of `Currency`:

```csharp
modelBuilder.Entity<Order>().ComplexProperty(o => o.Total, total => {
    total.Property(m => m.Amount);
    total.Property(m => m.CurrencyOrDefault)                              // a nullable column
        .HasColumnName("Total_Currency")                                  // otherwise Total_CurrencyOrDefault
        .HasConversion(
            currency => currency!.CurrencyCode,
            code => Currency.GetCurrency(code));
});
```

The default value is then stored as a zero amount and a `NULL` code, and reads back as the default value. Queries use `CurrencyOrDefault`:

```csharp
db.Orders.Count(o => o.Total.CurrencyOrDefault == Currency.GetCurrency("USD"));
db.Orders.Count(o => o.Total.CurrencyOrDefault == null);                  // rows that hold the default value
```

A `MonetaryValue?` property mapped this way keeps null and the default value apart: null is `NULL` in both columns, and the default value is `0` and `NULL`.

Map <xref:Singulink.Globalization.MonetaryValue.Currency> or <xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault>, not both. A row with a `NULL` code and an amount other than zero does not hold a monetary value, and reading it throws `ArgumentException`.

## Never Store Culture-Formatted Text

A formatted value looks like a complete record, but its text depends on the culture that formatted it:

```csharp
var value = new MonetaryValue(1234.5m, "USD");

value.ToString(null, CultureInfo.GetCultureInfo("en-US"));   // "USD 1,234.50"
value.ToString(null, CultureInfo.GetCultureInfo("de-DE"));   // "1.234,50 USD", the same value
```

> [!WARNING]
> The default format is meant for display. It depends on the culture's number formatting, so text written with it can read back as a different amount. To store a value as text, use the [round-trip format](formatting.md#round-trip-text).

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - The default value and how values are created.
- [Why Two USD Currencies Are Not Equal](../concepts/currency-identity.md) - Why the registry you read with matters.
- [Money Bags](money-bags.md) - The bag types and their operations.
- [Why Did This Throw?](why-did-this-throw.md) - The exceptions a lookup, a constructor or a converter can throw.

</div>
