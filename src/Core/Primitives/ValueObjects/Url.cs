using LightningArc.Primitives;

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

        private Url(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A URL não pode ser nula ou vazia.", nameof(value));
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                throw new ArgumentException($"O valor '{value}' não é uma URL absoluta válida.", nameof(value));
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
        public static bool TryCreate(string value, out Url? result)
        {
            try
            {
                result = new Url(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="Url"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(Url url) => url?.Value ?? throw new InvalidOperationException("Não é possível converter um ValueObject nulo para string.");

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
