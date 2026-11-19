using System.Text.Json;
using System.Text.Json.Serialization;

namespace Singulink.Globalization;

/// <summary>
/// Converts a <see cref="MonetaryValue"/> to and from JSON as an object with an amount and a currency code, i.e. <c>{ "amount": 12.34, "currency": "USD"
/// }</c>. Currencies are read from the currency registry that the converter was created with.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MonetaryValue"/> is annotated with this converter, so values serialize with the <see cref="CurrencyRegistry.Default"/> registry without any
/// setup. To read values from a different registry, add an instance created with <see cref="MonetaryValueJsonConverter(CurrencyRegistry)"/> to <see
/// cref="JsonSerializerOptions.Converters"/>, which takes precedence over the converter on the type.</para>
/// <para>
/// The amount is written as a JSON number that keeps the exact decimal value and its scale, so <c>12.30</c> stays <c>12.30</c>. The currency is written as its
/// <see cref="Currency.CurrencyCode"/>, which does not use the registry. The <see cref="MonetaryValue.Default"/> value is written as <c>null</c>, and
/// <c>null</c> is read as the default value.</para>
/// <para>
/// The property names are <c>amount</c> and <c>currency</c>. If <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> is set, the names are the policy's
/// conversion of <c>Amount</c> and <c>Currency</c> instead. Properties may appear in any order, names are matched case-insensitively if <see
/// cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/> is set, and unknown properties are skipped. A missing amount or currency, or a currency code
/// that is not in the registry, throws a <see cref="JsonException"/>.</para>
/// </remarks>
public sealed class MonetaryValueJsonConverter : JsonConverter<MonetaryValue>
{
    private static readonly PropertyNames DefaultNames = new(null, JsonEncodedText.Encode("amount"), JsonEncodedText.Encode("currency"));

    private readonly CurrencyRegistry? _registry;
    private PropertyNames? _policyNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that reads currencies from the <see cref="CurrencyRegistry.Default"/>
    /// registry.
    /// </summary>
    /// <remarks>
    /// The default registry is not accessed until the first value is read, so creating this converter does not prevent the default registry from being set at
    /// application startup.
    /// </remarks>
    public MonetaryValueJsonConverter() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonetaryValueJsonConverter"/> class that reads currencies from the specified registry.
    /// </summary>
    /// <param name="registry">The registry that currency codes are looked up in when values are read.</param>
    public MonetaryValueJsonConverter(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Reads a monetary value from an object with an amount and a currency code, or the <see cref="MonetaryValue.Default"/> value from <c>null</c>.
    /// </summary>
    /// <exception cref="JsonException">The JSON is not an object or <c>null</c>, the amount or the currency is missing or invalid, or the currency code is
    /// not in the converter's registry.</exception>
    public override MonetaryValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return default;

        if (reader.TokenType is not JsonTokenType.StartObject)
            throw new JsonException($"Expected a monetary value object or null but found a token of type '{reader.TokenType}'.");

        var names = GetNames(options);

        decimal amount = 0;
        bool hasAmount = false;
        Currency? currency = null;

        while (reader.Read() && reader.TokenType is JsonTokenType.PropertyName)
        {
            var property = MatchProperty(ref reader, names, options.PropertyNameCaseInsensitive);
            reader.Read();

            if (property is Property.Amount)
            {
                if (reader.TokenType is not JsonTokenType.Number)
                    throw new JsonException($"Expected a number for the '{names.Amount}' property but found a token of type '{reader.TokenType}'.");

                if (!reader.TryGetDecimal(out amount))
                    throw new JsonException($"The '{names.Amount}' property value is outside the range of a decimal.");

                hasAmount = true;
            }
            else if (property is Property.Currency)
            {
                if (reader.TokenType is not JsonTokenType.String)
                    throw new JsonException($"Expected a currency code string for the '{names.Currency}' property but found a token of type '{reader.TokenType}'.");

                currency = CurrencyJsonConverter.ReadCurrencyCode(ref reader, _registry ?? CurrencyRegistry.Default);
            }
            else
            {
                reader.Skip();
            }
        }

        if (!hasAmount)
            throw new JsonException($"The monetary value is missing the '{names.Amount}' property.");

        if (currency is null)
            throw new JsonException($"The monetary value is missing the '{names.Currency}' property.");

        return new MonetaryValue(amount, currency);
    }

    /// <summary>
    /// Writes a monetary value as an object with an amount and a currency code, or <c>null</c> for the <see cref="MonetaryValue.Default"/> value.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, MonetaryValue value, JsonSerializerOptions options)
    {
        if (value.IsDefault)
        {
            writer.WriteNullValue();
            return;
        }

        var names = GetNames(options);

        writer.WriteStartObject();
        writer.WriteNumber(names.Amount, value.Amount);
        writer.WriteString(names.Currency, value.CurrencyOrDefault.CurrencyCode);
        writer.WriteEndObject();
    }

    private static Property MatchProperty(ref Utf8JsonReader reader, PropertyNames names, bool caseInsensitive)
    {
        if (reader.ValueTextEquals(names.Amount.Value))
            return Property.Amount;

        if (reader.ValueTextEquals(names.Currency.Value))
            return Property.Currency;

        if (caseInsensitive)
        {
            string name = reader.GetString()!;

            if (string.Equals(name, names.Amount.Value, StringComparison.OrdinalIgnoreCase))
                return Property.Amount;

            if (string.Equals(name, names.Currency.Value, StringComparison.OrdinalIgnoreCase))
                return Property.Currency;
        }

        return Property.Unknown;
    }

    private PropertyNames GetNames(JsonSerializerOptions options)
    {
        var policy = options.PropertyNamingPolicy;

        if (policy is null)
            return DefaultNames;

        // Options cannot change once they are in use, so the names converted by the policy are cached for the last options seen.

        var names = _policyNames;

        if (names?.Options != options)
        {
            names = new PropertyNames(
                options,
                JsonEncodedText.Encode(policy.ConvertName(nameof(MonetaryValue.Amount)), options.Encoder),
                JsonEncodedText.Encode(policy.ConvertName(nameof(MonetaryValue.Currency)), options.Encoder));

            _policyNames = names;
        }

        return names;
    }

    private enum Property
    {
        Unknown,
        Amount,
        Currency,
    }

    private sealed class PropertyNames(JsonSerializerOptions? options, JsonEncodedText amount, JsonEncodedText currency)
    {
        public JsonSerializerOptions? Options { get; } = options;

        public JsonEncodedText Amount { get; } = amount;

        public JsonEncodedText Currency { get; } = currency;
    }
}
