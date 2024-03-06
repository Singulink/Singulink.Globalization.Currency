using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class Remove
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    // public bool Remove(string currencyCode) tests

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyFound_RemovesValue()
    {
        var resultSet = Set.Remove("USD");
        resultSet.Count.ShouldBe(2);
        resultSet.ShouldBe([Cad50, Eur25]);
    }

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyNotFound_NoChange()
    {
        var resultSet = Set.Remove("JPY");
        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void RemoveCurrencyCode_DisallowedCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.Remove("XXX"));
    }

    // public bool Remove(Currency currency) tests

    [TestMethod]
    public void RemoveCurrency_CurrencyFound_RemovesValue()
    {
        var resultSet = Set.Remove(Usd100.Currency);
        resultSet.Count.ShouldBe(2);
        resultSet.ShouldBe([Cad50, Eur25]);
    }

    [TestMethod]
    public void RemoveCurrency_CurrencyNotFound_NoChange()
    {
        var gbpCurrency = Currency.Get("GBP");
        var resultSet = Set.Remove(gbpCurrency);
        resultSet.ShouldBeSameAs(Set);
    }

    [TestMethod]
    public void RemoveCurrency_DisallowedCurrency_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("XXX", "Disallowed currency", "X", 2);
        Should.Throw<ArgumentException>(() => Set.Remove(disallowedCurrency));
    }
}