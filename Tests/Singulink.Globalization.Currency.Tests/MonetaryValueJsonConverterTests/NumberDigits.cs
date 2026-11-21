using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization.Tests.MonetaryValueJsonConverterTests;

[PrefixTestClass]
public class NumberDigits
{
    private static readonly JsonSerializerOptions ObjectOptions = new() { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Object) } };
    private static readonly JsonSerializerOptions LossyOptions = new() { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.LossyNumberObject) } };
    private static readonly JsonSerializerOptions TextOptions = new() { Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.Text) } };

    [TestMethod]
    [DataRow("0", 0)]
    [DataRow("0.00", 0)]
    [DataRow("12.30", 3)]
    [DataRow("-12.30", 3)]
    [DataRow("0.000123", 3)]
    [DataRow("1000000000000000000", 1)]
    [DataRow("1234567890123.4", 14)]
    [DataRow("123456789012345", 15)]
    [DataRow("1234567890123456", 16)]
    [DataRow("1234567890.12345", 15)]
    [DataRow("1234567890.123456", 16)]
    [DataRow("0.1234567890123456", 16)]
    [DataRow("79228162514264337593543950335", 29)]
    [DataRow("-79228162514264337593543950335", 29)]
    [DataRow("0.0000000000000000000000000001", 1)]
    [DataRow("7.9228162514264337593543950335", 29)]
    public void SignificantDigits_AreCounted(string amount, int digits)
    {
        // An amount over the limit reports its count in the message; one at or under the limit is written.

        var value = new MonetaryValue(decimal.Parse(amount, CultureInfo.InvariantCulture), "USD");

        if (digits > 15)
            Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(value)).Message.ShouldContain($" has {digits} significant digits,");
        else
            Should.NotThrow(() => JsonSerializer.Serialize(value));
    }

    [TestMethod]
    [DataRow("123456789012345")]
    [DataRow("-1234567890.12345")]
    [DataRow("1234567890123.4")]
    [DataRow("1000000000000000000")]
    [DataRow("0.0000000000000000000000000001")]
    [DataRow("0.000123")]
    public void UpTo15Digits_WritesTheSameInObjectAndLossy(string amount)
    {
        var value = new MonetaryValue(decimal.Parse(amount, CultureInfo.InvariantCulture), "USD");

        string json = JsonSerializer.Serialize(value, ObjectOptions);
        JsonSerializer.Serialize(value, LossyOptions).ShouldBe(json);
        JsonSerializer.Serialize(value).ShouldBe(json);
        JsonSerializer.Deserialize<MonetaryValue>(json).ShouldBe(value);
    }

    [TestMethod]
    [DataRow("1234567890123456")]
    [DataRow("-1234567890123456")]
    [DataRow("1234567890.123456")]
    [DataRow("0.1234567890123456")]
    [DataRow("79228162514264337593543950335")]
    [DataRow("-79228162514264337593543950335")]
    [DataRow("7.9228162514264337593543950335")]
    public void Over15Digits_ObjectThrows_LossyWritesEveryDigit(string amount)
    {
        var value = new MonetaryValue(decimal.Parse(amount, CultureInfo.InvariantCulture), "USD");

        Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(value));
        Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(value, ObjectOptions));

        string json = JsonSerializer.Serialize(value, LossyOptions);
        json.ShouldBe($$"""{"amount":{{value.ToString("O", null)[4..]}},"currency":"USD"}""");

        // This library's reader keeps every digit.
        JsonSerializer.Deserialize<MonetaryValue>(json).ShouldBe(value);
        JsonSerializer.Deserialize<MonetaryValue>(json, ObjectOptions).ShouldBe(value);
        JsonSerializer.Deserialize<MonetaryValue>(json, TextOptions).ShouldBe(value);
    }

    [TestMethod]
    public void Over15Digits_OtherWritesAreNotLimited()
    {
        var value = new MonetaryValue(10m / 3, "USD");
        var asString = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString };
        var lossyAsString = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.WriteAsString, Converters = { new MonetaryValueJsonConverter(MonetaryJsonFormat.LossyNumberObject) } };

        JsonSerializer.Serialize(value, asString).ShouldBe("""{"amount":"3.3333333333333333333333333333","currency":"USD"}""");
        JsonSerializer.Serialize(value, lossyAsString).ShouldBe("""{"amount":"3.3333333333333333333333333333","currency":"USD"}""");
        JsonSerializer.Serialize(value, TextOptions).ShouldBe("\"USD 3.3333333333333333333333333333\"");
        JsonSerializer.Serialize(new Dictionary<MonetaryValue, int> { [value] = 1 }).ShouldBe("""{"USD 3.3333333333333333333333333333":1}""");
        System.ComponentModel.TypeDescriptor.GetConverter(typeof(MonetaryValue)).ConvertToInvariantString(value).ShouldBe("USD 3.3333333333333333333333333333");
    }

    [TestMethod]
    public void Message_SaysWhatToDo()
    {
        var ex = Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(new MonetaryValue(10m / 3, "USD")));

        ex.Message.ShouldStartWith(
            "The amount of USD 3.3333333333333333333333333333 has 29 significant digits, but a JSON reader that uses double-precision numbers, such as " +
            "JavaScript, keeps only 15, so it would read a different amount. Round the value; write amounts as strings with JsonNumberHandling.WriteAsString " +
            "or use MonetaryJsonFormat.Text; or use MonetaryJsonFormat.LossyNumberObject if every reader keeps all the digits of a number.");
    }

    [TestMethod]
    public void DefaultAndNegativeValues()
    {
        JsonSerializer.Serialize(MonetaryValue.Default, ObjectOptions).ShouldBe("""{"amount":0,"currency":null}""");
        JsonSerializer.Serialize(MonetaryValue.Default, LossyOptions).ShouldBe("""{"amount":0,"currency":null}""");
        JsonSerializer.Serialize(new MonetaryValue(-12.3m, "USD"), ObjectOptions).ShouldBe("""{"amount":-12.30,"currency":"USD"}""");
    }

    [TestMethod]
    public void Reading_NeverLimitsDigits()
    {
        const string json = """{"amount":3.3333333333333333333333333333,"currency":"USD"}""";

        foreach (var options in new[] { ObjectOptions, LossyOptions, TextOptions })
            JsonSerializer.Deserialize<MonetaryValue>(json, options).ShouldBe(new MonetaryValue(10m / 3, "USD"));
    }

    [TestMethod]
    public void Bag_ChecksEachValue()
    {
        var bag = new SortedMoneyBag { new(1m, "EUR"), new(10m / 3, "USD") };

        Should.Throw<ArgumentException>(() => JsonSerializer.Serialize(bag));

        var lossy = new JsonSerializerOptions { Converters = { new MoneyBagJsonConverterFactory(MonetaryJsonFormat.LossyNumberObject) } };
        string json = JsonSerializer.Serialize(bag, lossy);
        json.ShouldBe("""[{"amount":1.00,"currency":"EUR"},{"amount":3.3333333333333333333333333333,"currency":"USD"}]""");
        JsonSerializer.Deserialize<SortedMoneyBag>(json).ShouldBe(bag);

        bag.Round();
        JsonSerializer.Serialize(bag).ShouldBe("""[{"amount":1.00,"currency":"EUR"},{"amount":3.33,"currency":"USD"}]""");
    }
}
