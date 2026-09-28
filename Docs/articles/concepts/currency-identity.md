<div class="article">

# Currency Identity

A <xref:Singulink.Globalization.Currency> is compared by reference, not by code. Two currency objects with the code `USD` are two different currencies unless they are the same object. This article explains why the library works this way, what it means in practice, and how to avoid the situations where it bites.

### The rule

Every <xref:Singulink.Globalization.CurrencyRegistry> populated from a data source creates its own currency instances. The system registry has one `USD`, a registry loaded from the CLDR package has another, and a registry loaded from a second provider instance has a third. Within a registry a currency is a singleton: every lookup of `USD` returns the same instance, and registries built by hand from existing currencies share those instances. Across data sources, they are distinct.

Monetary values inherit this. <xref:Singulink.Globalization.MonetaryValue> equality compares the currency instance and the amount, so a value created from the system registry's `USD` is not equal to, and cannot be added to, a value created from the CLDR registry's `USD`, even though both would format as `USD 10.00`.

## Why Not Compare by Code

Two currencies with the same code but from different data sources are not interchangeable. They can have different rounding policies, a different cash policy or none at all, different symbols, and different localized names. Treating them as equal would let a value carry one set of rules and be combined with a value carrying another, and the result would follow whichever happened to be on the left. Reference identity makes the data source part of the identity, so a currency always means exactly one set of rules.

The other reason is the same one that makes the library reject arithmetic across currencies: mistakes should fail loudly. An application that accidentally builds two registries from two data sources and mixes their values has a real configuration problem, and a thrown exception at the point of mixing is far easier to diagnose than a subtle inconsistency in totals.

## What This Means in Practice

For most applications the rule is invisible. There is one default registry, every currency code resolves through it, and every value uses one of its currencies. The situations where identity matters are:

- **Changing the default registry after use.** The default can only be set before it is first accessed, precisely so that no currencies from a previous default can exist.
- **Loading the same provider twice.** <xref:Singulink.Globalization.CurrencyData.Load*> caches by provider instance, so use one provider instance per data source. The CLDR package exposes a single <xref:Singulink.Globalization.CldrCurrencyData.Provider>, so this happens naturally unless a custom provider is constructed repeatedly.
- **Mixing the system registry and a provider.** Values created before a provider is registered, or from an explicitly created system-based registry, are not compatible with values from the provider's registry. Register the provider at startup and use it exclusively.
- **Tests.** Tests that create their own registries should create values from those registries, not from currency codes, since codes resolve through the process-wide default.

Registries created from the same data all share instances, so <xref:Singulink.Globalization.CurrencyData.CreateRegistry*> with different <xref:Singulink.Globalization.CurrencyTypes> and hand-built registries containing subsets of one source are all compatible with each other.

## Checking Compatibility

<xref:Singulink.Globalization.MonetaryValue.CurrencyOrDefault> and <xref:Singulink.Globalization.Currency> reference comparison answer the question directly. Comparing <xref:Singulink.Globalization.Currency.CurrencyCode> tells you two currencies represent the same real-world currency, which is a different question, and is the right check when reconciling data that came from outside the process.

## Further Reading

These articles cover the related topics in more depth:

- [Currencies and Registries](../guides/currencies-and-registries.md) - The default registry lifecycle and custom registries.
- [Monetary Values](../guides/monetary-values.md) - Equality and comparison semantics.
- [CLDR Currency Data](../guides/cldr-data.md) - Loading data once and sharing it.

</div>
