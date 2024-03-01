using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class RemoveAllTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void AllExistingCurrenciesFromSet_IsSuccessful()
    {
        var currencyList = new List<Currency> { _usd100.Currency, _cad50.Currency, _eur25.Currency };
        int removedValuesCount = _set.RemoveAll(currencyList);
        removedValuesCount.ShouldBe(3);
        _set.Count.ShouldBe(0);
    }

    [TestMethod]
    public void SomeExistingCurrenciesFromSet_IsSuccessful()
    {
        var currencyList = new List<Currency> { _usd100.Currency, _cad50.Currency};
        int removedValuesCount = _set.RemoveAll(currencyList);
        removedValuesCount.ShouldBe(2);
        _set.Count.ShouldBe(1);
        _set.ShouldBe(new[] { _eur25 });
    }

    [TestMethod]
    public void NoExistingCurrenciesFromSet_IsSuccessful()
    {
        var currencyList = new List<Currency> { Currency.Get("JPY"), Currency.Get("GBP") };
        int removedValuesCount = _set.RemoveAll(currencyList);
        removedValuesCount.ShouldBe(0);
        _set.Count.ShouldBe(3);
        _set.ShouldBe(_immutableSet);
    }

    [TestMethod]
    public void EmptyCollection_NoChange()
    {
        int removedValuesCount = _set.RemoveAll(new List<Currency>());
        removedValuesCount.ShouldBe(0);
        _set.Count.ShouldBe(3);
        _set.ShouldBe(_immutableSet);
    }

    [TestMethod]
    public void InexistentCurrency_RemovesExistingAndIgnoresInexistent()
    {
        var nonExistentCurrency = new Currency("XXX", "Non-existent currency", "X", 2);
        var currencyList = new List<Currency> { _usd100.Currency, _cad50.Currency, nonExistentCurrency };
        Should.Throw<ArgumentException>(() => _set.RemoveAll(currencyList));
    }

    [TestMethod]
    public void InexistentCurrencies_ThrowsArgumentException()
    {
        var nonExistentCurrencyX = new Currency("XXX", "Non-existent currency", "X", 2);
        var nonExistentCurrencyY = new Currency("XXX", "Non-existent currency", "X", 2);
        var currencyList = new List<Currency> { _usd100.Currency, nonExistentCurrencyX, nonExistentCurrencyY };
        Should.Throw<ArgumentException>(() => _set.RemoveAll(currencyList));
    }
}