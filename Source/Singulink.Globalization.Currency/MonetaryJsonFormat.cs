namespace Singulink.Globalization;

/// <summary>
/// Specifies the JSON form that <see cref="MonetaryValueJsonConverter"/> and <see cref="MoneyBagJsonConverterFactory"/> write monetary values in. Reading
/// accepts every form, whatever form is chosen for writing.
/// </summary>
public enum MonetaryJsonFormat
{
    /// <summary>
    /// An object with an amount and a currency code, i.e. <c>{ "amount": 12.34, "currency": "USD" }</c>. This is the default. Writing an amount with more
    /// than 15 significant digits as a JSON number throws an <see cref="ArgumentException"/>, because a reader that uses double-precision numbers, such as
    /// JavaScript, keeps only 15 and would read a different amount without an error.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Matches JsonValueKind.Object, the JSON term for this form")]
    Object,

    /// <summary>
    /// The same object as <see cref="Object"/>, but an amount with more than 15 significant digits is written as a JSON number with all its digits instead of
    /// throwing. Choose it only when every reader keeps all the digits of a number, such as this library or another reader into <see cref="decimal"/>.
    /// </summary>
    LossyNumberObject,

    /// <summary>
    /// A string with the value in the round-trip format <c>"O"</c>, i.e. <c>"USD 12.34"</c>.
    /// </summary>
    Text,
}
