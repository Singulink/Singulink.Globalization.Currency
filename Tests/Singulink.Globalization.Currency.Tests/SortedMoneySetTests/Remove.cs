namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class Remove
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSortedMoneySet();

    // public bool Remove(string currencyCode) tests

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyFound_RemovesValueAndReturnsTrue()
    {
        _set.Remove("USD").ShouldBeTrue();
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, Eur25]);
    }

    [TestMethod]
    public void RemoveCurrencyCode_CurrencyNotFound_ReturnsFalseAndNoChange()
    {
        _set.Remove("JPY").ShouldBeFalse();
        _set.Count.ShouldBe(3);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void RemoveCurrencyCode_DisallowedCurrency_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.Remove("XXX"));
    }

    // public bool Remove(Currency currency) tests

    [TestMethod]
    public void RemoveCurrency_CurrencyFound_RemovesValueAndReturnsTrue()
    {
        _set.Remove(Usd100.Currency).ShouldBeTrue();
        _set.Count.ShouldBe(2);
        _set.ShouldBe([Cad50, Eur25]);
    }

    [TestMethod]
    public void RemoveCurrency_CurrencyNotFound_ReturnsFalseAndNoChange()
    {
        var gbpCurrency = Currency.Get("GBP");
        _set.Remove(gbpCurrency).ShouldBeFalse();
        _set.Count.ShouldBe(3);
        _set.ShouldBe(ImmutableSet);
    }

    [TestMethod]
    public void RemoveCurrency_DisallowedCurrency_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("XXX", "Disallowed currency", "X", 2);
        Should.Throw<ArgumentException>(() => _set.Remove(disallowedCurrency));
    }
}