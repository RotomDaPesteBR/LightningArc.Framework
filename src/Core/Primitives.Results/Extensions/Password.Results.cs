using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Password"/> with the Result pattern.
    /// </summary>
    public static class PasswordResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="Password"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The password string.</param>
        /// <param name="minimumStrength">The minimum required strength (default is Moderate).</param>
        /// <returns>A successful result containing the Password, or a validation error.</returns>
        public static Result<Password> AsPassword(this string value, PasswordStrength minimumStrength = PasswordStrength.Moderate)
        {
            if (Password.IsValid(value, minimumStrength, out string? errorMessage))
            {
                return new Password(value, minimumStrength);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
