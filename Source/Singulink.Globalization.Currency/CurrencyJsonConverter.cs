using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization;

/// <summary>
/// Converts a <see cref="Currency"/> to and from JSON as its currency code, i.e. <c>"USD"</c>. Currencies are read from the currency registry that the
/// converter was created with.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Currency"/> is annotated with this converter, so currencies serialize with the <see cref="CurrencyRegistry.Default"/> registry without any
/// setup. To read currencies from a different registry, add an instance created with <see cref="CurrencyJsonConverter(CurrencyRegistry)"/> to <see
/// cref="JsonSerializerOptions.Converters"/>, which takes precedence over the converter on the type.</para>
/// <para>
/// Writing does not use the registry: a currency is always written as its <see cref="Currency.CurrencyCode"/>. Reading a currency code that is not in the
/// registry throws a <see cref="JsonException"/>. Currencies are also supported as dictionary keys.</para>
/// </remarks>
public sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    private readonly CurrencyRegistry? _registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyJsonConverter"/> class that reads currencies from the <see cref="CurrencyRegistry.Default"/>
    /// registry.
    /// </summary>
    /// <remarks>
    /// The default registry is not accessed until the first currency is read, so creating this converter does not prevent the default registry from being set
    /// at application startup.
    /// </remarks>
    public CurrencyJsonConverter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyJsonConverter"/> class that reads currencies from the specified registry.
    /// </summary>
    /// <param name="registry">The registry that currency codes are looked up in when currencies are read.</param>
    public CurrencyJsonConverter(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Reads a currency from its currency code.
    /// </summary>
    /// <exception cref="JsonException">The value is not a string, or the currency code is not in the converter's registry.</exception>
    public override Currency? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException($"Expected a currency code string but found a token of type '{reader.TokenType}'.");

        return ReadCurrencyCode(ref reader, _registry ?? CurrencyRegistry.Default);
    }

    /// <summary>
    /// Writes the currency code of a currency.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options) => writer.WriteStringValue(value.CurrencyCode);

    /// <summary>
    /// Reads a currency from a currency code property name, such as a dictionary key.
    /// </summary>
    /// <exception cref="JsonException">The currency code is not in the converter's registry.</exception>
    public override Currency ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadCurrencyCode(ref reader, _registry ?? CurrencyRegistry.Default);
    }

    /// <summary>
    /// Writes the currency code of a currency as a property name, such as a dictionary key.
    /// </summary>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options) => writer.WritePropertyName(value.CurrencyCode);

    /// <summary>
    /// Looks up the currency code that the reader is positioned on (a string value or a property name) in the specified registry.
    /// </summary>
    internal static Currency ReadCurrencyCode(ref Utf8JsonReader reader, CurrencyRegistry registry)
    {
        Currency? currency;

#if NET9_0_OR_GREATER
        // An unescaped value has at least as many bytes as it has chars, so a buffer of the value's byte length always fits it.

        const int MaxStackLength = 64;
        int maxLength = reader.HasValueSequence ? (int)Math.Min(reader.ValueSequence.Length, int.MaxValue) : reader.ValueSpan.Length;

        if (maxLength <= MaxStackLength)
        {
            Span<char> buffer = stackalloc char[MaxStackLength];
            var code = buffer[..reader.CopyString(buffer)];

            if (!registry.TryGetCurrency(code, out currency))
                ThrowNotFound(code.ToString(), registry);

            return currency;
        }
#endif

        string codeString = reader.GetString()!;

        if (!registry.TryGetCurrency(codeString, out currency))
            ThrowNotFound(codeString, registry);

        return currency;
    }

    [DoesNotReturn]
    internal static void ThrowNotFound(string code, CurrencyRegistry registry)
    {
        throw new JsonException(registry.GetCodeNotFoundMessage(code));
    }
}
