namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

// The default registry is process-wide, so registration is exercised once at assembly initialization and the tests verify the outcome.

[PrefixTestClass]
public class RegisterAsDefault
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        CurrencyRegistry.SetDefault(CldrCurrencyData.Provider);
    }

    [TestMethod]
    public void DefaultRegistryIsCldrRegistry()
    {
        CurrencyRegistry.Default.ShouldBeSameAs(CurrencyData.Load(CldrCurrencyData.Provider).Registry);
        Currency.GetCurrency("CAD").ShouldBeSameAs(CurrencyData.Load(CldrCurrencyData.Provider).Registry["CAD"]);
    }

    [TestMethod]
    public void DefaultRegistryCurrenciesHaveCashRounding()
    {
        new MonetaryValue(10.03m, "CAD").RoundToCash().ShouldBe(new MonetaryValue(10.05m, "CAD"));
    }

    [TestMethod]
    public void RegisteringAfterDefaultIsCreatedThrows()
    {
        _ = CurrencyRegistry.Default;
        Should.Throw<InvalidOperationException>(() => CurrencyRegistry.SetDefault(CldrCurrencyData.Provider));
        Should.Throw<InvalidOperationException>(() => CurrencyRegistry.SetDefault(CurrencyData.Load(CldrCurrencyData.Provider).Registry));
    }

    [TestMethod]
    public void SetDefaultRejectsEmptyTypes()
    {
        Should.Throw<ArgumentException>(() => CurrencyRegistry.SetDefault(CldrCurrencyData.Provider, default));
    }

    [TestMethod]
    public void LocalCurrencyLookupWorks()
    {
        Currency.TryGetLocalCurrency(CultureInfo.GetCultureInfo("en-CA"), out var currency).ShouldBeTrue();
        currency.CurrencyCode.ShouldBe("CAD");
    }
}
