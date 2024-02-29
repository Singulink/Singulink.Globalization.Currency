using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class AddTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly Currency _bbbCurrency = new("Blah blah blah", "BBB", "$$", 2);
    private static readonly ImmutableSortedMoneySet _set = new(_usd100, _cad50);
    private readonly SortedMoneySet _sortedSet = _set.ToSet();

    // public void Add(Money value) tests
    [TestMethod]
    public void AddMoney_CurrencyExistsInTheSet_IsSuccesful()
    {
        _sortedSet.Add(_usd100);
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddMoney_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        _sortedSet.Add(_eur25);
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddMoney_DefaultValue_NoChange()
    {
        _sortedSet.Add(default);
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe(_set);
    }

    [TestMethod]
    public void AddMoney_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        var value = new Money(100, _bbbCurrency);
        Should.Throw<ArgumentException>(() => _sortedSet.Add(value));
    }

    // public void Add(decimal amount, string currencyCode) tests
    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyExistsInTheSet_IsSuccesful()
    {
        _sortedSet.Add(100, "USD");
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        _sortedSet.Add(25m, "EUR");
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sortedSet.Add(100m, "BBB"));
    }

    // public void Add(decimal amount, Currency currency) tests
    [TestMethod]
    public void AddAmountCurrency_CurrencyExistsInTheSet_IsSuccesful()
    {
        var myCurrency = Currency.Get("USD");
        _sortedSet.Add(100m, myCurrency);
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddAmountCurrency_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        var myCurrency = Currency.Get("EUR");
        _sortedSet.Add(25m, myCurrency);
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddAmountCurrency_DefaultValue_NoChange()
    {
        var myCurrency = Currency.Get("USD");
        _sortedSet.Add(default, myCurrency);
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe(_set);
    }

    [TestMethod]
    public void AddAmountCurrency_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        _sortedSet.Count.ShouldBe(2);
        Should.Throw<ArgumentException>(() => _sortedSet.Add(100m, _bbbCurrency));
    }

}