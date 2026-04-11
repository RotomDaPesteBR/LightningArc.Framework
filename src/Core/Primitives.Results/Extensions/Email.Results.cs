using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods and implicit conversions for <see cref="Email"/> and <see cref="Result{TValue}"/>.
    /// </summary>
    public static class EmailResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns an <see cref="Email"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The email address string.</param>
        /// <returns>A successful result containing the email, or a validation error.</returns>
        public static Result<Email> AsEmail(this string value)
        {
            if (Email.IsValid(value, out string? errorMessage))
            {
                // Accessing internal constructor thanks to InternalsVisibleTo
                return new Email(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
