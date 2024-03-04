using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet Set = [Usd100, Cad50, Eur25];

    [TestMethod]
    public void AmountExists_ReturnsTrueAndOutputsAmount()
    {
        Set.TryGetAmount("USD", out decimal amount).ShouldBeTrue();
        amount.ShouldBe(100m);

        Set.TryGetAmount("CAD", out amount).ShouldBeTrue();
        amount.ShouldBe(50m);

        Set.TryGetAmount("EUR", out amount).ShouldBeTrue();
        amount.ShouldBe(25m);
    }

    [TestMethod]
    public void AmountDoesNotExist_ReturnsFalse()
    {
        Set.TryGetAmount("GBP", out _).ShouldBeFalse();
    }

    [TestMethod]
    public void CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Set.TryGetAmount("AAA", out _));
    }
}