<div class="article">

# Storing Monetary Values

Store a monetary value as two parts, its amount and its currency code, and rebuild the value from them when you read it back. This guide shows how, for database rows and any other stored form, and how to handle the default value and codes the registry does not know.

## Why Not Store the Formatted String

A formatted value looks like a complete record, but its text depends on the culture that formatted it:

```csharp
var value = new MonetaryValue(1234.5m, "USD");

value.ToString(null, CultureInfo.GetCultureInfo("en-US"));   // "USD 1,234.50"
value.ToString(null, CultureInfo.GetCultureInfo("de-DE"));   // "1.234,50 USD", the same value
```

> [!WARNING]
> The default format is meant for display. It depends on the culture's number formatting, so it is not a stable round-trip format for storage. See [Formatting](formatting.md).

## Two Columns or Two Properties

Keep the amount as a <xref:System.Decimal> and the currency as its code, in two database columns or two properties of the type you store:

```csharp
// The stored form: two plain values that any database or file format can hold.
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

## Default Values and Unknown Codes

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

Stored data can outlive a currency. If the default registry does not have a code, such as the code of a historical currency, the constructor throws <xref:System.ArgumentException>. When the data may hold such a code, check it first:

```csharp
if (Currency.TryGetCurrency(stored.CurrencyCode, out var currency))
    value = new MonetaryValue(stored.Amount, currency);
else
    ReportUnknownCurrency(stored);   // such as "DEM", which is in the CLDR data only when historical currencies are included
```

[CLDR Currency Data](cldr-data.md#registering-the-data) shows how to include historical currencies in the default registry.

## Money Bags

Store a money bag as one row per currency, and rebuild it from the rows:

```csharp
List<StoredPrice> rows = bag.Select(ToStored).ToList();      // one row per currency in the bag

var restored = new MoneyBag(rows.Select(FromStored));
```

## Next Steps

Continue with these related articles:

- [Monetary Values](monetary-values.md) - The default value and how values are created.
- [Money Bags](money-bags.md) - The bag types and their operations.
- [Why Did This Throw?](why-did-this-throw.md) - The exceptions a lookup or a constructor can throw.

</div>
