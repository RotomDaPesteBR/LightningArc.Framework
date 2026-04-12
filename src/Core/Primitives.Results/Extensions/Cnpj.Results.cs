using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cnpj"/> with the Result pattern.
    /// </summary>
    public static class CnpjResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="Cnpj"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The CNPJ string.</param>
        /// <returns>A successful result containing the CNPJ, or a validation error.</returns>
        public static Result<Cnpj> AsCnpj(this string value)
        {
            if (Cnpj.IsValid(value, out string? errorMessage))
            {
                return new Cnpj(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
