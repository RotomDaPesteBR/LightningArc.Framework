using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="PhoneNumber"/> with the Result pattern.
    /// </summary>
    public static class PhoneNumberResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="PhoneNumber"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The phone number string.</param>
        /// <returns>A successful result containing the PhoneNumber, or a validation error.</returns>
        public static Result<PhoneNumber> AsPhoneNumber(this string value)
        {
            if (PhoneNumber.IsValid(value, out string? errorMessage))
            {
                return new PhoneNumber(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
