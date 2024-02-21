using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[TestClass]
public class TryGetAmountTests
{
    private static readonly ImmutableSortedMoneySet _usd100Set = new ImmutableSortedMoneySet(new[] { new Money(100m, "USD") });

    [TestMethod]
    public void TryGetAmount_AmountExists_ReturnsTrue()
    {
        _usd100Set.TryGetAmount("USD", out var result).ShouldBeTrue();
    }

    [TestMethod]
    public void TryGetAmount_AmountDoesNotExist_ReturnsFalse()
    {
        _usd100Set.TryGetAmount("EUR", out var result).ShouldBeFalse();
    }

    [TestMethod]
    public void TryGetAmount_CurrencyDoesNotExist_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _usd100Set.TryGetAmount("AAA", out var result));
    }
}