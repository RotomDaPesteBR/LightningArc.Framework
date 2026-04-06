using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a valid phone number (digits only).
    /// </summary>
    public record PhoneNumber : IValueObject<string>
    {
        /// <summary>
        /// Gets the numeric string value of the phone number.
        /// </summary>
        public string Value { get; }

        private PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The phone number cannot be null or empty.", nameof(value));
            }

            var numericPhone = new string(value.Where(char.IsDigit).ToArray());

            // Basic validation: length between 7 and 15 digits (E.164 standard)
            if (numericPhone.Length < 7 || numericPhone.Length > 15)
            {
                throw new ArgumentException($"The phone number '{value}' is invalid.", nameof(value));
            }

            Value = numericPhone;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PhoneNumber"/> after validation.
        /// </summary>
        /// <param name="value">The phone number string.</param>
        /// <returns>A new <see cref="PhoneNumber"/> instance.</returns>
        public static PhoneNumber Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new instance of <see cref="PhoneNumber"/>.
        /// </summary>
        /// <param name="value">The phone number string.</param>
        /// <param name="result">The resulting <see cref="PhoneNumber"/> object, or null.</param>
        /// <returns>True if created successfully; otherwise, false.</returns>
        public static bool TryCreate(string value, out PhoneNumber? result)
        {
            try
            {
                result = new PhoneNumber(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Implicitly converts a <see cref="PhoneNumber"/> object to its <see cref="string"/> representation.
        /// </summary>
        public static implicit operator string(PhoneNumber phone) => phone?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to a <see cref="PhoneNumber"/> object.
        /// </summary>
        public static implicit operator PhoneNumber(string value) => Create(value);

        /// <summary>
        /// Returns the string representation of the phone number.
        /// </summary>
        public override string ToString() => Value;
    }
}
