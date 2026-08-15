using System.Runtime.CompilerServices;

namespace LightningArc.Results;

/// <summary>
/// Provides extension methods for <see cref="Task{ResultAggregator}"/> that mirror the
/// instance methods of <see cref="ResultAggregator"/> for fluent async chaining.
/// </summary>
public static class ResultAggregatorTaskExtensions
{
    /// <summary>
    /// Executes a validation check asynchronously. If the check returns a failure or throws an exception,
    /// the error is aggregated into the final result.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="action">The validation check to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Any exception thrown by the factory or the returned task is caught and converted to an error.
    /// The returned task will not fault; exceptions are aggregated as errors.
    /// </remarks>
    public static async Task<ResultAggregator> Check(
        this Task<ResultAggregator> aggregatorTask,
        Func<Result> action
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Check(action);
    }

    /// <summary>
    /// Executes multiple validation checks sequentially. Each check is executed in order,
    /// and any failures or exceptions are aggregated.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="actions">A collection of validation checks to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> Check(
        this Task<ResultAggregator> aggregatorTask,
        IEnumerable<Func<Result>> actions
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Check(actions);
    }

    /// <summary>
    /// Alias for <see cref="Check(Task{ResultAggregator}, IEnumerable{Func{Result}})"/>. Executes multiple validation
    /// checks sequentially with an explicit name indicating batch semantics.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="actions">A collection of validation checks to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> CheckEach(
        this Task<ResultAggregator> aggregatorTask,
        IEnumerable<Func<Result>> actions
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.CheckEach(actions);
    }

    /// <summary>
    /// Executes a validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="factory">The function that creates the value.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> Check<T>(
        this Task<ResultAggregator> aggregatorTask,
        Func<T> factory,
        Action<T?> onSuccess
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Check(factory, onSuccess);
    }

    /// <summary>
    /// Executes an asynchronous validation check. If the check returns a failure or throws an exception,
    /// the error is aggregated into the final result.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="action">The asynchronous validation check to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Any exception thrown by the factory or the returned task is caught and converted to an error.
    /// The returned task will not fault; exceptions are aggregated as errors.
    /// </remarks>
    public static async Task<ResultAggregator> CheckAsync(
        this Task<ResultAggregator> aggregatorTask,
        Func<Task<Result>> action
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.CheckAsync(action).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes multiple asynchronous validation checks concurrently. All checks are started
    /// simultaneously and awaited together via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="actions">A collection of asynchronous validation checks to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Each check's exceptions (both synchronous factory throws and task faults) are caught and converted to errors.
    /// No check failure or exception aborts the batch — all checks run to completion.
    /// Errors are aggregated in a single pass after all tasks complete.
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<ResultAggregator> CheckAll(
        this Task<ResultAggregator> aggregatorTask,
        IEnumerable<Func<Task<Result>>> actions
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.CheckAll(actions).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes multiple asynchronous validation checks concurrently. All checks are started
    /// simultaneously and awaited together via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// Each check may have an optional per-check callback invoked when that check resolves.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="checks">A collection of async checks, each with an optional callback.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Each check's exceptions (both synchronous factory throws and task faults) are caught
    /// and converted to errors. No check failure or exception aborts the batch — all checks
    /// run to completion. Errors are aggregated in a single pass after all tasks complete.
    ///
    /// Per-check callbacks (if provided) fire as soon as each individual check resolves, with
    /// no ordering guarantee relative to other checks' callbacks. Callbacks run on arbitrary
    /// threads (no <c>ConfigureAwait(true)</c>). A throwing callback propagates normally and
    /// will fault its task, which <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
    /// will surface once awaited — this is intentional asymmetry: <see cref="AsyncCheck.Action"/>
    /// throwing is caught; <see cref="AsyncCheck.OnComplete"/> throwing is not.
    /// </remarks>
    public static async Task<ResultAggregator> CheckAll(
        this Task<ResultAggregator> aggregatorTask,
        IEnumerable<AsyncCheck> checks
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.CheckAll(checks).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="factory">The asynchronous function that creates the value.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> CheckAsync<T>(
        this Task<ResultAggregator> aggregatorTask,
        Func<Task<T>> factory,
        Action<T?> onSuccess
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.CheckAsync(factory, onSuccess).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a validation check that returns a <see cref="Result{T}"/>. If the result is a failure,
    /// the error is aggregated. If the factory throws, the exception is converted to an error.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="factory">The function that returns a <see cref="Result{T}"/>.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> Check<T>(
        this Task<ResultAggregator> aggregatorTask,
        Func<Result<T>> factory,
        Action<T?> onSuccess
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Check(factory, onSuccess);
    }

    /// <summary>
    /// Executes an asynchronous validation check that returns a <see cref="Result{T}"/>. If the result is a failure,
    /// the error is aggregated. If the factory throws, the exception is converted to an error.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="factory">The asynchronous function that returns a <see cref="Result{T}"/>.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> CheckAsync<T>(
        this Task<ResultAggregator> aggregatorTask,
        Func<Task<Result<T>>> factory,
        Action<T?> onSuccess
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.CheckAsync(factory, onSuccess).ConfigureAwait(false);
    }

    /// <summary>
    /// Ensures a condition is met. If the condition is false, the specified error is aggregated.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> Ensure(
        this Task<ResultAggregator> aggregatorTask,
        bool condition,
        Error error
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Ensure(condition, error);
    }

    /// <summary>
    /// Ensures a condition is met. If the condition is false, the error factory is invoked to create the error.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="errorFactory">Function that creates the error when the condition is false. Only invoked when condition is false.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> Ensure(
        this Task<ResultAggregator> aggregatorTask,
        bool condition,
        Func<Error> errorFactory
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Ensure(condition, errorFactory);
    }

    /// <summary>
    /// Checks a condition and aggregates an error if the condition is false.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="ResultAggregatorExtensions.Check(ResultAggregator, Func{Result})"/>/<see cref="ResultAggregatorExtensions.CheckAsync(ResultAggregator, Func{Task{Result}})"/>, this method does not catch exceptions.
    /// If <paramref name="condition"/> throws, the exception propagates normally.
    /// </remarks>
    public static async Task<ResultAggregator> When(
        this Task<ResultAggregator> aggregatorTask,
        Func<bool> condition,
        Error error
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.When(condition, error);
    }

    /// <summary>
    /// Checks an asynchronous condition and aggregates an error if the condition is false.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="condition">The asynchronous condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="ResultAggregatorExtensions.CheckAsync(ResultAggregator, Func{Task{Result}})"/>, this method does not catch exceptions.
    /// If <paramref name="condition"/> throws or faults, the exception propagates normally.
    /// </remarks>
    public static async Task<ResultAggregator> WhenAsync(
        this Task<ResultAggregator> aggregatorTask,
        Func<Task<bool>> condition,
        Error error
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.WhenAsync(condition, error).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks multiple asynchronous conditions concurrently and aggregates errors for any that evaluate to false.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <param name="conditions">A collection of conditions paired with their respective errors.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="ResultAggregatorExtensions.CheckAll(ResultAggregator, IEnumerable{Func{Task{Result}}})"/>, this method does not catch exceptions from individual conditions.
    /// If any condition throws or faults, the exception propagates via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
    /// and aborts the batch. This is by design — the <c>When*</c> family carries no exception safety net.
    /// </remarks>
    public static async Task<ResultAggregator> WhenAll(
        this Task<ResultAggregator> aggregatorTask,
        IEnumerable<WhenCondition> conditions
    )
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return await aggregator.WhenAll(conditions).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the final <see cref="Result"/> from all aggregated checks asynchronously.
    /// </summary>
    /// <param name="aggregatorTask">The task providing the aggregator.</param>
    /// <returns>A task representing the asynchronous operation, with the final <see cref="Result"/>.</returns>
    public static async Task<Result> BuildAsync(this Task<ResultAggregator> aggregatorTask)
    {
        ResultAggregator aggregator = await aggregatorTask.ConfigureAwait(false);
        return aggregator.Build();
    }
}
