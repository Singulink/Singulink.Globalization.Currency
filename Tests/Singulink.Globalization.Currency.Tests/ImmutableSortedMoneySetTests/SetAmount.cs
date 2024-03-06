using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class SetAmount
{
    private static readonly Currency Usd = Currency.Get("USD");
    private static readonly Currency Aud = Currency.Get("AUD");
    private static readonly Currency DisallowedCurrency = new Currency("Blah blah blah", "BBB", "$$", 2);

    private static readonly Money Usd0 = new(0m, "USD");
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Money Aud75 = new(75m, "AUD");

    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    // public void SetAmount(decimal amount, string currencyCode) tests

    [TestMethod]
    public void SetByCurrencyCode_CurrencyExists_UpdatesValue()
    {
        var resultSet = Set.SetAmount(200m, "USD");
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Money.Create(200m, "USD"), Cad50, Eur25]);
    }

    [TestMethod]
    public void SetByCurrencyCode_NewCurrency_AddsValue()
    {
        var resultSet = Set.SetAmount(75m, "AUD");
        resultSet.Count.ShouldBe(4);
        resultSet.ShouldBe([Cad50, Eur25, Usd100, Aud75]);
    }

    [TestMethod]
    public void SetByCurrencyCode_ZeroAmount_ZeroesValue()
    {
        var resultSet = Set.SetAmount(0, "USD");
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Usd0, Cad50, Eur25]);
    }

    [TestMethod]
    public void SetByCurrencyCode_SetAlreadyContainsCurrencyCodeAndAmount_ReturnsItself()
    {
        var resultSet = Set.SetAmount(100m, "USD");
        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void SetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.SetAmount(123m, DisallowedCurrency.CurrencyCode));
    }

    // public void SetAmount(decimal amount, Currency currency) tests

    [TestMethod]

    public void SetByCurrency_CurrencyExists_UpdatesValue()
    {
        var resultSet = Set.SetAmount(200m, Usd);
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([new(200m, "USD"), Cad50, Eur25]);
    }

    [TestMethod]
    public void SetByCurrency_CurrencyDoesNotExist_AddsValue()
    {
        var resultSet = Set.SetAmount(75m, Aud);
        resultSet.Count.ShouldBe(4);
        resultSet.ShouldBe([Cad50, Eur25, Usd100, Aud75]);
    }

    [TestMethod]
    public void SetByCurrency_ZeroAmount_ZeroesValue()
    {
        var resultSet = Set.SetAmount(0, Usd);
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Usd0, Cad50, Eur25]);
    }

    [TestMethod]
    public void SetByCurrencyCode_SetAlreadyContainsCurrencyAndAmount_ReturnsItself()
    {
        var resultSet = Set.SetAmount(100m, Usd);
        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void SetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.SetAmount(123m, DisallowedCurrency));
    }
}