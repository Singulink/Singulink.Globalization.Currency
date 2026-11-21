namespace Singulink.Globalization.Tests.MonetaryValueTests;

[PrefixTestClass]
public class TransformAmount
{
    [TestMethod]
    public void WithCurrency_AppliesTheFunctionAndKeepsTheCurrency()
    {
        var value = new MonetaryValue(19.97m, "USD");

        value.TransformAmount(a => Math.Ceiling(a)).ShouldBe(new MonetaryValue(20m, "USD"));
        value.TransformAmount(a => Math.Truncate(a)).ShouldBe(new MonetaryValue(19m, "USD"));
        value.TransformAmount(a => Math.Ceiling(a * 4) / 4).ShouldBe(new MonetaryValue(20m, "USD"));
        value.TransformAmount(a => Math.Min(a, 15m)).ShouldBe(new MonetaryValue(15m, "USD"));
        value.TransformAmount(a => a * 2).ShouldBe(value * 2);

        var yen = new MonetaryValue(1234m, "JPY");
        yen.TransformAmount(a => a + 1).Currency.ShouldBeSameAs(yen.Currency);
    }

    [TestMethod]
    public void WithCurrency_ZeroResult_KeepsTheCurrency()
    {
        var value = new MonetaryValue(5m, "USD");
        var result = value.TransformAmount(_ => 0m);

        result.ShouldBe(new MonetaryValue(0m, "USD"));
        result.IsDefault.ShouldBeFalse();
        result.Currency.ShouldBeSameAs(value.Currency);

        new MonetaryValue(0m, "EUR").TransformAmount(a => a).ShouldBe(new MonetaryValue(0m, "EUR"));
    }

    [TestMethod]
    public void NegativeAmounts()
    {
        var value = new MonetaryValue(-19.97m, "USD");

        value.TransformAmount(a => Math.Ceiling(a)).ShouldBe(new MonetaryValue(-19m, "USD"));
        value.TransformAmount(a => Math.Floor(a)).ShouldBe(new MonetaryValue(-20m, "USD"));
        value.TransformAmount(a => Math.Truncate(a)).ShouldBe(new MonetaryValue(-19m, "USD"));
        value.TransformAmount(a => Math.Abs(a)).ShouldBe(new MonetaryValue(19.97m, "USD"));
        new MonetaryValue(5m, "USD").TransformAmount(a => -a).ShouldBe(new MonetaryValue(-5m, "USD"));
    }

    [TestMethod]
    public void Default_ZeroResult_IsDefault()
    {
        decimal received = -1;
        var result = MonetaryValue.Default.TransformAmount(Record);

        received.ShouldBe(0m);
        result.ShouldBe(MonetaryValue.Default);
        result.IsDefault.ShouldBeTrue();

        MonetaryValue.Default.TransformAmount(_ => 0.00m).IsDefault.ShouldBeTrue();

        decimal Record(decimal amount)
        {
            received = amount;
            return amount * 3;
        }
    }

    [TestMethod]
    public void Default_NonZeroResult_ThrowsAsAddingDoes()
    {
        var expected = Should.Throw<ArgumentException>(() => MonetaryValue.Default + 5m);
        var actual = Should.Throw<ArgumentException>(() => MonetaryValue.Default.TransformAmount(a => a + 5m));

        actual.GetType().ShouldBe(expected.GetType());
        actual.Message.ShouldBe(expected.Message);
        actual.ParamName.ShouldBe(expected.ParamName);
        actual.Message.ShouldBe("Non-zero amount monetary values must have a currency associated with them.");
    }

    [TestMethod]
    public void NullFunction_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new MonetaryValue(1m, "USD").TransformAmount(null!)).ParamName.ShouldBe("transform");
        Should.Throw<ArgumentNullException>(() => MonetaryValue.Default.TransformAmount(null!)).ParamName.ShouldBe("transform");
    }

    [TestMethod]
    public void MethodGroup_Binds()
    {
        var value = new MonetaryValue(-19.97m, "USD");

        value.TransformAmount(Math.Ceiling).ShouldBe(new MonetaryValue(-19m, "USD"));
        value.TransformAmount(Math.Floor).ShouldBe(new MonetaryValue(-20m, "USD"));
        value.TransformAmount(Math.Truncate).ShouldBe(new MonetaryValue(-19m, "USD"));
        value.TransformAmount(Math.Abs).ShouldBe(new MonetaryValue(19.97m, "USD"));
        value.TransformAmount(decimal.Negate).ShouldBe(new MonetaryValue(19.97m, "USD"));
        MonetaryValue.Default.TransformAmount(Math.Ceiling).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void CallsTheFunctionOnce()
    {
        int calls = 0;
        new MonetaryValue(1m, "USD").TransformAmount(Same);
        calls.ShouldBe(1);

        calls = 0;
        MonetaryValue.Default.TransformAmount(Same);
        calls.ShouldBe(1);

        calls = 0;
        Should.Throw<ArgumentException>(() => MonetaryValue.Default.TransformAmount(One));
        calls.ShouldBe(1);

        decimal Same(decimal amount)
        {
            calls++;
            return amount;
        }

        decimal One(decimal amount)
        {
            calls++;
            return 1m;
        }
    }
}
