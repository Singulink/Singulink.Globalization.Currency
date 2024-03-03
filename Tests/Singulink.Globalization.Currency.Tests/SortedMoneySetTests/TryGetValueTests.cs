using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetValueTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = [_usd100, _cad50, _eur25];
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void GetByCurrency_ValueExists_ReturnsTrue()
    {
        _set.TryGetValue(Currency.Get("USD"), out var money).ShouldBeTrue();
        money.ShouldBe(_usd100);

        _set.TryGetValue(Currency.Get("CAD"), out money).ShouldBeTrue();
        money.ShouldBe(_cad50);

        _set.TryGetValue(Currency.Get("EUR"), out money).ShouldBeTrue();
        money.ShouldBe(_eur25);
    }

    [TestMethod]
    public void GetByCurrency_ValueDoesNotExist_ReturnsFalse()
    {
        _set.TryGetValue(Currency.Get("GBP"), out var money).ShouldBeFalse();
        money.ShouldBe(default);
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetValue(Currency.Get("XXX"), out var money));
    }

    [TestMethod]
    public void GetByCurrencyCode_ValueExists_ReturnsTrue()
    {
        _set.TryGetValue("USD", out var money).ShouldBeTrue();
        money.ShouldBe(_usd100);

        _set.TryGetValue("CAD", out money).ShouldBeTrue();
        money.ShouldBe(_cad50);

        _set.TryGetValue("EUR", out money).ShouldBeTrue();
        money.ShouldBe(_eur25);
    }

    [TestMethod]
    public void GetByCurrencyCode_ValueDoesNotExist_ReturnsFalse()
    {
        _set.TryGetValue("GBP", out var money).ShouldBeFalse();
        money.ShouldBe(default);
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetValue("XXX", out var money));
    }
}