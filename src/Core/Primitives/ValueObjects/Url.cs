using LightningArc.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a valid URL.
    /// </summary>
    public record Url : IValueObject<string>
    {
        /// <summary>
        /// Gets the URL string value.
        /// </summary>
        public string Value { get; }

        internal Url(string value)
        {
            if (!IsValid(value, out string? errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Url"/> after validation.
        /// </summary>
        /// <param name="value">The URL string.</param>
        /// <returns>A new <see cref="Url"/> instance.</returns>
        public static Url Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new instance of <see cref="Url"/>.
        /// </summary>
        /// <param name="value">The URL string.</param>
        /// <param name="result">The resulting <see cref="Url"/> object, or null.</param>
        /// <returns>True if created successfully; otherwise, false.</returns>
        public static bool TryCreate(string value, [NotNullWhen(true)] out Url? result)
        {
            if (IsValid(value, out _))
            {
                result = new Url(value);
                return true;
            }

            result = null;
            return false;
        }

        internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = "The URL cannot be null or empty.";
                return false;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                errorMessage = $"The value '{value}' is not a valid absolute URL.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Url"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Url url) => url?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to a <see cref="Url"/> object.
        /// </summary>
        public static implicit operator Url(string value) => Create(value);

        /// <summary>
        /// Returns the string representation of the URL.
        /// </summary>
        public override string ToString() => Value;
    }
}
