using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class AddTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Currency DisallowedCurrency = new("Blah blah blah", "BBB", "$$", 2);
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50];

    // public void Add(Money value) tests

    [TestMethod]
    public void AddMoney_CurrencyExists_UpdatesValue()
    {
        var resultSet = Set.Add(Usd100);

        resultSet.Count.ShouldBe(2);
        resultSet.ShouldBe(ImmutableSortedMoneySet.Create(Money.Create(200m, "USD"), Cad50));
    }

    [TestMethod]
    public void AddMoney_NewCurrency_AddsValue()
    {
        var resultSet = Set.Add(Eur25);

        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe(ImmutableSortedMoneySet.Create(Cad50, Eur25, Usd100));
    }

    [TestMethod]
    public void AddMoney_DefaultValue_NoChange()
    {
        var resultSet = Set.Add(default);

        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void AddMoney_CurrencyDisallowed_ThrowsException()
    {
        var value = new Money(100, DisallowedCurrency);
        Should.Throw<ArgumentException>(() => Set.Add(value));
    }

    // public void Add(decimal amount, string currencyCode) tests

    [TestMethod]
    public void AddByCurrencyCode_CurrencyExists_UpdatesValue()
    {
        var resultSet = Set.Add(100, "USD");
        resultSet.Count.ShouldBe(2);
        resultSet.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void AddByCurrencyCode_NewCurrency_AddsValue()
    {
        var resultSet = Set.Add(25m, "EUR");
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void AddByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.Add(100m, DisallowedCurrency.CurrencyCode));
    }

    // public void Add(decimal amount, Currency currency) tests

    [TestMethod]
    public void AddByCurrency_CurrencyExists_UpdatesValue()
    {
        var resultSet = Set.Add(100m, Currency.Get("USD"));
        resultSet.Count.ShouldBe(2);
        resultSet.ShouldBe([Cad50, new(200m, "USD")]);
    }

    [TestMethod]
    public void AddByCurrency_NewCurrency_AddsValue()
    {
        var resultSet = Set.Add(25m, Currency.Get("EUR"));
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Cad50, Eur25, Usd100]);
    }

    [TestMethod]
    public void AddByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        Set.Count.ShouldBe(2);
        Should.Throw<ArgumentException>(() => Set.Add(100m, DisallowedCurrency));
    }
}