using System.Text.Json;

namespace Singulink.Globalization.Tests.MoneyBagJsonConverterFactoryTests;

public static class Bags
{
    [PrefixTestClass]
    public class TMoneyBag : Tests<MoneyBag>
    {
        protected override MoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => new(registry, values);
    }

    [PrefixTestClass]
    public class TSortedMoneyBag : Tests<SortedMoneyBag>
    {
        protected override SortedMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => new(registry, values);
    }

    [PrefixTestClass]
    public class TImmutableMoneyBag : Tests<ImmutableMoneyBag>
    {
        protected override ImmutableMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values) => ImmutableMoneyBag.CreateRange(registry, values);
    }

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Tests<ImmutableSortedMoneyBag>
    {
        protected override ImmutableSortedMoneyBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values)
            => ImmutableSortedMoneyBag.CreateRange(registry, values);
    }

    public abstract class Tests<TBag> where TBag : class, IReadOnlyMoneyBag
    {
        private const string Json = """[{"amount":12.34,"currency":"USD"},{"amount":5,"currency":"EUR"}]""";

        // A registry with its own USD and EUR, which are different currencies than the default registry's USD and EUR.
        private static readonly Currency TestUsd = new("USD", "Test Dollar");
        private static readonly Currency TestEur = new("EUR", "Test Euro");
        private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd, TestEur]);

        private static readonly JsonSerializerOptions TestRegistryOptions = new() { Converters = { new MoneyBagJsonConverterFactory(TestRegistry) } };

        protected abstract TBag Create(CurrencyRegistry registry, IEnumerable<MonetaryValue> values);

        private TBag CreateDefault(params MonetaryValue[] values) => Create(CurrencyRegistry.Default, values);

        [TestMethod]
        public void DefaultRegistry_RoundTrips()
        {
            var bag = CreateDefault(new MonetaryValue(12.34m, "USD"), new MonetaryValue(5m, "EUR"), new MonetaryValue(-1.50m, "CAD"));

            string json = JsonSerializer.Serialize(bag);
            var result = JsonSerializer.Deserialize<TBag>(json)!;

            result.ShouldBeOfType<TBag>();
            result.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            result.ShouldBe(bag, ignoreOrder: true);
            JsonSerializer.Serialize(result).ShouldBe(json);
        }

        [TestMethod]
        public void Write_FollowsEnumerationOrder()
        {
            var bag = CreateDefault(new MonetaryValue(1m, "USD"), new MonetaryValue(2m, "EUR"), new MonetaryValue(3m, "CAD"), new MonetaryValue(4m, "JPY"));

            string expected = "[" + string.Join(",", bag.Select(v => $$"""{"amount":{{v.Amount}},"currency":"{{v.Currency.CurrencyCode}}"}""")) + "]";
            JsonSerializer.Serialize(bag).ShouldBe(expected);

            if (bag.IsSorted)
                JsonSerializer.Serialize(bag).ShouldBe("""[{"amount":3,"currency":"CAD"},{"amount":2,"currency":"EUR"},{"amount":4,"currency":"JPY"},{"amount":1,"currency":"USD"}]""");
        }

        [TestMethod]
        public void CustomRegistry_ReadsIntoItsRegistry()
        {
            var result = JsonSerializer.Deserialize<TBag>(Json, TestRegistryOptions)!;

            result.Registry.ShouldBeSameAs(TestRegistry);
            result.Currencies.ShouldBe([TestUsd, TestEur], ignoreOrder: true);
            result[TestUsd].ShouldBe(new MonetaryValue(12.34m, TestUsd));

            // The values belong to the bag's registry, so they combine with other values of that registry.
            (result[TestUsd] + new MonetaryValue(1m, TestUsd)).ShouldBe(new MonetaryValue(13.34m, TestUsd));
        }

        [TestMethod]
        public void CustomRegistry_RoundTrips()
        {
            var bag = Create(TestRegistry, [new(1m, TestUsd), new(2.50m, TestEur)]);

            string json = JsonSerializer.Serialize(bag, TestRegistryOptions);
            var result = JsonSerializer.Deserialize<TBag>(json, TestRegistryOptions)!;

            result.Registry.ShouldBeSameAs(TestRegistry);
            result.ShouldBe(bag, ignoreOrder: true);
        }

        [TestMethod]
        public void Write_DoesNotUseRegistry()
        {
            // The bag is bound to the default registry, which the factory's registry does not match. The codes are written regardless.

            var bag = CreateDefault(new MonetaryValue(1m, "CAD"));
            JsonSerializer.Serialize(bag, TestRegistryOptions).ShouldBe("""[{"amount":1,"currency":"CAD"}]""");
        }

        [TestMethod]
        public void CodeNotInRegistry_ThrowsJsonException()
        {
            var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TBag>("""[{"amount":1,"currency":"USD"},{"amount":1,"currency":"CAD"}]""", TestRegistryOptions));
            ex.Message.ShouldBe("Currency code 'CAD' was not found in the 'Test' currency registry.");
            ex.Path.ShouldBe("$");
        }

        [TestMethod]
        public void Empty_RoundTrips()
        {
            JsonSerializer.Serialize(CreateDefault()).ShouldBe("[]");

            var result = JsonSerializer.Deserialize<TBag>("[]", TestRegistryOptions)!;
            result.Count.ShouldBe(0);
            result.Registry.ShouldBeSameAs(TestRegistry);
        }

        [TestMethod]
        public void Null_RoundTrips()
        {
            JsonSerializer.Serialize<TBag?>(null).ShouldBe("null");
            JsonSerializer.Deserialize<TBag>("null").ShouldBeNull();
        }

        [TestMethod]
        public void ZeroAmounts_AreKept()
        {
            var bag = CreateDefault(new MonetaryValue(0m, "USD"), new MonetaryValue(0.00m, "EUR"));

            string json = JsonSerializer.Serialize(bag);
            json.ShouldContain("""{"amount":0,"currency":"USD"}""");
            json.ShouldContain("""{"amount":0.00,"currency":"EUR"}""");

            var result = JsonSerializer.Deserialize<TBag>(json)!;
            result.Count.ShouldBe(2);
            result.ShouldBe(bag, ignoreOrder: true);
        }

        [TestMethod]
        public void DuplicateCurrencies_AreAdded()
        {
            const string json = """[{"amount":1.25,"currency":"USD"},{"amount":5,"currency":"EUR"},{"amount":2,"currency":"USD"},{"amount":-0.25,"currency":"USD"}]""";

            var result = JsonSerializer.Deserialize<TBag>(json)!;
            result.Count.ShouldBe(2);
            result["USD"].ShouldBe(new MonetaryValue(3.00m, "USD"));
            result["EUR"].ShouldBe(new MonetaryValue(5m, "EUR"));
        }

        [TestMethod]
        [DataRow("""{"amount":1,"currency":"USD"}""", "Expected an array of monetary values for a money bag but found a token of type 'StartObject'.", DisplayName = "Object")]
        [DataRow("12", "Expected an array of monetary values for a money bag but found a token of type 'Number'.", DisplayName = "Number")]
        [DataRow("\"USD 12\"", "Expected an array of monetary values for a money bag but found a token of type 'String'.", DisplayName = "String")]
        [DataRow("""[{"amount":1,"currency":"USD"},null]""", "The money bag contains a null element at index 1.", DisplayName = "Null element")]
        [DataRow("""[12]""", "Expected a monetary value object or null but found a token of type 'Number'.", DisplayName = "Number element")]
        [DataRow("""[["USD",1]]""", "Expected a monetary value object or null but found a token of type 'StartArray'.", DisplayName = "Array element")]
        [DataRow("""[{"amount":"1","currency":"USD"}]""", "Expected a number for the 'amount' property but found a token of type 'String'.", DisplayName = "String amount")]
        [DataRow("""[{"amount":1}]""", "The monetary value is missing the 'currency' property.", DisplayName = "Missing currency")]
        [DataRow("""[{"currency":"USD"}]""", "The monetary value is missing the 'amount' property.", DisplayName = "Missing amount")]
        [DataRow("""[{"amount":1,"currency":"ZZZ"}]""", "Currency code 'ZZZ' was not found in the '", DisplayName = "Unknown code")]
        public void BadInput_ThrowsJsonException(string json, string message)
        {
            var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TBag>(json));
            ex.Message.ShouldStartWith(message);
            ex.Path.ShouldNotBeNull();
        }

        [TestMethod]
        public void TruncatedInput_ThrowsJsonException()
        {
            Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TBag>("""[{"amount":1,"currency":"USD"}"""));
        }

        [TestMethod]
        public void Property_RoundTrips()
        {
            var holder = new Holder { Bag = CreateDefault(new MonetaryValue(1m, "USD")), Missing = null };

            string json = JsonSerializer.Serialize(holder);
            json.ShouldBe("""{"Bag":[{"amount":1,"currency":"USD"}],"Missing":null}""");

            var result = JsonSerializer.Deserialize<Holder>(json, TestRegistryOptions)!;
            result.Bag!.Registry.ShouldBeSameAs(TestRegistry);
            result.Bag.Single().ShouldBe(new MonetaryValue(1m, TestUsd));
            result.Missing.ShouldBeNull();
        }

        [TestMethod]
        public void BadElementInProperty_ExceptionHasPath()
        {
            var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Holder>("""{"Bag":[{"amount":1,"currency":"ZZZ"}]}"""));
            ex.Path.ShouldBe("$.Bag");
        }

        [TestMethod]
        public void InCollection_RoundTrips()
        {
            List<TBag> bags = [CreateDefault(new MonetaryValue(1m, "USD")), CreateDefault(), CreateDefault(new MonetaryValue(2m, "EUR"))];

            string json = JsonSerializer.Serialize(bags);
            json.ShouldBe("""[[{"amount":1,"currency":"USD"}],[],[{"amount":2,"currency":"EUR"}]]""");

            var result = JsonSerializer.Deserialize<List<TBag>>(json, TestRegistryOptions)!;
            result.Count.ShouldBe(3);
            result.ShouldAllBe(b => b.Registry == TestRegistry);
            result[2].Single().ShouldBe(new MonetaryValue(2m, TestEur));
        }

        [TestMethod]
        public void AsDictionaryValue_RoundTrips()
        {
            var accounts = new Dictionary<string, TBag> { ["cash"] = CreateDefault(new MonetaryValue(1m, "USD")), ["card"] = CreateDefault(new MonetaryValue(2m, "EUR")) };

            string json = JsonSerializer.Serialize(accounts);
            json.ShouldBe("""{"cash":[{"amount":1,"currency":"USD"}],"card":[{"amount":2,"currency":"EUR"}]}""");

            var result = JsonSerializer.Deserialize<Dictionary<string, TBag>>(json)!;
            result["cash"].ShouldBe(accounts["cash"]);
            result["card"].ShouldBe(accounts["card"]);
        }

        [TestMethod]
        public void FactoryOnly_EntriesUseFactoryRegistry()
        {
            // The options have no monetary value converter, but the entries still come from the factory's registry.

            var result = JsonSerializer.Deserialize<TBag>(Json, TestRegistryOptions)!;
            result.ShouldAllBe(v => v.Currency == TestUsd || v.Currency == TestEur);
        }

        [TestMethod]
        public void ValueConverterOnly_BagUsesDefaultRegistry()
        {
            // A monetary value converter in the options does not change how bags read: the bag and its entries use the default registry.

            var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(TestRegistry) } };

            var result = JsonSerializer.Deserialize<TBag>(Json, options)!;
            result.Registry.ShouldBeSameAs(CurrencyRegistry.Default);
            result.Currencies.ShouldBe([Currency.GetCurrency("USD"), Currency.GetCurrency("EUR")], ignoreOrder: true);

            // Values outside bags still use the value converter.
            JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"USD"}""", options).Currency.ShouldBeSameAs(TestUsd);
        }

        [TestMethod]
        public void FactoryAndValueConverterWithDifferentRegistries_BagUsesFactoryRegistry()
        {
            var options = new JsonSerializerOptions {
                Converters = { new MonetaryValueJsonConverter(CurrencyRegistry.Default), new MoneyBagJsonConverterFactory(TestRegistry) },
            };

            var result = JsonSerializer.Deserialize<TBag>(Json, options)!;
            result.Registry.ShouldBeSameAs(TestRegistry);
            result[TestUsd].Amount.ShouldBe(12.34m);
        }

        [TestMethod]
        public void NamingPolicy_PassesThroughToEntries()
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper };
            var bag = CreateDefault(new MonetaryValue(1m, "USD"));

            string json = JsonSerializer.Serialize(bag, options);
            json.ShouldBe("""[{"AMOUNT":1,"CURRENCY":"USD"}]""");
            JsonSerializer.Deserialize<TBag>(json, options).ShouldBe(bag);

            Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TBag>("""[{"amount":1,"currency":"USD"}]""", options));
        }

        [TestMethod]
        public void CaseInsensitive_PassesThroughToEntries()
        {
            Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TBag>("""[{"Amount":1,"CURRENCY":"USD"}]"""));

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            JsonSerializer.Deserialize<TBag>("""[{"Amount":1,"CURRENCY":"USD"}]""", options).ShouldBe(CreateDefault(new MonetaryValue(1m, "USD")));
        }

        public class Holder
        {
            public TBag? Bag { get; set; }

            public TBag? Missing { get; set; }
        }
    }
}
