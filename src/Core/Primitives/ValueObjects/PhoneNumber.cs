using LightningArc.Primitives;
using System.Diagnostics.CodeAnalysis;

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

        internal PhoneNumber(string value)
        {
            if (!IsValid(value, out string? errorMessage))
            {
                throw new ArgumentException(errorMessage, nameof(value));
            }

            Value = new string([.. value.Where(char.IsDigit)]);
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
        public static bool TryCreate(string value, [NotNullWhen(true)] out PhoneNumber? result)
        {
            if (IsValid(value, out _))
            {
                result = new PhoneNumber(value);
                return true;
            }

            result = null;
            return false;
        }

        internal static bool IsValid(string value, [NotNullWhen(false)] out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = "The phone number cannot be null or empty.";
                return false;
            }

            string numericPhone = new([.. value.Where(char.IsDigit)]);

            // Basic validation: length between 7 and 15 digits (E.164 standard)
            if (numericPhone.Length is < 7 or > 15)
            {
                errorMessage = $"The phone number '{value}' is invalid.";
                return false;
            }

            errorMessage = null;
            return true;
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
