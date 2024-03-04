using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = [_usd100, _cad50, _eur25];
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void AmountExists_ReturnsTrueAndOutputsAmount()
    {
        _set.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        _set.TryGetAmount("CAD", out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        _set.TryGetAmount("EUR", out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void AmountDoesNotExist_ReturnsFalse()
    {
        _set.TryGetAmount("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetAmount("AAA", out _));
    }
}