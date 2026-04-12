using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cep"/> with the Result pattern.
    /// </summary>
    public static class CepResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="Cep"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The CEP string.</param>
        /// <returns>A successful result containing the CEP, or a validation error.</returns>
        public static Result<Cep> AsCep(this string value)
        {
            if (Cep.IsValid(value, out string? errorMessage))
            {
                return new Cep(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
