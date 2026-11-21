using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class Forms
{
    private static readonly Currency TestUsd = new("USD", "Test Dollar");
    private static readonly CurrencyRegistry TestRegistry = new("Test", [TestUsd]);

    private static readonly JsonSerializerOptions ObjectOptions = new() { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Object) } };
    private static readonly JsonSerializerOptions TextOptions = new() { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text) } };

    [TestMethod]
    public void BothForms_Write()
    {
        var value = new MonetaryValue(12.3m, "USD");

        JsonSerializer.Serialize(value).ShouldBe("""{"amount":12.30,"currency":"USD"}""");
        JsonSerializer.Serialize(value, ObjectOptions).ShouldBe("""{"amount":12.30,"currency":"USD"}""");
        JsonSerializer.Serialize(value, TextOptions).ShouldBe("\"USD 12.30\"");
        JsonSerializer.Serialize(new MonetaryValue(-0.5m, "USD"), TextOptions).ShouldBe("\"USD -0.50\"");
        JsonSerializer.Serialize(new MonetaryValue(1250m, "JPY"), TextOptions).ShouldBe("\"JPY 1250\"");
    }

    [TestMethod]
    public void EqualValues_WriteIdenticalJson()
    {
        MonetaryValue[] equal = [new(12.3m, "USD"), new(12.30m, "USD"), new(12.3000m, "USD")];
        var numberAsString = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString };

        foreach (var value in equal)
        {
            JsonSerializer.Serialize(value).ShouldBe("""{"amount":12.30,"currency":"USD"}""");
            JsonSerializer.Serialize(value, TextOptions).ShouldBe("\"USD 12.30\"");
            JsonSerializer.Serialize(value, numberAsString).ShouldBe("""{"amount":"12.30","currency":"USD"}""");
            JsonSerializer.Serialize(new Dictionary<MonetaryValue, int> { [value] = 1 }).ShouldBe("""{"USD 12.30":1}""");
        }
    }

    [TestMethod]
    public void Decimals_AreTheCurrencysOrMoreWhenNeeded()
    {
        JsonSerializer.Serialize(new MonetaryValue(5m, "JPY")).ShouldBe("""{"amount":5,"currency":"JPY"}""");
        JsonSerializer.Serialize(new MonetaryValue(5.25m, "JPY")).ShouldBe("""{"amount":5.25,"currency":"JPY"}""");
        JsonSerializer.Serialize(new MonetaryValue(5m, "BHD")).ShouldBe("""{"amount":5.000,"currency":"BHD"}""");
        JsonSerializer.Serialize(new MonetaryValue(5.0001m, "BHD")).ShouldBe("""{"amount":5.0001,"currency":"BHD"}""");
        JsonSerializer.Serialize(new MonetaryValue(5.125m, "USD")).ShouldBe("""{"amount":5.125,"currency":"USD"}""");
    }

    [TestMethod]
    public void EachForm_ReadsByConverterSetToTheOther()
    {
        var value = new MonetaryValue(12.3m, "USD");

        JsonSerializer.Deserialize<MonetaryValue>(JsonSerializer.Serialize(value, TextOptions), ObjectOptions).ShouldBe(value);
        JsonSerializer.Deserialize<MonetaryValue>(JsonSerializer.Serialize(value, ObjectOptions), TextOptions).ShouldBe(value);
        JsonSerializer.Deserialize<MonetaryValue>("\"USD 12.3\"").ShouldBe(value);
    }

    [TestMethod]
    [DataRow("\"USD 12.3\"")]
    [DataRow("\"USD 12.30\"")]
    [DataRow("\"USD 12.3000\"")]
    [DataRow("""{"amount":12.3,"currency":"USD"}""")]
    [DataRow("""{"amount":12.3000,"currency":"USD"}""")]
    public void Reading_AcceptsAnyNumberOfDecimals(string json)
    {
        JsonSerializer.Deserialize<MonetaryValue>(json, TextOptions).ShouldBe(new MonetaryValue(12.3m, "USD"));
        JsonSerializer.Deserialize<MonetaryValue>(json, ObjectOptions).ShouldBe(new MonetaryValue(12.3m, "USD"));
    }

    [TestMethod]
    public void DefaultValue_InBothForms()
    {
        JsonSerializer.Serialize(MonetaryValue.Default, ObjectOptions).ShouldBe("""{"amount":0,"currency":null}""");
        JsonSerializer.Serialize(MonetaryValue.Default, TextOptions).ShouldBe("\"0\"");

        foreach (var options in new[] { ObjectOptions, TextOptions })
        {
            JsonSerializer.Deserialize<MonetaryValue>("""{"amount":0,"currency":null}""", options).IsDefault.ShouldBeTrue();
            JsonSerializer.Deserialize<MonetaryValue>("""{"currency":null,"amount":0.00}""", options).IsDefault.ShouldBeTrue();
            JsonSerializer.Deserialize<MonetaryValue>("\"0\"", options).IsDefault.ShouldBeTrue();
            JsonSerializer.Deserialize<MonetaryValue>("\"0.00\"", options).IsDefault.ShouldBeTrue();
        }

        JsonSerializer.Deserialize<Dictionary<MonetaryValue, int>>("""{"0.00":1}""")!.Keys.Single().IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void Null_IsNotTheDefaultValue()
    {
        foreach (var options in new[] { ObjectOptions, TextOptions })
        {
            Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("null", options));

            JsonSerializer.Serialize<MonetaryValue?>(null, options).ShouldBe("null");
            JsonSerializer.Deserialize<MonetaryValue?>("null", options).ShouldBeNull();

            MonetaryValue? holdsDefault = MonetaryValue.Default;
            var result = JsonSerializer.Deserialize<MonetaryValue?>(JsonSerializer.Serialize(holdsDefault, options), options);
            result.ShouldNotBeNull();
            result.Value.IsDefault.ShouldBeTrue();
        }
    }

    [TestMethod]
    public void NullCurrency_WithNonZeroAmount_Throws()
    {
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":5,"currency":null}"""))
            .Message.ShouldBe("The monetary value has a null 'currency' and a non-zero amount. Only a zero amount can have no currency.");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":0}"""))
            .Message.ShouldBe("The monetary value is missing the 'currency' property.");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"currency":null}"""))
            .Message.ShouldBe("The monetary value is missing the 'amount' property.");
    }

    [TestMethod]
    public void DefaultValue_NumberHandlingAndNamingPolicy()
    {
        var numberOptions = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString };
        JsonSerializer.Serialize(MonetaryValue.Default, numberOptions).ShouldBe("""{"amount":"0","currency":null}""");
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":"0","currency":null}""", numberOptions).IsDefault.ShouldBeTrue();

        var namingOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper };
        JsonSerializer.Serialize(MonetaryValue.Default, namingOptions).ShouldBe("""{"AMOUNT":0,"CURRENCY":null}""");
        JsonSerializer.Deserialize<MonetaryValue>("""{"AMOUNT":0,"CURRENCY":null}""", namingOptions).IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void IgnoreConditions()
    {
        var whenDefault = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
        var whenNull = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        var empty = new Holder { Value = MonetaryValue.Default, Nullable = null };
        var zeros = new Holder { Value = MonetaryValue.Default, Nullable = MonetaryValue.Default };

        // WhenWritingDefault leaves out the default value, and a nullable value that is null.
        JsonSerializer.Serialize(empty, whenDefault).ShouldBe("{}");
        JsonSerializer.Serialize(zeros, whenDefault).ShouldBe("""{"Nullable":{"amount":0,"currency":null}}""");

        // WhenWritingNull leaves out only a nullable value that is null; the default value is not null.
        JsonSerializer.Serialize(empty, whenNull).ShouldBe("""{"Value":{"amount":0,"currency":null}}""");
        JsonSerializer.Serialize(zeros, whenNull).ShouldBe("""{"Value":{"amount":0,"currency":null},"Nullable":{"amount":0,"currency":null}}""");
    }

    public class Holder
    {
        public MonetaryValue Value { get; set; }

        public MonetaryValue? Nullable { get; set; }
    }

    [TestMethod]
    public void CustomRegistry_ReadsStringForm()
    {
        var options = new JsonSerializerOptions { Converters = { new MonetaryValueJsonConverter(TestRegistry, MonetaryJsonFormat.Text) } };

        JsonSerializer.Deserialize<MonetaryValue>("\"USD 1.00\"", options).Currency.ShouldBeSameAs(TestUsd);
        JsonSerializer.Serialize(new MonetaryValue(1m, TestUsd), options).ShouldBe("\"USD 1.00\"");

        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("\"EUR 1.00\"", options));
        ex.Message.ShouldBe("Currency code 'EUR' was not found in the 'Test' currency registry.");
    }

    [TestMethod]
    [DataRow("$12.34")]
    [DataRow("USD 1,234.50")]
    [DataRow("12.34 USD")]
    [DataRow("USD +12.34")]
    [DataRow("USD 12,34")]
    [DataRow("USD  12.34")]
    [DataRow(" USD 12.34")]
    [DataRow("USD 12.34 ")]
    [DataRow("USD12.34")]
    [DataRow("USD 12.")]
    [DataRow("USD .5")]
    [DataRow("USD (12.34)")]
    [DataRow("USD 1e3")]
    [DataRow("10")]
    [DataRow("")]
    public void CultureFormattedString_Throws(string text)
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>(JsonSerializer.Serialize(text)));
        ex.Message.ShouldBe($"The string '{text}' is not a monetary value in the round-trip format, such as \"USD 12.34\".");
    }

    [TestMethod]
    public void UnknownCodeInString_Throws()
    {
        var ex = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("\"ZZZ 1.00\""));
        ex.Message.ShouldStartWith("Currency code 'ZZZ' was not found in the '");
    }

    [TestMethod]
    public void NumberHandling_WriteAsString()
    {
        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString };

        JsonSerializer.Serialize(new MonetaryValue(12.3m, "USD"), options).ShouldBe("""{"amount":"12.30","currency":"USD"}""");
        JsonSerializer.Serialize(new MonetaryValue(-1250m, "JPY"), options).ShouldBe("""{"amount":"-1250","currency":"JPY"}""");

        // The text form is a string anyway.
        var textOptions = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString, Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text) } };
        JsonSerializer.Serialize(new MonetaryValue(12.3m, "USD"), textOptions).ShouldBe("\"USD 12.30\"");
    }

    [TestMethod]
    public void NumberHandling_AllowReadingFromString()
    {
        const string json = """{"amount":"12.30","currency":"USD"}""";

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>(json))
            .Message.ShouldBe("Expected a number for the 'amount' property but found a token of type 'String'.");

        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        JsonSerializer.Deserialize<MonetaryValue>(json, options).ShouldBe(new MonetaryValue(12.3m, "USD"));
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":"-1.5e2","currency":"USD"}""", options).ShouldBe(new MonetaryValue(-150m, "USD"));
        JsonSerializer.Deserialize<MonetaryValue>("""{"amount":12.30,"currency":"USD"}""", options).ShouldBe(new MonetaryValue(12.3m, "USD"));

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":"12,30","currency":"USD"}""", options));

        // The web defaults, which ASP.NET Core uses, include AllowReadingFromString.
        JsonSerializer.Deserialize<MonetaryValue>(json, JsonSerializerOptions.Web).ShouldBe(new MonetaryValue(12.3m, "USD"));
    }

    [TestMethod]
    public void NumberHandling_FloatingPointLiterals_DoNotApply()
    {
        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString };

        JsonSerializer.Serialize(new MonetaryValue(12.3m, "USD"), options).ShouldBe("""{"amount":12.30,"currency":"USD"}""");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":"NaN","currency":"USD"}""", options));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":"Infinity","currency":"USD"}""", options));
    }

    [TestMethod]
    public void NumberHandling_AttributeOnTypeDoesNotReachConverter()
    {
        // A [JsonNumberHandling] on a type applies to its number members only, and the converter sees only the options.

        JsonSerializer.Serialize(new TypeWithNumberHandling { Price = new MonetaryValue(1m, "USD"), Count = 3 })
            .ShouldBe("""{"Price":{"amount":1.00,"currency":"USD"},"Count":"3"}""");
    }

    [TestMethod]
    public void NumberHandling_AttributeOnPropertyIsRejectedBySerializer()
    {
        Should.Throw<InvalidOperationException>(() => JsonSerializer.Serialize(new PropertyWithNumberHandling { Price = new MonetaryValue(1m, "USD") }));
    }

    [TestMethod]
    public void NamingPolicy_DoesNotAffectTextForm()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper, Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text) } };

        JsonSerializer.Serialize(new MonetaryValue(1m, "USD"), options).ShouldBe("\"USD 1.00\"");
        JsonSerializer.Deserialize<MonetaryValue>("""{"AMOUNT":1,"CURRENCY":"USD"}""", options).ShouldBe(new MonetaryValue(1m, "USD"));
    }

    [TestMethod]
    public void DictionaryKeys_RoundTripInBothForms()
    {
        var values = new Dictionary<MonetaryValue, string> {
            [new MonetaryValue(12.3m, "USD")] = "a",
            [new MonetaryValue(-5m, "JPY")] = "b",
            [MonetaryValue.Default] = "c",
        };

        foreach (var options in new[] { ObjectOptions, TextOptions })
        {
            string json = JsonSerializer.Serialize(values, options);
            json.ShouldBe("""{"USD 12.30":"a","JPY -5":"b","0":"c"}""");

            var result = JsonSerializer.Deserialize<Dictionary<MonetaryValue, string>>(json, options)!;
            result.Count.ShouldBe(3);
            result[new MonetaryValue(12.3m, "USD")].ShouldBe("a");
            result[new MonetaryValue(-5m, "JPY")].ShouldBe("b");
            result[MonetaryValue.Default].ShouldBe("c");
        }

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Dictionary<MonetaryValue, int>>("""{"$12.30":1}"""));
    }

    [TestMethod]
    public void UndefinedFormat_Throws()
    {
        Should.Throw<ArgumentException>(() => new MonetaryValueJsonConverter((MonetaryJsonFormat)5));
        Should.Throw<ArgumentException>(() => new MonetaryValueJsonConverter(TestRegistry, (MonetaryJsonFormat)5));
        Should.Throw<ArgumentException>(() => new MoneyBagJsonConverterFactory((MonetaryJsonFormat)5));
    }

    public class PropertyWithNumberHandling
    {
        [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
        public MonetaryValue Price { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public class TypeWithNumberHandling
    {
        public MonetaryValue Price { get; set; }

        public int Count { get; set; }
    }
}
