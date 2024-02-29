﻿using Shouldly;

namespace Singulink.Globalization.Tests.SortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly Money _usd100 = new(100m, "USD");
    private static readonly Money _cad50 = new(50m, "CAD");
    private static readonly Money _eur25 = new(25m, "EUR");
    private static readonly ImmutableSortedMoneySet _immutableSet = new(_usd100, _cad50, _eur25);
    private readonly SortedMoneySet _set = _immutableSet.ToSet();

    [TestMethod]
    public void TryGetAmount_AmountExists_ReturnsTrue()
    {
        _set.TryGetAmount("USD", out decimal result).ShouldBeTrue();
    }

    [TestMethod]
    public void TryGetAmount_AmountDoesNotExist_ReturnsFalse()
    {
        _set.TryGetAmount("GBP", out decimal result).ShouldBeFalse();
    }

    [TestMethod]
    public void TryGetAmount_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _set.TryGetAmount("AAA", out decimal result));
    }
}