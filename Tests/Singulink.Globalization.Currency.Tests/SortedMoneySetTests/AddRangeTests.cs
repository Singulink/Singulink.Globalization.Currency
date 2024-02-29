using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;


[TestClass]
public class AddRangeTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly List<Money> _moneyValues = new() { new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR") };
    private static readonly ImmutableSortedMoneySet _set = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _sortedSet = _set.ToSet();

    [TestMethod]
    public void AddRange_AllCurrenciesExistInTheSet_IsSuccessful()
    {
        _sortedSet.AddRange(_moneyValues);
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe([new(200m, "USD"), new(100m, "CAD"), new(50m, "EUR")]);
    }

    [TestMethod]
    public void AddRange_SomeCurrenciesDoNotExistInTheSet_IsSuccessful()
    {
        List<Money> moneyValues = new() { new(100m, "USD"), new(50m, "JPY"), new(25m, "CHF") };
        _sortedSet.AddRange(moneyValues);
        _sortedSet.Count.ShouldBe(5);
        _sortedSet.ShouldBe([new(200m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void AddRange_NoCurrencyExistsInTheSet_IsSuccessful()
    {
        List<Money> moneyValues = new() { new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF") };
        _sortedSet.AddRange(moneyValues);
        _sortedSet.Count.ShouldBe(6);
        _sortedSet.ShouldBe([new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(100m, "GBP"), new(50m, "JPY"), new(25m, "CHF")]);
    }

    [TestMethod]
    public void AddRange_EmptyCollection_NoChange()
    {
        _sortedSet.AddRange(new List<Money>());
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe(_set);
    }

    [TestMethod]
    public void AddRange_InexistentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sortedSet.AddRange(new List<Money> { new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR"), new(25m, "XXX") }));
    }

    [TestMethod]
    public void AddRange_InexistentCurrencies_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sortedSet.AddRange(new List<Money> { new(100m, "USD"), new(25m, "XXX"), new(25m, "YYY") }));
    }
}