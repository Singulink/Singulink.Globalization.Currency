using System.Text.Json;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class Registries
{
    // A registry with its own USD, which is a different currency than the default registry's USD.
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd, Common.CurrencyX]);

    [TestMethod]
    public void Attribute_ReadsFromDefaultRegistry()
    {
        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"USD"}""");
        result.Currency.ShouldBeSameAs(CurrencyRegistry.Default["USD"]);
    }

    [TestMethod]
    public void ParameterlessConstructor_ReadsFromDefaultRegistry()
    {
        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter() } };

        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"USD"}""", options);
        result.Currency.ShouldBeSameAs(CurrencyRegistry.Default["USD"]);
    }

    [TestMethod]
    public void OptionsConverter_ReadsFromItsRegistry_OverAttribute()
    {
        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(TestRegistry) } };

        var result = JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"USD"}""", options);
        result.Currency.ShouldBeSameAs(TestUsd);
        result.Currency.ShouldNotBeSameAs(CurrencyRegistry.Default["USD"]);

        var order = JsonSerializer.Deserialize<RoundTrip.Order>("""{"Price":{"amount":1,"currency":"USD"},"Discount":{"amount":2,"currency":"XXXX"}}""", options)!;
        order.Price.Currency.ShouldBeSameAs(TestUsd);
        order.Discount!.Value.Currency.ShouldBeSameAs(Common.CurrencyX);
    }

    [TestMethod]
    public void OptionsConverter_CodeNotInItsRegistry_Throws()
    {
        // EUR is in the default registry but not in the test registry, so reading it with the test registry fails.

        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(TestRegistry) } };

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"EUR"}""", options));
        ex.Message.ShouldBe("Currency code 'EUR' was not found in the 'Test' currency registry.");
        ex.Path.ShouldBe("$");
    }

    [TestMethod]
    public void Write_DoesNotUseRegistry()
    {
        // The value's currency is from the default registry, which the converter's registry does not contain. Its code is written regardless.

        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(TestRegistry) } };

        JsonSerializer.Serialize(new MonetaryValue(3m, "EUR"), options).ShouldBe("""{"amount":3,"currency":"EUR"}""");
        JsonSerializer.Serialize(new MonetaryValue(3m, Common.CurrencyY)).ShouldBe("""{"amount":3,"currency":"YYYY"}""");
    }
}
