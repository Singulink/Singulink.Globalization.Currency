using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetValueTests
{
    private static readonly Money _usd100 = new Money(100m, "USD");
    private static readonly Money _eur50 = new Money(50m, "EUR");
    private static readonly Money _cad25 = new Money(25m, "CAD");

    private static readonly ImmutableSortedMoneySet _moneySetValues = new(_usd100, _eur50, _cad25);

    [TestMethod]
    public void TryGetValue_ValueExists_ReturnsTrue()
    {
        _moneySetValues.ToSet().TryGetValue(Currency.Get("USD"), out var money).ShouldBeTrue();
        money.ShouldBe(_usd100);

        _moneySetValues.ToSet().TryGetValue(Currency.Get("EUR"), out money).ShouldBeTrue();
        money.ShouldBe(_eur50);

        _moneySetValues.ToSet().TryGetValue(Currency.Get("CAD"), out money).ShouldBeTrue();
        money.ShouldBe(_cad25);
    }

    [TestMethod]
    public void TryGetValue_ValueDoesNotExist_ReturnsFalse()
    {
        _moneySetValues.ToSet().TryGetValue(Currency.Get("GBP"), out var money).ShouldBeFalse();
        money.ShouldBe(default);
    }

    [TestMethod]
    public void TryGetValue_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _moneySetValues.ToSet().TryGetValue(Currency.Get("XXX"), out var money));
    }

    // TryGetValue(string currencyCode, out Money value) tests

    [TestMethod]
    public void TryGetValue_MoneyValueExists_ReturnsTrue()
    {
        _moneySetValues.ToSet().TryGetValue("USD", out var money).ShouldBeTrue();
        money.ShouldBe(_usd100);

        _moneySetValues.ToSet().TryGetValue("EUR", out money).ShouldBeTrue();
        money.ShouldBe(_eur50);

        _moneySetValues.ToSet().TryGetValue("CAD", out money).ShouldBeTrue();
        money.ShouldBe(_cad25);
    }

    [TestMethod]
    public void TryGetValue_MoneyValueDoesNotExist_ReturnsFalse()
    {
        _moneySetValues.ToSet().TryGetValue("GBP", out var money).ShouldBeFalse();
        money.ShouldBe(default);
    }

    [TestMethod]
    public void TryGetValue_MoneyCurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _moneySetValues.ToSet().TryGetValue("XXX", out var money));
    }
}