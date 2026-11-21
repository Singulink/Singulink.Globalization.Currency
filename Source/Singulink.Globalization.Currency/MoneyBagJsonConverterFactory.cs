using System.Text.Json;
using System.Text.Json.Serialization;
using Singulink.Enums;

namespace Singulink.Globalization;

/// <summary>
/// Creates converters that convert money bags to and from JSON as an array of monetary values, i.e. <c>[{ "amount": 12.34, "currency": "USD" }, {
/// "amount": 5, "currency": "EUR" }]</c>. Bags that are read are bound to the currency registry that the factory was created with.
/// </summary>
/// <remarks>
/// <para>
/// Converts <see cref="MoneyBag"/>, <see cref="SortedMoneyBag"/>, <see cref="ImmutableMoneyBag"/> and <see cref="ImmutableSortedMoneyBag"/>, and the bag
/// interfaces <see cref="IReadOnlyMoneyBag"/>, <see cref="IMoneyBag"/> and <see cref="IImmutableMoneyBag"/>. Each of these types is annotated with this
/// factory, so bags serialize with the <see cref="CurrencyRegistry.Default"/> registry without any setup. To read bags into a different registry, add an instance created with <see cref="MoneyBagJsonConverterFactory(CurrencyRegistry)"/> to <see
/// cref="JsonSerializerOptions.Converters"/>, which takes precedence over the factory on the types.</para>
/// <para>
/// Each value is written in the form chosen for the factory and read in either form, as <see cref="MonetaryValueJsonConverter"/> does, including its
/// property names and number handling. Values are read with
/// the factory's registry, not with a <see cref="MonetaryValueJsonConverter"/> in the options, so a bag and its values always share one registry. Writing does
/// not use the registry. Values are written in the bag's enumeration order, so sorted bags are written sorted by currency code.</para>
/// <para>
/// Reading adds the values to the bag as constructing a bag from them does: values in the same currency are added together, and values with a zero amount
/// are kept. A value that is not an array, an element that is not a valid monetary value (such as <c>null</c>), or a currency code that is not in the registry
/// throws a <see cref="JsonException"/>.</para>
/// <para>
/// A bag interface is read as the bag type that a collection expression of that interface type creates: <see cref="IReadOnlyMoneyBag"/> and <see
/// cref="IMoneyBag"/> as a <see cref="MoneyBag"/>, and <see cref="IImmutableMoneyBag"/> as an <see cref="ImmutableMoneyBag"/>. A bag interface is written as
/// the bag it holds.</para>
/// </remarks>
public sealed class MoneyBagJsonConverterFactory : JsonConverterFactory
{
    private readonly CurrencyRegistry? _registry;
    private readonly MonetaryJsonFormat _format;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyBagJsonConverterFactory"/> class that writes values in the object form and binds bags to the <see
    /// cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <remarks>
    /// The default registry is not accessed until the first bag is read, so creating this factory does not prevent the default registry from being set at
    /// application startup.
    /// </remarks>
    public MoneyBagJsonConverterFactory() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyBagJsonConverterFactory"/> class that binds bags to the specified registry.
    /// </summary>
    /// <param name="registry">The registry that bags are bound to and that currency codes are looked up in when bags are read.</param>
    public MoneyBagJsonConverterFactory(CurrencyRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyBagJsonConverterFactory"/> class that writes values in the specified form and binds bags to the <see
    /// cref="CurrencyRegistry.Default"/> registry.
    /// </summary>
    /// <param name="format">The form that the values in a bag are written in. Reading accepts the object and the string form, also mixed in one array.</param>
    /// <remarks>
    /// The default registry is not accessed until the first bag is read, so creating this factory does not prevent the default registry from being set at
    /// application startup.
    /// </remarks>
    /// <exception cref="ArgumentException">The format is not a defined <see cref="MonetaryJsonFormat"/> value.</exception>
    public MoneyBagJsonConverterFactory(MonetaryJsonFormat format)
    {
        if (!format.IsValid())
            throw new ArgumentException($"An undefined {nameof(MonetaryJsonFormat)} value is being used.", nameof(format));

        _format = format;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyBagJsonConverterFactory"/> class that writes values in the specified form and binds bags to the
    /// specified registry.
    /// </summary>
    /// <param name="registry">The registry that bags are bound to and that currency codes are looked up in when bags are read.</param>
    /// <param name="format">The form that the values in a bag are written in. Reading accepts the object and the string form, also mixed in one array.</param>
    /// <exception cref="ArgumentException">The format is not a defined <see cref="MonetaryJsonFormat"/> value.</exception>
    public MoneyBagJsonConverterFactory(CurrencyRegistry registry, MonetaryJsonFormat format) : this(format)
    {
        _registry = registry;
    }

    /// <summary>
    /// Determines whether the specified type is a money bag type that this factory converts.
    /// </summary>
    /// <returns><see langword="true"/> for <see cref="MoneyBag"/>, <see cref="SortedMoneyBag"/>, <see cref="ImmutableMoneyBag"/>, <see
    /// cref="ImmutableSortedMoneyBag"/>, <see cref="IReadOnlyMoneyBag"/>, <see cref="IMoneyBag"/> and <see cref="IImmutableMoneyBag"/>; otherwise <see
    /// langword="false"/>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(MoneyBag) || typeToConvert == typeof(SortedMoneyBag) ||
               typeToConvert == typeof(ImmutableMoneyBag) || typeToConvert == typeof(ImmutableSortedMoneyBag) ||
               typeToConvert == typeof(IReadOnlyMoneyBag) || typeToConvert == typeof(IMoneyBag) || typeToConvert == typeof(IImmutableMoneyBag);
    }

    /// <summary>
    /// Creates a converter for the specified money bag type.
    /// </summary>
    /// <exception cref="ArgumentException">The type is not a money bag type that this factory converts.</exception>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(MoneyBag))
            return new BagConverter<MoneyBag>(_registry, _format, static (registry, values) => new MoneyBag(registry, values));

        if (typeToConvert == typeof(SortedMoneyBag))
            return new BagConverter<SortedMoneyBag>(_registry, _format, static (registry, values) => new SortedMoneyBag(registry, values));

        if (typeToConvert == typeof(ImmutableMoneyBag))
            return new BagConverter<ImmutableMoneyBag>(_registry, _format, static (registry, values) => ImmutableMoneyBag.CreateRange(registry, values));

        if (typeToConvert == typeof(ImmutableSortedMoneyBag))
            return new BagConverter<ImmutableSortedMoneyBag>(_registry, _format, static (registry, values) => ImmutableSortedMoneyBag.CreateRange(registry, values));

        // The interfaces are read as the types that their collection builders create (see MoneyBagBuilder and the CollectionBuilder attributes).

        if (typeToConvert == typeof(IReadOnlyMoneyBag))
            return new BagConverter<IReadOnlyMoneyBag>(_registry, _format, static (registry, values) => new MoneyBag(registry, values));

        if (typeToConvert == typeof(IMoneyBag))
            return new BagConverter<IMoneyBag>(_registry, _format, static (registry, values) => new MoneyBag(registry, values));

        if (typeToConvert == typeof(IImmutableMoneyBag))
            return new BagConverter<IImmutableMoneyBag>(_registry, _format, static (registry, values) => ImmutableMoneyBag.CreateRange(registry, values));

        throw new ArgumentException($"Type '{typeToConvert}' is not a money bag type that this factory converts.", nameof(typeToConvert));
    }

    private sealed class BagConverter<TBag> : JsonConverter<TBag>
        where TBag : class, IReadOnlyMoneyBag
    {
        private readonly CurrencyRegistry? _registry;
        private readonly MonetaryValueJsonConverter _valueConverter;
        private readonly Func<CurrencyRegistry, List<MonetaryValue>, TBag> _create;

        public BagConverter(CurrencyRegistry? registry, MonetaryJsonFormat format, Func<CurrencyRegistry, List<MonetaryValue>, TBag> create)
        {
            _registry = registry;
            _valueConverter = MonetaryValueJsonConverter.Create(registry, format);
            _create = create;
        }

        public override TBag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartArray)
                throw new JsonException($"Expected an array of monetary values for a money bag but found a token of type '{reader.TokenType}'.");

            var values = new List<MonetaryValue>();

            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
            {
                values.Add(_valueConverter.Read(ref reader, typeof(MonetaryValue), options));
            }

            // Every currency was looked up in the registry that the bag is bound to, so creating the bag cannot fail.
            return _create(_registry ?? CurrencyRegistry.Default, values);
        }

        public override void Write(Utf8JsonWriter writer, TBag value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
                _valueConverter.Write(writer, item, options);

            writer.WriteEndArray();
        }
    }
}
