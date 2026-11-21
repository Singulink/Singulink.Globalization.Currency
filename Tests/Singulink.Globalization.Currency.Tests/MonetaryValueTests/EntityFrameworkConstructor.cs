using System.Reflection;

namespace Singulink.Globalization.Tests.MonetaryValueTests;

/// <summary>
/// Entity Framework Core binds a constructor to the mapped properties by parameter name, and it also binds private constructors. A complex property that maps
/// <see cref="MonetaryValue.CurrencyOrDefault"/> is created through the private constructor that these tests keep in place.
/// </summary>
[PrefixTestClass]
public class EntityFrameworkConstructor
{
    private static readonly Currency Usd = Currency.GetCurrency("USD");

    private static ConstructorInfo GetConstructor()
    {
        var constructor = typeof(MonetaryValue).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Currency), typeof(decimal) }, null);

        constructor.ShouldNotBeNull();
        return constructor!;
    }

    private static MonetaryValue Invoke(Currency? currency, decimal amount) => (MonetaryValue)GetConstructor().Invoke(new object?[] { currency, amount });

    [TestMethod]
    public void ParameterNames_MatchTheProperties()
    {
        foreach (var parameter in GetConstructor().GetParameters())
        {
            string propertyName = char.ToUpperInvariant(parameter.Name![0]) + parameter.Name.Substring(1);
            var property = typeof(MonetaryValue).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            property.ShouldNotBeNull($"No public property matches the parameter '{parameter.Name}'.");
            property!.PropertyType.ShouldBe(parameter.ParameterType);
        }

        GetConstructor().GetParameters().Select(p => p.Name).ShouldBe(new[] { "currencyOrDefault", "amount" }, ignoreOrder: true);
    }

    [TestMethod]
    public void PublicConstructors_DoNotBindCurrencyOrDefault()
    {
        // A mapping of CurrencyOrDefault must leave exactly one constructor that can be bound, and so must a mapping of Currency.

        foreach (var constructor in typeof(MonetaryValue).GetConstructors())
            constructor.GetParameters().ShouldNotContain(p => p.Name == "currencyOrDefault");

        GetConstructor().GetParameters().ShouldNotContain(p => p.Name == "currency");
    }

    [TestMethod]
    public void CurrencyAndAmount_CreatesTheValue()
    {
        Invoke(Usd, 12.3m).ShouldBe(new MonetaryValue(12.3m, Usd));

        var zero = Invoke(Usd, 0m);
        zero.IsDefault.ShouldBeFalse();
        zero.Currency.ShouldBeSameAs(Usd);
    }

    [TestMethod]
    public void NoCurrencyAndZeroAmount_CreatesTheDefaultValue()
    {
        Invoke(null, 0m).IsDefault.ShouldBeTrue();
        Invoke(null, 0.00m).ShouldBe(MonetaryValue.Default);
    }

    [TestMethod]
    public void NoCurrencyAndNonZeroAmount_Throws()
    {
        var ex = Should.Throw<TargetInvocationException>(() => Invoke(null, 5m));
        ex.InnerException.ShouldBeOfType<ArgumentException>();
    }
}
