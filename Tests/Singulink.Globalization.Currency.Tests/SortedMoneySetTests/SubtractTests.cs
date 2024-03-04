using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class SubtractTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    // public void Subtract(Money value) tests

    [TestMethod]
    public void SubtractMoney_CurrencyExists_UpdatesValue()
    {
        _set.Subtract(-Usd100);
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, Money.Create(200m, "USD")]);
    }

    [TestMethod]
    public void SubtractMoney_NewCurrency_AddsValue()
    {
        _set.Subtract(-Eur25);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void SubtractMoney_DefaultValue_NoChange()
    {
        _set.Subtract(default);
        _set.Count.ShouldBe(2);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void SubtractMoney_CurrencyDisallowed_ThrowsArgumentException()
    {
        var value = new Money(100, DisallowedCurrency);
        Should.Throw<ArgumentException>(() => _set.Subtract(value));
    }

    // public void Subtract(decimal amount, string currencyCode) tests

    [TestMethod]
    public void SubtractByCurrencyCode_CurrencyExists_UpdatesValue()
    {
        _set.Subtract(-100, "USD");
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void SubtractByCurrencyCode_NewCurrency_AddsValue()
    {
        _set.Subtract(-25m, "EUR");
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void SubtractByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.Subtract(100m, DisallowedCurrency.CurrencyCode));
    }

    // public void Subtract(decimal amount, Currency currency) tests

    [TestMethod]
    public void SubtractByCurrency_CurrencyExists_UpdatesValue()
    {
        _set.Subtract(-100m, Currency.Get("USD"));
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void SubtractByCurrency_NewCurrency_AddsValue()
    {
        _set.Subtract(-25m, Currency.Get("EUR"));
        _set.Count.ShouldBe(3);
        _set.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void SubtractByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        _set.Count.ShouldBe(2);
        Should.Throw<ArgumentException>(() => _set.Subtract(100m, DisallowedCurrency));
    }
}