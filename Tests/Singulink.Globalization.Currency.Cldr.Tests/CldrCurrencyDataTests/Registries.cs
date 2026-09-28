namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class Registries
{
    private static CurrencyData Cldr => CurrencyData.Load(CldrCurrencyData.Provider);

    [TestMethod]
    public void AllCurrenciesAreOrderedAndUnique()
    {
        var all = Cldr.AllCurrencies;

        all.Length.ShouldBeGreaterThan(250);
        all.Select(c => c.CurrencyCode).ShouldBe(all.Select(c => c.CurrencyCode).OrderBy(c => c, StringComparer.Ordinal));
        all.Select(c => c.CurrencyCode).Distinct().Count().ShouldBe(all.Length);
        all.ShouldAllBe(c => c.CurrencyCode.Length == 3);
    }

    [TestMethod]
    public void DefaultRegistryContainsCurrentTenderOnly()
    {
        var registry = Cldr.Registry;

        registry.Name.ShouldBe("CLDR");
        registry.Count.ShouldBeGreaterThan(140);
        registry.ShouldAllBe(c => Cldr.GetCurrencyType(c) == CurrencyTypes.CurrentTender);

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
        Cldr.Registry.ShouldBeSameAs(Cldr.Registry);
    }

    [TestMethod]
    public void CreateRegistryWithTypes()
    {
        var nonTender = Cldr.CreateRegistry(CurrencyTypes.CurrentNonTender);
        nonTender.Contains("XAU").ShouldBeTrue();
        nonTender.Contains("XDR").ShouldBeTrue();
        nonTender.Contains("USD").ShouldBeFalse();

        var historical = Cldr.CreateRegistry(CurrencyTypes.Historical);
        historical.Contains("DEM").ShouldBeTrue();
        historical.Contains("FRF").ShouldBeTrue();
        historical.Contains("USD").ShouldBeFalse();

        var all = Cldr.CreateRegistry(CurrencyTypes.All);
        all.Count.ShouldBe(Cldr.AllCurrencies.Length);
        all.Count.ShouldBe(Cldr.Registry.Count + nonTender.Count + historical.Count);

        Should.Throw<ArgumentException>(() => Cldr.CreateRegistry(default));
    }

    [TestMethod]
    public void RegistriesShareCurrencyInstances()
    {
        Cldr.CreateRegistry(CurrencyTypes.All)["USD"].ShouldBeSameAs(Cldr.Registry["USD"]);
        Cldr.AllCurrencies.Single(c => c.CurrencyCode == "USD").ShouldBeSameAs(Cldr.Registry["USD"]);
    }

    [TestMethod]
    public void GetCurrencyTypeRejectsForeignCurrency()
    {
        Should.Throw<ArgumentException>(() => Cldr.GetCurrencyType(new Currency("ABC", "Test")));
    }

    [TestMethod]
    public void CurrencyCodesAndSymbolsAreParsable()
    {
        foreach (var currency in Cldr.Registry)
        {
            Currency.IsSymbolOrCodeParsable(currency.CurrencyCode, out string? error).ShouldBeTrue(error);
            Currency.IsSymbolOrCodeParsable(currency.Symbol, out error).ShouldBeTrue($"{currency.CurrencyCode}: {error}");
        }
    }

    [TestMethod]
    public void ParseAndFormatRoundTrip()
    {
        var registry = Cldr.Registry;
        var value = new MonetaryValue(1234.56m, registry["CAD"]);

        string s = value.ToString("C", CultureInfo.GetCultureInfo("en-CA"));
        registry.TryParseMonetaryValue(s, MonetaryStyles.CurrencyCodeOrLocalSymbol, CultureInfo.GetCultureInfo("en-CA"), out var parsed).ShouldBeTrue();
        parsed.ShouldBe(value);
    }
}
