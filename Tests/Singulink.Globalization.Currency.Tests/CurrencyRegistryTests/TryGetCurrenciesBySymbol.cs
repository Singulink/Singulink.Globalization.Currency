using System.Globalization;

namespace Singulink.Globalization.Tests.CurrencyRegistryTests;

[PrefixTestClass]
public class TryGetCurrenciesBySymbol
{
    [TestMethod]
    public void SymbolNotFound()
    {
        bool found = CurrencyRegistry.Default.TryGetCurrenciesBySymbol("%", out var result);
        found.ShouldBeFalse();
        result.ShouldBe([]);
    }

    [TestMethod]
    public void SymbolFound()
    {
        bool found = CurrencyRegistry.Default.TryGetCurrenciesBySymbol("$", out var result);
        found.ShouldBeTrue();
        result.Count.ShouldBeGreaterThan(1);

        result.ShouldAllBe(c => c.GetLocalizedSymbol(CultureInfo.CurrentCulture) == "$");
    }
}
