﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrefixClassName.MsTest;
using Shouldly;

namespace Singulink.Globalization.Tests.ImmutableSortedMoneySetTests;

[PrefixTestClass]
public class TrimZeroAmounts
{
    private static readonly Money Usd100 = new(100m, "USD");
    private static readonly Money Cad50 = new(50m, "CAD");
    private static readonly Money Eur25 = new(25m, "EUR");
    private static readonly Money Gbp0 = new(0m, "GBP");
    private static readonly Money Jpy0 = new(0m, "JPY");
    private static readonly ImmutableSortedMoneySet ZeroAmountsSet = [Usd100, Cad50, Eur25, Gbp0, Jpy0];
    private static readonly ImmutableSortedMoneySet NoZeroAmountsSet = [Usd100, Cad50, Eur25];

    [TestMethod]
    public void TrimZeroAmounts_ZeroAmountsExist_RemovesZeroAmounts()
    {
        var resultSet = ZeroAmountsSet.TrimZeroAmounts();
        resultSet.Count.ShouldBe(3);
        resultSet.ShouldBe([Usd100, Cad50, Eur25]);
    }

    [TestMethod]
    public void TrimZeroAmounts_NoZeroAmountsExist_NoChange()
    {
        var resultSet = NoZeroAmountsSet.TrimZeroAmounts();
        resultSet.ShouldBeSameAs(NoZeroAmountsSet);
    }
}