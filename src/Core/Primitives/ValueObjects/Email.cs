using System.Text.RegularExpressions;
using LightningArc.Primitives;

namespace LightningArc.Primitives.ValueObjects
{
    /// <summary>
    /// Represents a data type for an email address, 
    /// ensuring its validity at the time of creation through a factory method.
    /// </summary>
    /// <remarks>
    /// This 'record' is an immutable value type. Email validation is enforced 
    /// in the private constructor, accessible through the <see cref="Create(string)"/> static factory method.
    /// </remarks>
    public record Email : IValueObject<string>
    {
        /// <summary>
        /// Regular expression for email format validation.
        /// A common and robust regex to validate most valid email formats.
        /// </summary>
        private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        /// <summary>
        /// Gets the string value of the email address.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Private constructor for the Email class.
        /// This constructor performs the format validation of the email string.
        /// </summary>
        /// <param name="value">The string representing the email address.</param>
        /// <exception cref="ArgumentException">Thrown if 'value' is null, empty, or not a valid email format.</exception>
        private Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The email address cannot be null or empty.", nameof(value));
            }

            if (!Regex.IsMatch(value, EmailRegexPattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
            {
                throw new ArgumentException($"The value '{value}' is not a valid email address.", nameof(value));
            }

            Value = value;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Email"/> after validating the string format.
        /// This is the preferred public method for constructing <see cref="Email"/> objects.
        /// </summary>
        /// <param name="value">The string representing the email address.</param>
        /// <returns>A new instance of <see cref="Email"/> if the string is a valid email.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided string is not a valid email.</exception>
        public static Email Create(string value) => new(value);

        /// <summary>
        /// Tries to create a new instance of <see cref="Email"/> from the provided string.
        /// </summary>
        /// <param name="value">The string representing the email address.</param>
        /// <param name="result">The resulting <see cref="Email"/> object, or null if validation fails.</param>
        /// <returns>True if the email was successfully created; otherwise, false.</returns>
        public static bool TryCreate(string value, out Email? result)
        {
            try
            {
                result = new Email(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Implicitly converts an <see cref="Email"/> object to its <see cref="string"/> representation.
        /// </summary>
        /// <param name="email">The <see cref="Email"/> object to be converted.</param>
        /// <returns>The email address string.</returns>
        public static implicit operator string(Email email) => email?.Value ?? throw new InvalidOperationException("Cannot convert a null ValueObject to string.");

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to an <see cref="Email"/> object.
        /// This operator uses the <see cref="Create(string)"/> factory method to ensure validation.
        /// </summary>
        /// <param name="value">The string to be converted.</param>
        /// <returns>A new <see cref="Email"/> object.</returns>
        /// <exception cref="ArgumentException">Thrown if the string is not a valid email.</exception>
        public static implicit operator Email(string value) => Create(value);

        /// <summary>
        /// Returns the string representation of the email address.
        /// </summary>
        /// <returns>The email address as a string.</returns>
        public override string ToString() => Value;
    }
}
