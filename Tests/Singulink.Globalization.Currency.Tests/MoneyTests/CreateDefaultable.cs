namespace Singulink.Globalization.Tests.MoneyTests;

[PrefixTestClass]
public class CreateDefaultable
{
    [TestMethod]
    public void NonZeroAmountWithNullCurrency_Throws()
    {
        Should.Throw<ArgumentException>(() => Money.CreateDefaultable(123, (Currency)null));
    }

    [TestMethod]
    public void NonZeroAmountWithNullCurrencyCode_Throws()
    {
        Should.Throw<ArgumentException>(() => Money.CreateDefaultable(123, (string)null));
    }
}