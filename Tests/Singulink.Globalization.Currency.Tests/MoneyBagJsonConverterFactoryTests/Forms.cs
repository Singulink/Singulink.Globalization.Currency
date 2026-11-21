using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization.Tests.MoneyBagJsonConverterFactoryTests;

[PrefixTestClass]
public class Forms
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly Currency TestEur = new("EUR", "Test Euro");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd, TestEur]);

    [TestMethod]
    public void TextForm_WritesArrayOfStrings()
    {
        var options = new JsonSerializerOptions { Converters = { new MoneyBagJsonConverterFactory(MonetaryJsonFormat.Text) } };
        var bag = new SortedMoneyBag { new(12.3m, "USD"), new(-5m, "EUR"), new(1250m, "JPY") };

        string json = JsonSerializer.Serialize(bag, options);
        json.ShouldBe("""["EUR -5.00","JPY 1250","USD 12.30"]""");

        JsonSerializer.Deserialize<SortedMoneyBag>(json, options).ShouldBe(bag);
        JsonSerializer.Deserialize<SortedMoneyBag>(json).ShouldBe(bag);
    }

    [TestMethod]
    public void ObjectForm_IsTheDefault()
    {
        var bag = new SortedMoneyBag { new(12.3m, "USD") };

        JsonSerializer.Serialize(bag).ShouldBe("""[{"amount":12.30,"currency":"USD"}]""");
        JsonSerializer.Serialize(bag, new JsonSerializerOptions { Converters = { new MoneyBagJsonConverterFactory(MonetaryJsonFormat.Object) } })
            .ShouldBe("""[{"amount":12.30,"currency":"USD"}]""");
    }

    [TestMethod]
    public void MixedArray_Reads()
    {
        const string json = """["USD 1.5",{"amount":2,"currency":"EUR"},"USD 0.50"]""";

        foreach (var format in new[] { MonetaryJsonFormat.Object, MonetaryJsonFormat.Text })
        {
            var options = new JsonSerializerOptions { Converters = { new MoneyBagJsonConverterFactory(format) } };
            var bag = JsonSerializer.Deserialize<MoneyBag>(json, options)!;

            bag["USD"].ShouldBe(new MonetaryValue(2m, "USD"));
            bag["EUR"].ShouldBe(new MonetaryValue(2m, "EUR"));
        }
    }

    [TestMethod]
    public void CustomRegistryAndTextForm()
    {
        var options = new JsonSerializerOptions { Converters = { new MoneyBagJsonConverterFactory(TestRegistry, MonetaryJsonFormat.Text) } };

        var bag = JsonSerializer.Deserialize<ImmutableMoneyBag>("""["USD 1.00",{"amount":2,"currency":"EUR"}]""", options)!;
        bag.Registry.ShouldBeSameAs(TestRegistry);
        bag[TestUsd].Amount.ShouldBe(1m);
        bag[TestEur].Amount.ShouldBe(2m);

        JsonSerializer.Serialize(bag, options).ShouldContain("\"USD 1.00\"");

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ImmutableMoneyBag>("""["CAD 1.00"]""", options))
            .Message.ShouldBe("Currency code 'CAD' was not found in the 'Test' currency registry.");
    }

    [TestMethod]
    public void CultureFormattedElement_Throws()
    {
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MoneyBag>("""["$12.30"]"""))
            .Message.ShouldBe("The string '$12.30' is not a monetary value in the round-trip format, such as \"USD 12.34\".");
    }

    [TestMethod]
    public void NumberHandling_PassesThroughToEntries()
    {
        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString };
        var bag = new SortedMoneyBag { new(12.3m, "USD") };

        string json = JsonSerializer.Serialize(bag, options);
        json.ShouldBe("""[{"amount":"12.30","currency":"USD"}]""");
        JsonSerializer.Deserialize<SortedMoneyBag>(json, options).ShouldBe(bag);
    }

    [TestMethod]
    public void DefaultValueElements_AddNothing()
    {
        // Constructing a bag from values skips the default value, so its two spellings add nothing to a bag.

        var bag = JsonSerializer.Deserialize<MoneyBag>("""["USD 1.00",{"amount":0,"currency":null},"0"]""")!;
        bag.Count.ShouldBe(1);
        bag["USD"].ShouldBe(new MonetaryValue(1m, "USD"));
    }

    [TestMethod]
    public void EqualBags_WriteIdenticalJson()
    {
        var a = new SortedMoneyBag { new(12.3m, "USD"), new(1250m, "JPY") };
        var b = new SortedMoneyBag { new(12.3000m, "USD"), new(1250.00m, "JPY") };

        JsonSerializer.Serialize(a).ShouldBe(JsonSerializer.Serialize(b));
    }
}
