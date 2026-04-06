using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Email"/> with the Result pattern.
    /// </summary>
    public static class EmailExtensions
    {
        /// <summary>
        /// Wraps the email in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Email> ToResult(this Email email) => Result.Success(email);

        /// <summary>
        /// Attempts to create an <see cref="Email"/> and returns it as a Result.
        /// </summary>
        public static Result<Email> CreateEmailResult(this string value)
        {
            if (Email.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation($"'{value}' is not a valid email address.", nameof(value));
        }
    }
}
