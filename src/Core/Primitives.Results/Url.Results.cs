using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Url"/> with the Result pattern.
    /// </summary>
    public static class UrlExtensions
    {
        /// <summary>
        /// Wraps the URL in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Url> ToResult(this Url url) => Result.Success(url);

        /// <summary>
        /// Attempts to create a <see cref="Url"/> and returns it as a Result.
        /// </summary>
        public static Result<Url> CreateUrlResult(this string value)
        {
            if (Url.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid absolute URL.");
        }
    }
}
