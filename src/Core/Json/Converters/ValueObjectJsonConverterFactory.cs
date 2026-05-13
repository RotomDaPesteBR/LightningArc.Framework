using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightningArc.Primitives;

namespace LightningArc.Json.Converters
{
    /// <summary>
    /// Factory that creates a JSON converter for any type that implements <see cref="IValueObject{T}"/>.
    /// This allows Value Objects to be serialized and deserialized as their underlying primitive values.
    /// </summary>
    /// <remarks>
    /// A value object is JSON-convertible when it implements <see cref="IValueObject{T}"/> and exposes a public static Create(T value) method.
    /// </remarks>
    public sealed class ValueObjectJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return GetValueObjectInterface(typeToConvert) != null;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            Type? interfaceType = GetValueObjectInterface(typeToConvert);
            if (interfaceType == null)
            {
                throw new InvalidOperationException(
                    $"The type {typeToConvert.Name} does not implement IValueObject<T>."
                );
            }

            Type valueType = interfaceType.GetGenericArguments()[0];
            Type converterType = typeof(ValueObjectJsonConverter<,>).MakeGenericType(
                typeToConvert,
                valueType
            );

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private static Type? GetValueObjectInterface(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValueObject<>))
            {
                return type;
            }

            return type.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>)
                );
        }

        private class ValueObjectJsonConverter<TValueObject, TValue> : JsonConverter<TValueObject>
            where TValueObject : IValueObject<TValue>
        {
            private readonly MethodInfo _createMethod = typeof(TValueObject).GetMethod(
                                                            "Create",
                                                            BindingFlags.Public | BindingFlags.Static,
                                                            null,
                                                            [typeof(TValue)],
                                                            null
                                                        )
                                                        ?? throw new InvalidOperationException(
                                                            $"Type {typeof(TValueObject).Name} must have a static method 'Create({typeof(TValue).Name})'."
                                                        );

            // Looking for the static 'Create' method that takes the underlying value type as parameter.

            public override TValueObject? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options
            )
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return default;
                }

                // Deserialize the underlying value.
                TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);

                if (value == null)
                {
                    return default;
                }

                try
                {
                    // Invoke the static Create method to build the Value Object.
                    return (TValueObject)_createMethod.Invoke(null, [value])!;
                }
                catch (TargetInvocationException ex)
                    when (ex.InnerException is ArgumentException argEx)
                {
                    // Propagate validation exceptions from the Value Object as JsonException.
                    throw new JsonException(argEx.Message, argEx);
                }
                catch (Exception ex)
                {
                    throw new JsonException(
                        $"Error creating Value Object {typeof(TValueObject).Name}: {ex.Message}",
                        ex
                    );
                }
            }

            public override void Write(
                Utf8JsonWriter writer,
                TValueObject value,
                JsonSerializerOptions options
            )
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                // Serialize the inner Value directly.
                JsonSerializer.Serialize(writer, value.Value, options);
            }
        }
    }
}
