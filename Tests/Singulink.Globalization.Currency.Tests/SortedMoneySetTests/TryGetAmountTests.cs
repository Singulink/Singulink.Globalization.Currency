using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet ImmutableSet = [Usd100, Cad50, Eur25];

    private readonly SortedMoneySet _set = ImmutableSet.ToSet();

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
    public void CurrencyDisallowed_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetAmount("AAA", out _));
    }
}