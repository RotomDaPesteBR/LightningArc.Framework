using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Url"/> with the Result pattern.
    /// </summary>
    public static class UrlResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="Url"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The URL string.</param>
        /// <returns>A successful result containing the URL, or a validation error.</returns>
        public static Result<Url> AsUrl(this string value)
        {
            if (Url.IsValid(value, out string? errorMessage))
            {
                return new Url(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
