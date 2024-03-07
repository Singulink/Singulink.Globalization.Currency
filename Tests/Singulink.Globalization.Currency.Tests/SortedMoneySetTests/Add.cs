namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class Add
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    // public void Add(Money value) tests

    [TestMethod]
    public void AddMoney_CurrencyExists_UpdatesValue()
    {
        _set.Add(Usd100);
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, Money.Create(200m, "USD")]);
    }

    [TestMethod]
    public void AddMoney_NewCurrency_AddsValue()
    {
        _set.Add(Eur25);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void AddMoney_DefaultValue_NoChange()
    {
        _set.Add(default);
        _set.Count.ShouldBe(2);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void AddMoney_CurrencyDisallowed_ThrowsArgumentException()
    {
        var value = new Money(100, DisallowedCurrency);
        Should.Throw<ArgumentException>(() => _set.Add(value));
    }

    // public void Add(decimal amount, string currencyCode) tests

    [TestMethod]
    public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
    {
        _set.Add(100, "USD");
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void AddByCurrencyCode_NewCurrency_AddsValue()
    {
        _set.Add(25m, "EUR");
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.Add(100m, DisallowedCurrency.CurrencyCode));
    }

    // public void Add(decimal amount, Currency currency) tests

    [TestMethod]
    public void AddByCurrency_CurrencyExists_UpdatesValue()
    {
        _set.Add(100m, Currency.Get("USD"));
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void AddByCurrency_NewCurrency_AddsValue()
    {
        _set.Add(25m, Currency.Get("EUR"));
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        _set.Count.ShouldBe(2);
        Should.Throw<ArgumentException>(() => _set.Add(100m, DisallowedCurrency));
    }
}