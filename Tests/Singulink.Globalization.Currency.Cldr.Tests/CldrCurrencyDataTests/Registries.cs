namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class Registries
{
    [TestMethod]
    public void AllCurrenciesAreOrderedAndUnique()
    {
        var all = CldrCurrencyData.AllCurrencies;

        all.Length.ShouldBeGreaterThan(250);
        all.Select(c => c.CurrencyCode).ShouldBe(all.Select(c => c.CurrencyCode).OrderBy(c => c, StringComparer.Ordinal));
        all.Select(c => c.CurrencyCode).Distinct().Count().ShouldBe(all.Length);
        all.ShouldAllBe(c => c.CurrencyCode.Length == 3);
    }

    [TestMethod]
    public void DefaultRegistryContainsCurrentTenderOnly()
    {
        var registry = CldrCurrencyData.Registry;

        registry.Name.ShouldBe("CLDR");
        registry.Count.ShouldBeGreaterThan(140);
        registry.ShouldAllBe(c => CldrCurrencyData.GetCurrencyType(c) == CldrCurrencyTypes.CurrentTender);

        registry.Contains("USD").ShouldBeTrue();
        registry.Contains("EUR").ShouldBeTrue();
        registry.Contains("JPY").ShouldBeTrue();
        registry.Contains("XAU").ShouldBeFalse(); // Gold, not tender
        registry.Contains("DEM").ShouldBeFalse(); // German mark, historical
        registry.Contains("XXX").ShouldBeFalse(); // No currency
    }

    [TestMethod]
    public void RegistryIsCached()
    {
        CldrCurrencyData.Registry.ShouldBeSameAs(CldrCurrencyData.Registry);
    }

    [TestMethod]
    public void CreateRegistryWithTypes()
    {
        var nonTender = CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.CurrentNonTender);
        nonTender.Contains("XAU").ShouldBeTrue();
        nonTender.Contains("XDR").ShouldBeTrue();
        nonTender.Contains("USD").ShouldBeFalse();

        var historical = CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.Historical);
        historical.Contains("DEM").ShouldBeTrue();
        historical.Contains("FRF").ShouldBeTrue();
        historical.Contains("USD").ShouldBeFalse();

        var all = CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.All);
        all.Count.ShouldBe(CldrCurrencyData.AllCurrencies.Length);
        all.Count.ShouldBe(CldrCurrencyData.Registry.Count + nonTender.Count + historical.Count);

        Should.Throw<ArgumentException>(() => CldrCurrencyData.CreateRegistry(default));
    }

    [TestMethod]
    public void RegistriesShareCurrencyInstances()
    {
        CldrCurrencyData.CreateRegistry(CldrCurrencyTypes.All)["USD"].ShouldBeSameAs(CldrCurrencyData.Registry["USD"]);
        CldrCurrencyData.AllCurrencies.Single(c => c.CurrencyCode == "USD").ShouldBeSameAs(CldrCurrencyData.Registry["USD"]);
    }

    [TestMethod]
    public void GetCurrencyTypeRejectsForeignCurrency()
    {
        Should.Throw<ArgumentException>(() => CldrCurrencyData.GetCurrencyType(new Currency("ABC", "Test")));
    }

    [TestMethod]
    public void CurrencyCodesAndSymbolsAreParsable()
    {
        foreach (var currency in CldrCurrencyData.Registry)
        {
            Currency.IsSymbolOrCodeParsable(currency.CurrencyCode, out string? error).ShouldBeTrue(error);
            Currency.IsSymbolOrCodeParsable(currency.Symbol, out error).ShouldBeTrue($"{currency.CurrencyCode}: {error}");
        }
    }

    [TestMethod]
    public void ParseAndFormatRoundTrip()
    {
        var registry = CldrCurrencyData.Registry;
        var value = new MonetaryValue(1234.56m, registry["CAD"]);

        string s = value.ToString("C", CultureInfo.GetCultureInfo("en-CA"));
        registry.TryParseMoney(s, MonetaryStyles.CurrencyCode | MonetaryStyles.AllowLocalSymbol, CultureInfo.GetCultureInfo("en-CA"), out var parsed).ShouldBeTrue();
        parsed.ShouldBe(value);
    }

    [TestMethod]
    public void CldrVersionIsSet()
    {
        CldrCurrencyData.CldrVersion.ShouldMatch(@"^\d+(\.\d+)*$");
    }
}
