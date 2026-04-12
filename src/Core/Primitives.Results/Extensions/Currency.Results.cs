using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="Currency"/> with the Result pattern.
    /// </summary>
    public static class CurrencyResultsExtensions
    {
        /// <summary>
        /// Validates a value and returns a <see cref="Currency"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The currency amount.</param>
        /// <param name="currencyCode">The ISO currency code (default is BRL).</param>
        /// <returns>A successful result containing the Currency, or a validation error.</returns>
        public static Result<Currency> AsCurrency(this decimal value, string currencyCode = "BRL")
        {
            if (Currency.IsValid(value, currencyCode, out string? errorMessage))
            {
                return new Currency(value, currencyCode);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
