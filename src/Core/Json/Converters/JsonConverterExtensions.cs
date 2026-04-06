using System.Text.Json.Serialization;
using LightningArc.Primitives.ValueObjects;

namespace LightningArc.Json.Converters
{
    /// <summary>
    /// Extension methods for JsonConverter.
    /// </summary>
    public static class JsonConverterExtensions
    {
        /// <summary>
        /// Adds and configures JsonConverters for ValueObjects.
        /// </summary>
        /// <param name="converters">The collection of converters to be configured.</param>
        /// <returns>The same collection of converters with the infrastructure services added.</returns>
        public static ICollection<JsonConverter> AddJsonConverters(this ICollection<JsonConverter> converters)
        {
            converters.Add(new ValueObjectJsonConverterFactory());

            return converters;
        }
    }
}

