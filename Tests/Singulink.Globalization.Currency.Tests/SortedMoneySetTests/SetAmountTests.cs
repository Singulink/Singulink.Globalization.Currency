using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class SetAmountTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly Money _aud75 = new(75m, "AUD");
    private static readonly ImmutableSortedMoneySet _immutableSet = [_usd100, _cad50, _eur25];
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    // public void SetAmount(decimal amount, string currencyCode) tests
    [TestMethod]
    public void SetAmount_CurrencyExists_UpdatesValue()
    {
        _set.SetAmount(200m, "USD");
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), _cad50, _eur25]);
    }

    [TestMethod]
    public void SetAmount_CurrencyDoesNotExist_AddsValue()
    {
        _set.SetAmount(75m, "AUD");
        _set.Count.ShouldBe(4);
        _set.ShouldBe([_cad50, _eur25, _usd100, _aud75]);
    }

    [TestMethod]
    public void SetAmount_DefaultValue_IsSuccessful()
    {
        _set.SetAmount(default, _usd100.Currency.CurrencyCode);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(0m, "USD"), _cad50, _eur25]);
    }

    [TestMethod]
    public void SetAmount_CurrencyIsNotAccepted_ThrowsArgumentException()
    {
        var value = new Money(100, new Currency("Blah blah blah", "BBB", "$$", 2));
        Should.Throw<ArgumentException>(() => _set.SetAmount(value.Amount, value.Currency.CurrencyCode));
    }

    // public void SetAmount(decimal amount, Currency currency) tests
    [TestMethod]

    public void SetAmountCurrency_CurrencyExists_UpdatesValue()
    {
        _set.SetAmount(200m, _usd100.Currency);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(200m, "USD"), _cad50, _eur25]);
    }

    [TestMethod]
    public void SetAmountCurrency_CurrencyDoesNotExist_AddsValue()
    {
        var currency = Currency.Get("AUD");
        _set.SetAmount(75m, currency);
        _set.Count.ShouldBe(4);
        _set.ShouldBe([_cad50, _eur25, _usd100, _aud75]);
    }

    [TestMethod]
    public void SetAmountCurrency_DefaultValue_IsSuccessful()
    {
        _set.SetAmount(default, _usd100.Currency);
        _set.Count.ShouldBe(3);
        _set.ShouldBe([new(0m, "USD"), _cad50, _eur25]);
    }

    [TestMethod]
    public void SetAmountCurrency_CurrencyIsNotAccepted_ThrowsArgumentException()
    {
        var value = new Money(100, new Currency("Blah blah blah", "BBB", "$$", 2));
        Should.Throw<ArgumentException>(() => _set.SetAmount(value.Amount, value.Currency));
    }

    [TestMethod]
    public void SetAmount_CurrencyIsNull_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => _set.SetAmount(100m, (Currency)null!));
    }
}