namespace LightningArc.Results
{
    /// <summary>
    /// Provides extension methods for the <see cref="Result"/> and <see cref="Result{TValue}"/> classes,
    /// enabling the composition of functional operations (e.g., mapping, unwrapping).
    /// </summary>
    public static partial class ResultExtensions
    {
        #region Result
        /// <summary>
        /// Maps a non-generic result (without value) to a new <see cref="T:LightningArc.Results.Result`1" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The function to apply on success, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static Result<TOut> Match<TOut>(
            this Result result,
            Func<Result<TOut>> success,
            Func<Error, Result<TOut>> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success();
        }

        /// <summary>
        /// Maps a non-generic result to a new <see cref="T:LightningArc.Results.Result`1" /> where the success function returns the raw <typeparamref name="TOut" /> value,
        /// which is then wrapped in a <see cref="T:LightningArc.Results.Result`1" />.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The function to apply on success, returning the raw <typeparamref name="TOut" /> value (which is automatically wrapped).</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static Result<TOut> Match<TOut>(
            this Result result,
            Func<TOut> success,
            Func<Error, Result<TOut>> failure
        )
        {
            return !result.IsSuccess
                ? failure(result.Error)
                : Result.Success<TOut>(success(), result.SuccessState);
        }

        /// <summary>
        /// Maps a non-generic result (without value) to another non-generic <see cref="T:LightningArc.Results.Result" />.
        /// </summary>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The function to apply on success, returning <see cref="T:LightningArc.Results.Result" />.</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result" />.</returns>
        public static Result Match(
            this Result result,
            Func<Result> success,
            Func<Error, Result> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success();
        }

        /// <summary>
        /// Unwraps a non-generic result (without value) by applying one of two functions,
        /// returning the final raw value <typeparamref name="TOut" />. This method ends the Result chain.
        /// </summary>
        /// <typeparam name="TOut">The type of the output (unwrapped) value.</typeparam>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The function to apply on success, which returns <typeparamref name="TOut" />.</param>
        /// <param name="failure">The function to apply on failure, which returns <typeparamref name="TOut" />.</param>
        /// <returns>The final <typeparamref name="TOut" /> value.</returns>
        public static TOut Match<TOut>(
            this Result result,
            Func<TOut> success,
            Func<Error, TOut> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success();
        }

        /// <summary>
        /// Maps a non-generic result by applying one of two functions, where the success function
        /// receives the original success details (<see cref="T:LightningArc.Results.Success" /> object).
        /// </summary>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The function to apply to the success details, returning <see cref="T:LightningArc.Results.Result" />.</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result" />.</returns>
        public static Result Match(
            this Result result,
            Func<Success, Result> success,
            Func<Error, Result> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success(result.SuccessState);
        }

        /// <summary>
        /// Maps a generic result to a new <see cref="T:LightningArc.Results.Result`1" /> where the success function returns the raw <typeparamref name="TOut" /> value,
        /// which is then wrapped in a <see cref="T:LightningArc.Results.Result`1" />.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The function to apply to the success details, returning the raw <typeparamref name="TOut" /> value (which is automatically wrapped).</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static Result<TOut> Match<TIn, TOut>(
            this Result<TIn> result,
            Func<Success<TIn>, TOut> success,
            Func<Error, Result<TOut>> failure
        )
        {
            return !result.IsSuccess
                ? failure(result.Error)
                : Result.Success<TOut>(success(result.SuccessState), (Success)result.SuccessState);
        }

        /// <summary>
        /// Maps a generic result by applying one of two functions: one for success and one for failure.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The function to apply to the success details, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <param name="failure">The function to apply on failure, returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <returns>The new mapped <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static Result<TOut> Match<TIn, TOut>(
            this Result<TIn> result,
            Func<Success<TIn>, Result<TOut>> success,
            Func<Error, Result<TOut>> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success(result.SuccessState);
        }

        /// <summary>
        /// Unwraps a <see cref="T:LightningArc.Results.Result`1" /> by applying one of two functions,
        /// returning the final raw value <typeparamref name="TOut" />. This method ends the <see cref="T:LightningArc.Results.Result" /> chain.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output (unwrapped) value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The function to apply to the success details, which returns <typeparamref name="TOut" />.</param>
        /// <param name="failure">The function to apply on failure, which returns <typeparamref name="TOut" />.</param>
        /// <returns>The final <typeparamref name="TOut" /> value.</returns>
        public static TOut Match<TIn, TOut>(
            this Result<TIn> result,
            Func<Success<TIn>, TOut> success,
            Func<Error, TOut> failure
        )
        {
            return !result.IsSuccess ? failure(result.Error) : success(result.SuccessState);
        }
        #endregion
    }
}
