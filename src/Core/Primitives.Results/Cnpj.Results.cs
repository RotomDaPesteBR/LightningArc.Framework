using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cnpj"/> with the Result pattern.
    /// </summary>
    public static class CnpjExtensions
    {
        /// <summary>
        /// Wraps the CNPJ in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Cnpj> ToResult(this Cnpj cnpj) => Result.Success(cnpj);

        /// <summary>
        /// Attempts to create a <see cref="Cnpj"/> and returns it as a Result.
        /// </summary>
        public static Result<Cnpj> CreateCnpjResult(this string value)
        {
            if (Cnpj.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid CNPJ.");
        }
    }
}
