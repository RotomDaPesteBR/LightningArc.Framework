using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Rg"/> with the Result pattern.
    /// </summary>
    public static class RgResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns an <see cref="Rg"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The RG string.</param>
        /// <returns>A successful result containing the RG, or a validation error.</returns>
        public static Result<Rg> AsRg(this string value)
        {
            if (Rg.IsValid(value, out string? errorMessage))
            {
                return new Rg(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
