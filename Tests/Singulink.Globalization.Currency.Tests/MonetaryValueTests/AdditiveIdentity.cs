#if NET

using System.Globalization;
using System.Numerics;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class AdditiveIdentity
{
    private static readonly MonetaryValue Usd1050 = new(10.50m, "USD");
    private static readonly MonetaryValue Jpy5 = new(5m, "JPY");

    [TestMethod]
    public void AdditiveIdentity_IsDefault()
    {
        GetAdditiveIdentity<MonetaryValue>().ShouldBe(MonetaryValue.Default);
        GetAdditiveIdentity<MonetaryValue>().IsDefault.ShouldBeTrue();
    }

    [TestMethod]
    public void AddingIdentity_ReturnsSameValue()
    {
        var identity = GetAdditiveIdentity<MonetaryValue>();

        foreach (var value in new[] { Usd1050, Jpy5, new MonetaryValue(0m, "EUR"), new MonetaryValue(-3.125m, "CAD"), MonetaryValue.Default })
        {
            var right = value + identity;
            var left = identity + value;

            right.ShouldBe(value);
            left.ShouldBe(value);
            right.CurrencyOrDefault.ShouldBeSameAs(value.CurrencyOrDefault);
            left.CurrencyOrDefault.ShouldBeSameAs(value.CurrencyOrDefault);
        }
    }

    [TestMethod]
    public void AddingIdentity_KeepsScale()
    {
        var identity = GetAdditiveIdentity<MonetaryValue>();

        (Usd1050 + identity).Amount.ToString(CultureInfo.InvariantCulture).ShouldBe("10.50");
        (identity + Usd1050).Amount.ToString(CultureInfo.InvariantCulture).ShouldBe("10.50");
    }

    [TestMethod]
    public void GenericSum_StartsFromIdentity()
    {
        Sum<MonetaryValue>([Usd1050, new(2m, "USD"), new(0.25m, "USD")]).ShouldBe(new MonetaryValue(12.75m, "USD"));
        Sum<MonetaryValue>([Jpy5]).ShouldBe(Jpy5);
        Sum<MonetaryValue>([]).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void GenericSum_MixedCurrencies_Throws()
    {
        Should.Throw<ArgumentException>(() => Sum<MonetaryValue>([Usd1050, Jpy5]));
    }

    private static T GetAdditiveIdentity<T>() where T : IAdditiveIdentity<T, T> => T.AdditiveIdentity;

    private static T Sum<T>(IEnumerable<T> values) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        T sum = T.AdditiveIdentity;

        foreach (var value in values)
            sum += value;

        return sum;
    }
}

#endif
