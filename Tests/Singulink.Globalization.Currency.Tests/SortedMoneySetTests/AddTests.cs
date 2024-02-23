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

    // public void Add(Money value) tests
    [TestMethod]
    public void AddMoney_CurrencyExistsInTheSet_IsSuccesful()
    {
        var set = _set.ToSet();
        set.Add(_usd100);
        set.Count.ShouldBe(2);
        set.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddMoney_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        var set = _set.ToSet();
        set.Add(_eur25);
        set.Count.ShouldBe(3);
        set.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddMoney_DefaultValue_NoChange()
    {
        var set = _set.ToSet();
        set.Add(default);
        set.Count.ShouldBe(2);
        set.ShouldBe(_set);
    }

    [TestMethod]
    public void AddMoney_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        var set = _set.ToSet();
        var value = new Money(100, _bbbCurrency);

        Should.Throw<ArgumentException>(() => set.Add(value));
    }

    // public void Add(decimal amount, string currencyCode) tests

    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyExistsInTheSet_IsSuccesful()
    {
        var set = _set.ToSet();
        set.Add(100, "USD");
        set.Count.ShouldBe(2);
        set.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        var set = _set.ToSet();
        set.Add(25m, "EUR");
        set.Count.ShouldBe(3);
        set.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddAmountCurrencyCode_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        var set = _set.ToSet();
        Should.Throw<ArgumentException>(() => set.Add(100m, "BBB"));
    }

    // public void Add(decimal amount, Currency currency) tests

    [TestMethod]
    public void AddAmountCurrency_CurrencyExistsInTheSet_IsSuccesful()
    {
        var set = _set.ToSet();
        var myCurrency = Currency.Get("USD");
        set.Add(100m, myCurrency);
        set.Count.ShouldBe(2);
        set.ShouldBe([_cad50, new Money(200m, "USD")]);
    }

    [TestMethod]
    public void AddAmountCurrency_CurrencyDoesNotExistInSet_IsSuccessful()
    {
        var set = _set.ToSet();
        var myCurrency = Currency.Get("EUR");
        set.Add(25m, myCurrency);
        set.Count.ShouldBe(3);
        set.ShouldBe([_cad50, _eur25, _usd100]);
    }

    [TestMethod]
    public void AddAmountCurrency_DefaultValue_NoChange()
    {
        var set = _set.ToSet();
        var myCurrency = Currency.Get("USD");
        set.Add(default, myCurrency);
        set.Count.ShouldBe(2);
        set.ShouldBe(_set);
    }

    [TestMethod]
    public void AddAmountCurrency_CurrencyIsNotAcceptedInTheSet_ThrowsArgumentException()
    {
        var set = _set.ToSet();
        set.Count.ShouldBe(2);
        Should.Throw<ArgumentException>(() => set.Add(100m, _bbbCurrency));
    }

}