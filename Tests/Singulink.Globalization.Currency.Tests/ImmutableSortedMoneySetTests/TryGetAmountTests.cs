using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class TryGetAmountTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    // public void TryGetAmount(string currencyCode, out decimal amount) tests

    [TestMethod]
    public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsAmount()
    {
        Set.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        Set.TryGetAmount("CAD", out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        Set.TryGetAmount("EUR", out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
    {
        Set.TryGetAmount("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.TryGetAmount("AAA", out _));
    }

    // public void TryGetAmount(Currency currency, out decimal amount) tests

    [TestMethod]
    public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsAmount()
    {
        Set.TryGetAmount(Currency.Get("USD"), out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        Set.TryGetAmount(Currency.Get("CAD"), out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        Set.TryGetAmount(Currency.Get("EUR"), out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
    {
        Set.TryGetAmount(Currency.Get("GBP"), out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.TryGetAmount(Currency.Get("AAA"), out _));
    }
}