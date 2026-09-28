namespace Singulink.Globalization.Tests.CldrCurrencyDataTests;

// The default registry is process-wide, so registration is exercised once at assembly initialization and the tests verify the outcome.

[PrefixTestClass]
public class RegisterAsDefault
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context)
    {
        CldrCurrencyData.RegisterAsDefault();
    }

    [TestMethod]
    public void DefaultRegistryIsCldrRegistry()
    {
        CurrencyRegistry.Default.ShouldBeSameAs(CldrCurrencyData.Registry);
        Currency.GetCurrency("CAD").ShouldBeSameAs(CldrCurrencyData.Registry["CAD"]);
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
        Should.Throw<InvalidOperationException>(() => CldrCurrencyData.RegisterAsDefault());
        Should.Throw<InvalidOperationException>(() => CurrencyRegistry.SetDefault(CldrCurrencyData.Registry));
    }

    [TestMethod]
    public void LocalCurrencyLookupWorks()
    {
        Currency.TryGetLocalCurrency(CultureInfo.GetCultureInfo("en-CA"), out var currency).ShouldBeTrue();
        currency.CurrencyCode.ShouldBe("CAD");
    }
}
