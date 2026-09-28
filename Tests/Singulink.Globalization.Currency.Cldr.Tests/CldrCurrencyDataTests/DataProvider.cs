namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

[PrefixTestClass]
public class DataProvider
{
    [TestMethod]
    public void ProviderMetadata()
    {
        var provider = CldrCurrencyData.Provider;

        provider.Name.ShouldBe("CLDR");
        provider.Version.ShouldBe(CldrCurrencyData.CldrVersion);
        provider.Version.ShouldMatch(@"^\d+(\.\d+)*$");
        provider.ReleaseDate.ShouldBe(CldrCurrencyData.CldrReleaseDate);
        CldrCurrencyData.CldrReleaseDate.Kind.ShouldBe(DateTimeKind.Utc);
        CldrCurrencyData.CldrReleaseDate.ShouldBeGreaterThan(new DateTime(2025, 1, 1));
    }

    [TestMethod]
    public void ProviderEntries()
    {
        var currencies = CldrCurrencyData.Provider.GetCurrencies().ToList();
        var cad = currencies.Single(c => c.CurrencyCode == "CAD");

        cad.Name.ShouldBe("Canadian Dollar");
        cad.Symbol.ShouldBe("CA$");
        cad.DecimalDigits.ShouldBe(2);
        cad.DecimalUnits.ShouldBe(0);
        cad.CashDecimalDigits.ShouldBe(2);
        cad.CashDecimalUnits.ShouldBe(5);
        cad.Type.ShouldBe(CurrencyTypes.CurrentTender);

        string? kwdSymbol = currencies.Single(c => c.CurrencyCode == "KWD").Symbol;
        (kwdSymbol is null or "KWD").ShouldBeTrue(); // No distinct symbol in CLDR, code is used

        var localizations = CldrCurrencyData.Provider.GetLocalizations().ToList();
        localizations.ShouldNotContain(l => l.Locale.Length == 0);
        localizations.ShouldContain(l => l.Locale == "fr-CA" && l.CurrencyCode == "CAD" && l.Symbol == "$");
    }

    [TestMethod]
    public void LoadIsCachedPerProvider()
    {
        var cldr = CurrencyData.Load(CldrCurrencyData.Provider);

        cldr.ShouldBeSameAs(CurrencyData.Load(CldrCurrencyData.Provider));
        cldr.Provider.ShouldBeSameAs(CldrCurrencyData.Provider);

        // A different provider instance over the same data produces separate currency instances:
        var copy = new TestProvider(CldrCurrencyData.Provider.GetCurrencies, CldrCurrencyData.Provider.GetLocalizations);
        CurrencyData.Load(copy).ShouldNotBeSameAs(cldr);
        CurrencyData.Load(copy).Registry["USD"].ShouldNotBeSameAs(cldr.Registry["USD"]);
        CurrencyData.Load(copy).Registry.Name.ShouldBe("Test");
    }

    [TestMethod]
    public void CustomProvider()
    {
        var provider = new TestProvider(
            () => [
                new CurrencyDataEntry("AAA", "Currency A") { Symbol = "A", CashDecimalUnits = 5 },
                new CurrencyDataEntry("BBB", "Currency B") { DecimalDigits = 0, Type = CurrencyTypes.Historical },
            ],
            () => [
                new CurrencyLocalizationEntry("fr", "AAA") { Name = "Monnaie A" },
                new CurrencyLocalizationEntry("fr-CA", "AAA") { Symbol = "A$" },
            ]);

        var data = CurrencyData.Load(provider);
        data.AllCurrencies.Select(c => c.CurrencyCode).ShouldBe(["AAA", "BBB"]);

        var a = data.Registry["AAA"];
        a.Name.ShouldBe("Currency A");
        a.Symbol.ShouldBe("A");
        a.RoundingPolicy.ShouldBe(new RoundingPolicy(2));
        a.CashRoundingPolicy.ShouldBe(new RoundingPolicy(2, 5));
        a.GetLocalizedName(CultureInfo.GetCultureInfo("fr")).ShouldBe("Monnaie A");
        a.GetLocalizedName(CultureInfo.GetCultureInfo("fr-CA")).ShouldBe("Monnaie A"); // Inherited from fr
        a.GetLocalizedSymbol(CultureInfo.GetCultureInfo("fr")).ShouldBe("A");         // Inherited from invariant
        a.GetLocalizedSymbol(CultureInfo.GetCultureInfo("fr-CA")).ShouldBe("A$");
        a.GetLocalizedName(CultureInfo.GetCultureInfo("de")).ShouldBe("Currency A");

        data.Registry.Contains("BBB").ShouldBeFalse();
        var b = data.CreateRegistry(CurrencyTypes.Historical)["BBB"];
        b.Symbol.ShouldBe("BBB"); // No symbol, code is used
        b.RoundingPolicy.ShouldBe(new RoundingPolicy(0));
        b.CashRoundingPolicy.ShouldBe(new RoundingPolicy(0));
        data.GetCurrencyType(b).ShouldBe(CurrencyTypes.Historical);
    }

    [TestMethod]
    public void EmptyProviderLoads()
    {
        var data = CurrencyData.Load(new TestProvider(() => [], () => []));
        data.AllCurrencies.ShouldBeEmpty();
        data.Registry.Count.ShouldBe(0);
    }

    [TestMethod]
    public void InvalidDataThrows()
    {
        var duplicate = new TestProvider(() => [new("AAA", "A"), new("aaa", "A2")], () => []);
        Should.Throw<InvalidDataException>(() => CurrencyData.Load(duplicate)).Message.ShouldContain("Test 1.0");

        var unknownLocalization = new TestProvider(() => [new("AAA", "A")], () => [new("fr", "BBB") { Name = "B" }]);
        Should.Throw<InvalidDataException>(() => CurrencyData.Load(unknownLocalization)).Message.ShouldContain("BBB");

        var badDigits = new TestProvider(() => [new("AAA", "A") { DecimalDigits = 99 }], () => []);
        Should.Throw<InvalidDataException>(() => CurrencyData.Load(badDigits)).Message.ShouldContain("AAA");

        var badType = new TestProvider(() => [new("AAA", "A") { Type = CurrencyTypes.All }], () => []);
        Should.Throw<InvalidDataException>(() => CurrencyData.Load(badType));
    }

    [TestMethod]
    public void EntryValidation()
    {
        Should.Throw<ArgumentException>(() => new CurrencyDataEntry(string.Empty, "A"));
        Should.Throw<ArgumentException>(() => new CurrencyDataEntry("AAA", " "));
        Should.Throw<ArgumentException>(() => new CurrencyLocalizationEntry(string.Empty, "AAA"));
        Should.Throw<ArgumentException>(() => new CurrencyLocalizationEntry("fr", string.Empty));
    }

    private sealed class TestProvider(Func<IEnumerable<CurrencyDataEntry>> currencies, Func<IEnumerable<CurrencyLocalizationEntry>> localizations) : ICurrencyDataProvider
    {
        public string Name => "Test";

        public string Version => "1.0";

        public DateTime? ReleaseDate => null;

        public IEnumerable<CurrencyDataEntry> GetCurrencies() => currencies();

        public IEnumerable<CurrencyLocalizationEntry> GetLocalizations() => localizations();
    }
}
