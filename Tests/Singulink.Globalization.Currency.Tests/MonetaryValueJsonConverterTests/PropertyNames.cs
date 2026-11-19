using System.Text.Json;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class PropertyNames
{
    private static readonly MonetaryValue Value = new(12.30m, "USD");

    [TestMethod]
    public void DefaultOptions_NamesAreCaseSensitive()
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"Amount":12.30,"Currency":"USD"}"""));
        ex.Message.ShouldContain("'amount'");
    }

    [TestMethod]
    public void CaseInsensitiveOptions_ReadsAnyCase()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        JsonSerializer.Deserialize<MonetaryValue>("""{"AMOUNT":12.30,"Currency":"USD"}""", options).ShouldBe(Value);
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":12.30,"currency":"USD"}""", options).ShouldBe(Value);
    }

    [TestMethod]
    public void WebDefaults_UseCamelCaseNames()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        string json = JsonSerializer.Serialize(Value, options);
        json.ShouldBe("""{"amount":12.30,"currency":"USD"}""");
        JsonSerializer.Deserialize<MonetaryValue>("""{"Amount":12.30,"CURRENCY":"USD"}""", options).ShouldBe(Value);
    }

    [TestMethod]
    [DataRow("snake_case_upper", """{"AMOUNT":12.30,"CURRENCY":"USD"}""")]
    [DataRow("kebab-case-lower", """{"amount":12.30,"currency":"USD"}""")]
    [DataRow("prefix", """{"xAmount":12.30,"xCurrency":"USD"}""")]
    public void NamingPolicy_IsAppliedOnWriteAndRead(string policyName, string expectedJson)
    {
        var options = new JsonSerializerOptions {
            PropertyNamingPolicy = policyName switch {
                "snake_case_upper" => JsonNamingPolicy.SnakeCaseUpper,
                "kebab-case-lower" => JsonNamingPolicy.KebabCaseLower,
                _ => new PrefixNamingPolicy(),
            },
        };

        string json = JsonSerializer.Serialize(Value, options);
        json.ShouldBe(expectedJson);
        JsonSerializer.Deserialize<MonetaryValue>(json, options).ShouldBe(Value);
    }

    [TestMethod]
    public void NamingPolicy_DefaultNamesAreUnknown()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = new PrefixNamingPolicy() };

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":12.30,"currency":"USD"}""", options));
        ex.Message.ShouldContain("'xAmount'");
    }

    [TestMethod]
    public void NamingPolicy_SameConverterWithTwoOptions_UsesEachOptionsNames()
    {
        var converter = new MonetaryValueJsonConverter();
        var prefixOptions = new JsonSerializerOptions { PropertyNamingPolicy = new PrefixNamingPolicy(), Converters = { converter } };
        var upperOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper, Converters = { converter } };
        var plainOptions = new JsonSerializerOptions { Converters = { converter } };

        JsonSerializer.Serialize(Value, prefixOptions).ShouldBe("""{"xAmount":12.30,"xCurrency":"USD"}""");
        JsonSerializer.Serialize(Value, upperOptions).ShouldBe("""{"AMOUNT":12.30,"CURRENCY":"USD"}""");
        JsonSerializer.Serialize(Value, plainOptions).ShouldBe("""{"amount":12.30,"currency":"USD"}""");
        JsonSerializer.Serialize(Value, prefixOptions).ShouldBe("""{"xAmount":12.30,"xCurrency":"USD"}""");
    }

    private sealed class PrefixNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => "x" + name;
    }
}
