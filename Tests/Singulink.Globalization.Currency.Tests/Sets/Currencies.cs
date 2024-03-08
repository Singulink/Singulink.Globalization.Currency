namespace Singulink.Globalization.Tests.Sets;
public static class Currencies
{
    [PrefixTestClass]
    public class TMoneySet : Tests<MoneySet>;

    [PrefixTestClass]
    public class TSortedMoneySet : Tests<SortedMoneySet>;

    [PrefixTestClass]
    public class TImmutableMoneySet : Tests<ImmutableMoneySet>;

    [PrefixTestClass]
    public class TImmutableSortedMoneySet : Tests<ImmutableSortedMoneySet>;

    public class Tests<T> where T : IReadOnlyMoneySet
    {
        private static readonly ImmutableArray<Currency> Currencies = [Currency.Get("USD"), Currency.Get("CAD"), Currency.Get("EUR")];

        private readonly IReadOnlyMoneySet _set = T.Create(CurrencyRegistry.Default, [new(100m, "USD"), new(50m, "CAD"), new(25m, "EUR")]);

        [TestMethod]
        public void Currencies_GetsValueCurrencies()
        {
            _set.Currencies.Count.ShouldBe(3);
            _set.Currencies.ShouldBe(Currencies, ignoreOrder: true);
        }
    }
}
