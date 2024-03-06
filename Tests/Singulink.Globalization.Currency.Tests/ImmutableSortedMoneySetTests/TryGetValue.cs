using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class TryGetValue
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    [TestMethod]
    public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsValue()
    {
        Set.TryGetValue(Currency.Get("USD"), out var value).ShouldBeTrue();
        value.ShouldBe(Usd100);

        Set.TryGetValue(Currency.Get("CAD"), out value).ShouldBeTrue();
        value.ShouldBe(Cad50);

        Set.TryGetValue(Currency.Get("EUR"), out value).ShouldBeTrue();
        value.ShouldBe(Eur25);
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
    {
        Set.TryGetValue(Currency.Get("GBP"), out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.TryGetValue(Currency.Get("XXX"), out _));
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsValue()
    {
        Set.TryGetValue("USD", out var value).ShouldBeTrue();
        value.ShouldBe(Usd100);

        Set.TryGetValue("CAD", out value).ShouldBeTrue();
        value.ShouldBe(Cad50);

        Set.TryGetValue("EUR", out value).ShouldBeTrue();
        value.ShouldBe(Eur25);
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
    {
        Set.TryGetValue("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.TryGetValue("XXX", out _));
    }
}