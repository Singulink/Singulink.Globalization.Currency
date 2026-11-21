using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Singulink.Globalization.Tests.CurrencyRegistryTests;

[PrefixTestClass]
public class CodeNotFoundMessages
{
    private const string Message = "Currency code 'ZZZ' was not found in the 'System' currency registry.";

    [TestMethod]
    public void EveryLookup_ReportsTheSameMessage()
    {
        Should.Throw<ArgumentException>(() => CurrencyRegistry.Default["ZZZ"]).Message.ShouldStartWith(Message);
        Should.Throw<ArgumentException>(() => Currency.GetCurrency("ZZZ")).Message.ShouldStartWith(Message);
        Should.Throw<ArgumentException>(() => new MonetaryValue(1m, "ZZZ")).Message.ShouldStartWith(Message);
        Should.Throw<ArgumentException>(() => MonetaryValue.FromMinorUnits(1, "ZZZ")).Message.ShouldStartWith(Message);

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Currency>("\"ZZZ\"")).Message.ShouldBe(Message);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("\"ZZZ 1.00\"")).Message.ShouldBe(Message);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonetaryValue>("""{"amount":1,"currency":"ZZZ"}""")).Message.ShouldBe(Message);

        Should.Throw<ArgumentException>(() => TypeDescriptor.GetConverter(typeof(Currency)).ConvertFromInvariantString("ZZZ")).Message.ShouldContain(Message);
    }

    [TestMethod]
    public void Parsing_ListsTheFailedLookups()
    {
        Should.Throw<FormatException>(() => MonetaryValue.Parse("ZZZ 1.00", CultureInfo.InvariantCulture))
            .Message.ShouldBe("The input string 'ZZZ 1.00' had a currency indicator 'ZZZ' that could not be resolved. The following lookups were attempted but failed: Currency code: CodeNotFound");
    }

    [TestMethod]
    public void EmptyRegistry_NotBuiltFromSystemData_HasNoHint()
    {
        // Only the registry built from system data explains a missing code by invariant globalization mode.

        var empty = new CurrencyRegistry("System", []);

        var ex = Should.Throw<ArgumentException>(() => empty["USD"]);
        ex.Message.ShouldStartWith("Currency code 'USD' was not found in the 'System' currency registry.");
        ex.Message.ShouldNotContain("invariant");
        empty.TryParseMonetaryValue("USD 1", MonetaryStyles.CurrencyCode, CultureInfo.InvariantCulture, out _, out string error).ShouldBeFalse();
        error.ShouldEndWith("Currency code: CodeNotFound");
    }
}
