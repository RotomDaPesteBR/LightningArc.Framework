using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Currency"/> with the Result pattern.
    /// </summary>
    public static class CurrencyExtensions
    {
        /// <summary>
        /// Wraps the currency in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<Currency> ToResult(this Currency currency) => Result.Success(currency);

        /// <summary>
        /// Attempts to create a <see cref="Currency"/> and returns it as a Result.
        /// </summary>
        public static Result<Currency> CreateCurrencyResult(this decimal value, string code)
        {
            if (Currency.TryCreate(value, code, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{code}' is not a valid currency code.");
        }
    }
}
