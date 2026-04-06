using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Cpf"/> with the Result pattern.
    /// </summary>
    public static class CpfExtensions
    {
        /// <summary>
        /// Wraps the CPF in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Cpf> ToResult(this Cpf cpf) => Result.Success(cpf);

        /// <summary>
        /// Attempts to create a <see cref="Cpf"/> and returns it as a Result.
        /// </summary>
        public static Result<Cpf> CreateCpfResult(this string value)
        {
            if (Cpf.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation($"'{value}' is not a valid CPF.", nameof(value));
        }
    }
}
