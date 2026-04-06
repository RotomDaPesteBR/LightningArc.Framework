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
    public class ValueObjectJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return GetValueObjectInterface(typeToConvert) != null;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var interfaceType = GetValueObjectInterface(typeToConvert);
            if (interfaceType == null)
            {
                throw new InvalidOperationException($"The type {typeToConvert.Name} does not implement IValueObject<T>.");
            }

            var valueType = interfaceType.GetGenericArguments()[0];
            var converterType = typeof(ValueObjectJsonConverter<,>).MakeGenericType(typeToConvert, valueType);

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private static Type? GetValueObjectInterface(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValueObject<>))
            {
                return type;
            }

            return type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValueObject<>));
        }

        private class ValueObjectJsonConverter<TValueObject, TValue> : JsonConverter<TValueObject>
            where TValueObject : IValueObject<TValue>
        {
            private readonly MethodInfo _createMethod;

            public ValueObjectJsonConverter()
            {
                // Looking for the static 'Create' method that takes the underlying value type as parameter.
                _createMethod = typeof(TValueObject).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(TValue) }, null)
                                ?? throw new InvalidOperationException($"O tipo {typeof(TValueObject).Name} deve possuir um método estático 'Create({typeof(TValue).Name})'.");
            }

            public override TValueObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    return (TValueObject)_createMethod.Invoke(null, new object[] { value })!;
                }
                catch (TargetInvocationException ex) when (ex.InnerException is ArgumentException argEx)
                {
                    // Propagate validation exceptions from the Value Object as JsonException.
                    throw new JsonException(argEx.Message, argEx);
                }
                catch (Exception ex)
                {
                    throw new JsonException($"Erro ao criar o Value Object {typeof(TValueObject).Name}: {ex.Message}", ex);
                }
            }

            public override void Write(Utf8JsonWriter writer, TValueObject value, JsonSerializerOptions options)
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
