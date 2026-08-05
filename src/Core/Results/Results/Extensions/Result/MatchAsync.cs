namespace LightningArc.Results
{
    /// <summary>
    /// Provides extension methods for the <see cref="Result"/> and <see cref="Result{TValue}"/> classes,
    /// enabling the composition of functional and asynchronous operations.
    /// </summary>
    public static partial class ResultExtensions
    {
        #region Result
        /// <summary>
        /// Maps a non-generic result (without value) by applying asynchronous functions,
        /// returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.
        /// </summary>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The asynchronous function to apply on success, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <param name="failure">The asynchronous function to apply on failure, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <returns>A Task containing the new <see cref="T:LightningArc.Results.Result" />.</returns>
        public static async Task<Result> MatchAsync(
            this Result result,
            Func<Task<Result>> success,
            Func<Error, Task<Result>> failure
        ) =>
            result.IsSuccess
                ? await success().ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Maps a non-generic result (without value) using a synchronous success function,
        /// but with an asynchronous failure function. Returns a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.
        /// </summary>
        /// <param name="result">The input result (without value).</param>
        /// <param name="success">The synchronous function to apply on success, returning a <see cref="T:LightningArc.Results.Result" />.</param>
        /// <param name="failure">The asynchronous function to apply on failure, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <returns>A Task containing the new <see cref="T:LightningArc.Results.Result" />.</returns>
        public static async Task<Result> MatchAsync(
            this Result result,
            Func<Success> success,
            Func<Error, Task<Result>> failure
        ) => result.IsSuccess ? success() : await failure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see> and maps the result using asynchronous functions,
        /// returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.
        /// </summary>
        /// <param name="resultTask">The Task containing the input result.</param>
        /// <param name="success">The asynchronous function to apply on success, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <param name="failure">The asynchronous function to apply on failure, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <returns>A Task containing the new <see cref="T:LightningArc.Results.Result" />.</returns>
        public static async Task<Result> MatchAsync(
            this Task<Result> resultTask,
            Func<Task<Success>> success,
            Func<Error, Task<Result>> failure
        )
        {
            Result result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? await success().ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see> and maps the result using synchronous functions for success,
        /// with an asynchronous failure function. Returns a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.
        /// </summary>
        /// <param name="resultTask">The Task containing the input result.</param>
        /// <param name="success">The synchronous function to apply on success, returning a <see cref="T:LightningArc.Results.Result" />.</param>
        /// <param name="failure">The asynchronous function to apply on failure, returning a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.</param>
        /// <returns>A Task containing the new <see cref="T:LightningArc.Results.Result" />.</returns>
        public static async Task<Result> MatchAsync(
            this Task<Result> resultTask,
            Func<Success> success,
            Func<Error, Task<Result>> failure
        )
        {
            Result result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess ? success() : await failure(result.Error).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see> and maps the result. The success function receives the original <see cref="T:LightningArc.Results.Result" />
        /// (for re-sending), and failure receives the <see cref="T:LightningArc.Results.Error" />, both returning <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see>.
        /// </summary>
        public static async Task<Result> MatchAsync(
            this Task<Result> resultTask,
            Func<Success, Task<Success>> success,
            Func<Error, Task<Result>> failure
        )
        {
            Result result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);
        }

        /// <summary>
        /// Maps a synchronous <see cref="T:LightningArc.Results.Result" /> using a delegate that receives the original result.
        /// </summary>
        public static async Task<Result> MatchAsync(
            this Result result,
            Func<Success, Task<Success>> success,
            Func<Error, Task<Result>> failure
        ) =>
            result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&gt;</see> and maps the result using asynchronous functions, where success returns a <see cref="T:LightningArc.Results.Result`1" />.
        /// The success function receives the success details (<see cref="T:LightningArc.Results.Success" />).
        /// </summary>
        /// <typeparam name="TOut">The type of the output value (if successful).</typeparam>
        /// <param name="resultTask">The Task containing the input result.</param>
        /// <param name="success">The asynchronous function to apply to the success details, which returns a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see>.</param>
        /// <param name="failure">The asynchronous function to apply to the error.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see> containing the new <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static async Task<Result<TOut>> MatchAsync<TOut>(
            this Task<Result> resultTask,
            Func<Success, Task<Success<TOut>>> success,
            Func<Error, Task<Result<TOut>>> failure
        )
        {
            Result result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);
        }

        /// <summary>
        /// Maps a non-generic result to a generic result by applying asynchronous functions.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The asynchronous function to apply on success.</param>
        /// <param name="failure">The asynchronous function to apply on failure.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see> containing the new <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static async Task<Result<TOut>> MatchAsync<TOut>(
            this Result result,
            Func<Success, Task<Success<TOut>>> success,
            Func<Error, Task<Result<TOut>>> failure
        ) =>
            result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Maps a non-generic result to a generic result by applying asynchronous functions.
        /// </summary>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The asynchronous function to apply on success.</param>
        /// <param name="failure">The asynchronous function to apply on failure.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see> containing the new <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static async Task<Result<TOut>> MatchAsync<TOut>(
            this Result result,
            Func<Success, Task<Result<TOut>>> success,
            Func<Error, Task<Result<TOut>>> failure
        ) =>
            result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);
        #endregion

        #region Result<TValue>
        /// <summary>
        /// Maps the result by applying one of two asynchronous functions: one for success and one for failure.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="result">The input result.</param>
        /// <param name="success">The asynchronous function to apply to the success details (<see cref="T:LightningArc.Results.Success`1" />).</param>
        /// <param name="failure">The asynchronous function to apply to the error.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see></returns>
        public static async Task<Result<TOut>> MatchAsync<TIn, TOut>(
            this Result<TIn> result,
            Func<Success<TIn>, Task<Success<TOut>>> success,
            Func<Error, Task<Result<TOut>>> failure
        ) =>
            result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TIn&gt;&gt;</see> and maps the result
        /// using asynchronous functions for success and failure.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="resultTask">The Task containing the input result.</param>
        /// <param name="success">The asynchronous function to apply to the success details (<see cref="T:LightningArc.Results.Success`1" />), returning <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see>.</param>
        /// <param name="failure">The asynchronous function to apply to the error, returning the failure <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see>.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see> containing the new <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static async Task<Result<TOut>> MatchAsync<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<Success<TIn>, Task<Success<TOut>>> success,
            Func<Error, Task<Result<TOut>>> failure
        )
        {
            Result<TIn> result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : await failure(result.Error).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits a <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TIn&gt;&gt;</see> and maps the result
        /// using synchronous functions for success, returning a <see cref="T:LightningArc.Results.Result`1" />.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <typeparam name="TOut">The type of the output value.</typeparam>
        /// <param name="resultTask">The Task containing the input result.</param>
        /// <param name="success">The synchronous function to apply to the success details (<see cref="T:LightningArc.Results.Success`1" />), returning <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <param name="failure">The synchronous function to apply to the error, returning the failure <see cref="T:LightningArc.Results.Result`1" />.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1">Task&lt;Result&lt;TOut&gt;&gt;</see> containing the new <see cref="T:LightningArc.Results.Result`1" />.</returns>
        public static async Task<Result<TOut>> MatchAsync<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<Success<TIn>, Success<TOut>> success,
            Func<Error, Result<TOut>> failure
        )
        {
            Result<TIn> result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? (Result<TOut>)success(result.SuccessState)
                : failure(result.Error);
        }

        /// <summary>
        /// Maps the synchronous result (<see cref="T:LightningArc.Results.Result`1" />) using an asynchronous success function
        /// that returns a <see cref="T:LightningArc.Results.Result`1" /> and a synchronous failure function.
        /// </summary>
        public static async Task<Result<TOut>> MatchAsync<TIn, TOut>(
            this Result<TIn> result,
            Func<Success<TIn>, Task<Success<TOut>>> success,
            Func<Error, Result<TOut>> failure
        ) =>
            result.IsSuccess
                ? await success(result.SuccessState).ConfigureAwait(false)
                : failure(result.Error);

        /// <summary>
        /// Awaits the asynchronous input result and maps it using a synchronous success function (returns <see cref="T:LightningArc.Results.Result`1" />)
        /// and an asynchronous failure function.
        /// </summary>
        public static async Task<Result<TOut>> MatchAsync<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<Success<TIn>, Success<TOut>> success,
            Func<Error, Task<Result<TOut>>> failure
        )
        {
            Result<TIn> result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess
                ? success(result.SuccessState)
                : await failure(result.Error).ConfigureAwait(false);
        }
        #endregion
    }
}
