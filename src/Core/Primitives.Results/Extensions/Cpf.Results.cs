using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cpf"/> with the Result pattern.
    /// </summary>
    public static class CpfResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns a <see cref="Cpf"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The CPF string.</param>
        /// <returns>A successful result containing the CPF, or a validation error.</returns>
        public static Result<Cpf> AsCpf(this string value)
        {
            if (Cpf.IsValid(value, out string? errorMessage))
            {
                return new Cpf(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
