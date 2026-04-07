using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Password"/> with the Result pattern.
    /// </summary>
    public static class PasswordExtensions
    {
        /// <summary>
        /// Wraps the password in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Password> ToResult(this Password password) => Result.Success(password);

        /// <summary>
        /// Attempts to create a <see cref="Password"/> and returns it as a Result.
        /// </summary>
        public static Result<Password> CreatePasswordResult(this string value)
        {
            if (Password.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid password.");
        }
    }
}
