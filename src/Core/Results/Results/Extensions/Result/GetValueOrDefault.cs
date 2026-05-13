namespace LightningArc.Results
{
    /// <summary>
    /// Provides extension methods for the <see cref="Result{TValue}"/> class,
    /// allowing functional composition of operations.
    /// </summary>
    public static partial class ResultExtensions
    {
        /// <summary>
        /// Returns the success value contained in the <see cref="Result{TValue}"/>.
        /// If the result is a failure, returns the provided default value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="result">The result.</param>
        /// <param name="defaultValue">The value to be returned in case of failure.</param>
        /// <returns>The success value or the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(
            this Result<TValue> result,
            TValue defaultValue
        ) => result.IsSuccess ? result.Value : defaultValue;

        /// <summary>
        /// Returns the success value contained in the <see cref="Result{TValue}"/>.
        /// If the result is a failure, returns the provided default value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="result">The result.</param>
        /// <param name="defaultValue">The value to be returned in case of failure.</param>
        /// <returns>The success value or the default value.</returns>
        public static TValue GetValueOrDefault<TValue>(
            this Result<TValue> result,
            Func<TValue> defaultValue
        ) => result.IsSuccess ? result.Value : defaultValue();

        /// <summary>
        /// Returns the success value contained in the <see cref="Result{TValue}"/>.
        /// If the result is a failure, returns the default value of <typeparamref name="TValue"/> (i.e., null for reference/nullable types, 0 for int, etc.).
        /// </summary>
        /// <typeparam name="TValue">O tipo de valor.</typeparam>
        /// <param name="result">O resultado.</param>
        /// <returns>O valor de sucesso ou o valor padr�o de <typeparamref name="TValue"/>.</returns>
        public static TValue? GetValueOrDefault<TValue>(this Result<TValue> result) =>
            result.IsSuccess ? result.Value : default;
    }
}

