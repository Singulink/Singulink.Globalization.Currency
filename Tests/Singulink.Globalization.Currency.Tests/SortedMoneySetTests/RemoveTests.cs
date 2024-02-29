using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class RemoveTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _set = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _sortedSet = _set.ToSet();

    // public bool Remove(string currencyCode) tests

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyExistsInTheSet_ReturnsTrue()
    {
        _sortedSet.Remove("USD").ShouldBeTrue();
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe([_cad50, _eur25]);
    }

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyDoesNotExistInSet_ReturnsFalse()
    {
        _sortedSet.Remove("JPY").ShouldBeFalse();
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe(_set);
    }

    [TestMethod]
    public void RemoveCurrencyCode_InexistentCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sortedSet.Remove("XXX"));
    }

    // public bool Remove(Currency currency) tests

    [TestMethod]
    public void RemoveCurrency_CurrencyExistsInTheSet_ReturnsTrue()
    {
        _sortedSet.Remove(_usd100.Currency).ShouldBeTrue();
        _sortedSet.Count.ShouldBe(2);
        _sortedSet.ShouldBe([_cad50, _eur25]);
    }

    [TestMethod]
    public void RemoveCurrency_CurrencyDoesNotExistInSet_ReturnsFalse()
    {
        var gbpCurrency = Currency.Get("GBP");
        _sortedSet.Remove(gbpCurrency).ShouldBeFalse();
        _sortedSet.Count.ShouldBe(3);
        _sortedSet.ShouldBe(_set);
    }

    [TestMethod]
    public void RemoveCurrency_InexistentCurrency_ThrowsArgumentException()
    {
        var nonExistentCurrency = new Currency("XXX", "Non-existent currency", "X", 2);
        Should.Throw<ArgumentException>(() => _sortedSet.Remove(nonExistentCurrency));
    }
}