using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Rg"/> with the Result pattern.
    /// </summary>
    public static class RgExtensions
    {
        /// <summary>
        /// Wraps the RG in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Rg> ToResult(this Rg rg) => Result.Success(rg);

        /// <summary>
        /// Attempts to create an <see cref="Rg"/> and returns it as a Result.
        /// </summary>
        public static Result<Rg> CreateRgResult(this string value)
        {
            if (Rg.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid RG.");
        }
    }
}
