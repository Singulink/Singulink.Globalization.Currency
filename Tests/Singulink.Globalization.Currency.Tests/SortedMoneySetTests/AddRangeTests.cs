using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class AddRangeTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void AllCurrenciesExistInTheSet_IsSuccessful()
    {
        _set.AddRange([_usd100, _cad50, _eur25]);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void SomeCurrenciesDoNotExistInTheSet_IsSuccessful()
    {
        _set.AddRange([new(100m, "USD"), new(50m, "JPY"), new(25m, "CHF")]);
        _set.Count.ShouldBe(5);
        _set.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void NoCurrencyExistsInTheSet_IsSuccessful()
    {
        _set.AddRange([new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
        _set.Count.ShouldBe(6);
        _set.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void EmptyCollection_NoChange()
    {
        _set.AddRange(new List<Money>());
        _set.Count.ShouldBe(3);
        _set.ShouldBe(_immutableSet);
    }

    [TestMethod]
    public void InexistentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.AddRange([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(25m, "XXX")]));
    }

    [TestMethod]
    public void InexistentCurrencies_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.AddRange([new(100m, "USD"), new(25m, "XXX"), new(25m, "YYY")]));
    }
}