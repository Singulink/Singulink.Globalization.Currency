using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class TryGetValueTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    [TestMethod]
    public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsValue()
    {
        _set.TryGetValue(Currency.Get("USD"), out var value).ShouldBeTrue();
        value.ShouldBe(Usd100);

        _set.TryGetValue(Currency.Get("CAD"), out value).ShouldBeTrue();
        value.ShouldBe(Cad50);

        _set.TryGetValue(Currency.Get("EUR"), out value).ShouldBeTrue();
        value.ShouldBe(Eur25);
    }

    [TestMethod]
    public void GetByCurrency_ValueDoesNotExist_ReturnsFalse()
    {
        _set.TryGetValue(Currency.Get("GBP"), out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetValue(Currency.Get("XXX"), out _));
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsValue()
    {
        _set.TryGetValue("USD", out var value).ShouldBeTrue();
        value.ShouldBe(Usd100);

        _set.TryGetValue("CAD", out value).ShouldBeTrue();
        value.ShouldBe(Cad50);

        _set.TryGetValue("EUR", out value).ShouldBeTrue();
        value.ShouldBe(Eur25);
    }

    [TestMethod]
    public void GetByCurrencyCode_ValueDoesNotExist_ReturnsFalse()
    {
        _set.TryGetValue("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetValue("XXX", out _));
    }
}