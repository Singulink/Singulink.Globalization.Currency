using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _set = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _sortedSet = _set.ToSet();

    [TestMethod]
    public void AmountExists_ReturnsTrue()
    {
        _sortedSet.TryGetAmount("USD", out decimal result).ShouldBeTrue();
    }

    [TestMethod]
    public void AmountDoesNotExist_ReturnsFalse()
    {
        _sortedSet.TryGetAmount("GBP", out decimal result).ShouldBeFalse();
    }

    [TestMethod]
    public void CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sortedSet.TryGetAmount("AAA", out decimal result));
    }
}