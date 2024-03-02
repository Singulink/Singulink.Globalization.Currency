using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class RemoveAllTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = [_usd100, _cad50, _eur25];
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    // public int RemoveAll(IEnumerable<Currency> currencies) tests

    [TestMethod]
    public void AllExistingCurrenciesFromSet_IsSuccessful()
    {
        List<Currency> currencyList = [_usd100.Currency, _cad50.Currency, _eur25.Currency];
        int removedValuesCount = _set.RemoveAll(currencyList);
        removedValuesCount.ShouldBe(3);
        _set.Count.ShouldBe(0);
    }

    [TestMethod]
    public void SomeExistingCurrenciesFromSet_IsSuccessful()
    {
        List<Currency> currencyList = [_usd100.Currency, _cad50.Currency];
        int removedValuesCount = _set.RemoveAll(currencyList);
        removedValuesCount.ShouldBe(2);
        _set.Count.ShouldBe(1);
        _set.ShouldBe(new[] { _eur25 });
    }

    [TestMethod]
    public void NoExistingCurrenciesFromSet_IsSuccessful()
    {
        List<Currency> currencyList = [Currency.Get("JPY"), Currency.Get("GBP")];
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
    public void NonExistentCurrency_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("XXX", "Non-existent currency", "X", 2);
        List<Currency> currencyList = [_usd100.Currency, disallowedCurrency, disallowedCurrency, _cad50.Currency];

        Should.Throw<ArgumentException>(() => _set.RemoveAll(currencyList))
            .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {disallowedCurrency} (Parameter 'currencies')");
        _set.Count.ShouldBe(1);
        _set.ShouldBe(new[] { _eur25 });
    }

    [TestMethod]
    public void NonExistentCurrencies_ThrowsArgumentException()
    {
        var disallowedCurrencyX = new Currency("XXX", "Non-existent currency", "X", 2);
        var disallowedCurrencyY = new Currency("YYY", "Non-existent currency", "Y", 2);
        List<Currency> currencyList = [_usd100.Currency, disallowedCurrencyX, disallowedCurrencyX, disallowedCurrencyY, disallowedCurrencyY, _cad50.Currency];

        Should.Throw<ArgumentException>(() => _set.RemoveAll(currencyList))
            .Message.ShouldBe($"The following currencies are not present in the set's currency registry: {disallowedCurrencyX}, {disallowedCurrencyY} (Parameter 'currencies')");
        _set.Count.ShouldBe(1);
        _set.ShouldBe(new[] { _eur25 });
    }

    // public SortedMoneySet RemoveAll(Func<Money, bool> predicate) tests

    [TestMethod]
    public void RemoveAllByPredicate_IsSuccessful()
    {
        _set.RemoveAll(m => m.Amount > 30);
        _set.Count.ShouldBe(1);
        _set.ShouldBe(new[] { _eur25 });
    }
}