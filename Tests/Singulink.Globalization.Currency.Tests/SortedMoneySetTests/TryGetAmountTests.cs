using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private readonly ImmutableSortedMoneySet _usd100Set = new(new[] { new Money(100m, "USD") });

    [TestMethod]
    public void TryGetAmount_AmountExists_ReturnsTrue()
    {
         _usd100Set.ToSet().TryGetAmount("USD", out var result).ShouldBeTrue();
    }

    [TestMethod]
    public void TryGetAmount_AmountDoesNotExist_ReturnsFalse()
    {
         _usd100Set.ToSet().TryGetAmount("EUR", out var result).ShouldBeFalse();
    }

    [TestMethod]
    public void TryGetAmount_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd100Set.ToSet().TryGetAmount("AAA", out var result));
    }
}