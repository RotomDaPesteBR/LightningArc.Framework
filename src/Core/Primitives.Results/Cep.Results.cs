using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cep"/> with the Result pattern.
    /// </summary>
    public static class CepExtensions
    {
        /// <summary>
        /// Wraps the CEP in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Cep> ToResult(this Cep cep) => Result.Success(cep);

        /// <summary>
        /// Attempts to create a <see cref="Cep"/> and returns it as a Result.
        /// </summary>
        public static Result<Cep> CreateCepResult(this string value)
        {
            if (Cep.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid CEP.");
        }
    }
}
