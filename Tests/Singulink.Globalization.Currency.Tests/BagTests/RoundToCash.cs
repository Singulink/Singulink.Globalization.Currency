namespace Singulink.Globalization.Tests.BagTests;

public static class RoundToCash
{
    private static readonly Currency CashCurrency = new("CSH", "Cash Currency") { CashRoundingPolicy = new RoundingPolicy(2, 5) };
    private static readonly Currency WholeCashCurrency = new("WHL", "Whole Cash Currency") { CashRoundingPolicy = new RoundingPolicy(0) };
    private static readonly Currency NoCashCurrency = new("NCS", "No Cash Currency");

    private static readonly CurrencyRegistry Registry = new("Test", [CashCurrency, WholeCashCurrency, NoCashCurrency]);

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    private static readonly ImmutableArray<MonetaryValue> Values          = [new(10.03m, CashCurrency), new(6.5m, WholeCashCurrency)];
    private static readonly ImmutableArray<MonetaryValue> ToEvenResults   = [new(10.05m, CashCurrency), new(6m, WholeCashCurrency)];
    private static readonly ImmutableArray<MonetaryValue> AwayFromZeroResults = [new(10.05m, CashCurrency), new(7m, WholeCashCurrency)];
#pragma warning restore SA1025

    [PrefixTestClass]
    public class TMoneyBag : Mutable<MoneyBag>;

    [PrefixTestClass]
    public class TSortedMoneyBag : Mutable<SortedMoneyBag>;

    [PrefixTestClass]
    public class TImmutableMoneyBag : Immutable<ImmutableMoneyBag>;

    [PrefixTestClass]
    public class TImmutableSortedMoneyBag : Immutable<ImmutableSortedMoneyBag>;

    public class Mutable<TBag> where TBag : IMoneyBag
    {
        [TestMethod]
        public void DefaultToEven()
        {
            var bag = TBag.Create(Registry, Values);
            bag.RoundToCash();
            bag.ShouldBe(ToEvenResults, ignoreOrder: true);
        }

        [TestMethod]
        public void AwayFromZero()
        {
            var bag = TBag.Create(Registry, Values);
            bag.RoundToCash(MidpointRounding.AwayFromZero);
            bag.ShouldBe(AwayFromZeroResults, ignoreOrder: true);
        }

        [TestMethod]
        public void AlreadyRounded()
        {
            var bag = TBag.Create(Registry, ToEvenResults);
            bag.RoundToCash();
            bag.ShouldBe(ToEvenResults, ignoreOrder: true);
        }

        [TestMethod]
        public void ThrowsWithoutPolicy()
        {
            var bag = TBag.Create(Registry, [new(10.03m, CashCurrency), new(1.23m, NoCashCurrency)]);
            Should.Throw<NotSupportedException>(() => bag.RoundToCash());
        }
    }

    public class Immutable<TBag> where TBag : IImmutableMoneyBag
    {
        [TestMethod]
        public void DefaultToEven()
        {
            var bag = TBag.Create(Registry, Values);
            bag.RoundToCash().ShouldBe(ToEvenResults, ignoreOrder: true);
        }

        [TestMethod]
        public void AwayFromZero()
        {
            var bag = TBag.Create(Registry, Values);
            bag.RoundToCash(MidpointRounding.AwayFromZero).ShouldBe(AwayFromZeroResults, ignoreOrder: true);
        }

        [TestMethod]
        public void AlreadyRoundedReturnsSameInstance()
        {
            var bag = TBag.Create(Registry, ToEvenResults);
            bag.RoundToCash().ShouldBeSameAs(bag);
        }

        [TestMethod]
        public void ThrowsWithoutPolicy()
        {
            var bag = TBag.Create(Registry, [new(10.03m, CashCurrency), new(1.23m, NoCashCurrency)]);
            Should.Throw<NotSupportedException>(() => bag.RoundToCash());
        }
    }
}
