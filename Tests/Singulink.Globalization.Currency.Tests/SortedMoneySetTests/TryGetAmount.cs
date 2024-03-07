namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[PrefixTestClass]
public class TryGetAmount
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

    // By Currency Tests

    [TestMethod]
    public void GetByCurrency_CurrencyExists_ReturnsTrueAndOutputsAmount()
    {
        _set.TryGetAmount(Currency.Get("USD"), out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        _set.TryGetAmount(Currency.Get("CAD"), out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        _set.TryGetAmount(Currency.Get("EUR"), out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDoesNotExist_ReturnsFalse()
    {
        _set.TryGetAmount(Currency.Get("GBP"), out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrency_CurrencyDisallowed_ThrowsArgumentException()
    {
        var disallowedCurrency = new Currency("AAA", "Disallowed currency", "A", 2);
        Should.Throw<ArgumentException>(() => _set.TryGetAmount(disallowedCurrency, out _));
    }

    // By Currency Code Tests

    [TestMethod]
    public void GetByCurrencyCode_CurrencyExists_ReturnsTrueAndOutputsAmount()
    {
        _set.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        _set.TryGetAmount("CAD", out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        _set.TryGetAmount("EUR", out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDoesNotExist_ReturnsFalse()
    {
        _set.TryGetAmount("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void GetByCurrencyCode_CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetAmount("AAA", out _));
    }
}