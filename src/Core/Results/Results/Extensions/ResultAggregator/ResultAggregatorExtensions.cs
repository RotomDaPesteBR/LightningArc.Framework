using System.Runtime.CompilerServices;
using LightningArc.Results.Exceptions;

namespace LightningArc.Results;

/// <summary>
/// Provides extension methods for <see cref="ResultAggregator"/> that mirror the
/// instance methods for fluent chaining.
/// </summary>
public static class ResultAggregatorExtensions
{
    /// <summary>
    /// Executes a validation check. If the check returns a failure or throws an exception,
    /// the error is aggregated into the final result.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="action">The validation check to execute.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Check(this ResultAggregator aggregator, Func<Result> action)
    {
        try
        {
            Result result = action();
            if (result.IsFailure)
            {
                aggregator.AppendError(result.Error);
            }
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
        }

        return aggregator;
    }

    /// <summary>
    /// Executes multiple validation checks sequentially. Each check is executed in order,
    /// and any failures or exceptions are aggregated.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="actions">A collection of validation checks to execute.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Check(
        this ResultAggregator aggregator,
        IEnumerable<Func<Result>> actions
    )
    {
        foreach (var action in actions)
        {
            _ = aggregator.Check(action);
        }
        return aggregator;
    }

    /// <summary>
    /// Alias for <see cref="Check(ResultAggregator, IEnumerable{Func{Result}})"/>. Executes multiple validation
    /// checks sequentially with an explicit name indicating batch semantics.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="actions">A collection of validation checks to execute.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator CheckEach(
        this ResultAggregator aggregator,
        IEnumerable<Func<Result>> actions
    ) => aggregator.Check(actions);

    /// <summary>
    /// Executes a validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="factory">The function that creates the value.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Check<T>(
        this ResultAggregator aggregator,
        Func<T> factory,
        Action<T?> onSuccess
    )
    {
        T value;
        try
        {
            value = factory();
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
            onSuccess(default);
            return aggregator;
        }

        onSuccess(value);
        return aggregator;
    }

    /// <summary>
    /// Executes a validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="factory">The function that creates the value.</param>
    /// <param name="value">The captured value if successful; otherwise, default.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// This method cannot be used after an awaited step in the chain (e.g., after <see cref="CheckAsync(ResultAggregator, Func{Task{Result}})"/>
    /// or <see cref="CheckAll(ResultAggregator, IEnumerable{Func{Task{Result}}})"/>), because <c>out</c> parameters
    /// are not allowed on <c>async</c> methods. The <see cref="Task{ResultAggregator}"/> extension mirrors do not include this overload.
    /// </remarks>
    public static ResultAggregator Check<T>(
        this ResultAggregator aggregator,
        Func<T> factory,
        out T? value
    )
    {
        T? captured = default;
        _ = aggregator.Check(factory, v => captured = v);
        value = captured;
        return aggregator;
    }

    /// <summary>
    /// Ensures a condition is met. If the condition is false, the specified error is aggregated.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Ensure(
        this ResultAggregator aggregator,
        bool condition,
        Error error
    )
    {
        if (!condition)
        {
            aggregator.AppendError(error);
        }
        return aggregator;
    }

    /// <summary>
    /// Ensures a condition is met. If the condition is false, the error factory is invoked to create the error.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="errorFactory">Function that creates the error when the condition is false. Only invoked when condition is false.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Ensure(
        this ResultAggregator aggregator,
        bool condition,
        Func<Error> errorFactory
    )
    {
        if (!condition)
        {
            aggregator.AppendError(errorFactory());
        }
        return aggregator;
    }

    /// <summary>
    /// Checks a condition and aggregates an error if the condition is false.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="condition">The condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="Check(ResultAggregator, Func{Result})"/>/<see cref="CheckAsync(ResultAggregator, Func{Task{Result}})"/>, this method does not catch exceptions.
    /// If <paramref name="condition"/> throws, the exception propagates normally.
    /// </remarks>
    public static ResultAggregator When(
        this ResultAggregator aggregator,
        Func<bool> condition,
        Error error
    )
    {
        if (!condition())
        {
            aggregator.AppendError(error);
        }
        return aggregator;
    }

    /// <summary>
    /// Checks an asynchronous condition and aggregates an error if the condition is false.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="condition">The asynchronous condition to check.</param>
    /// <param name="error">The error to aggregate if the condition is false.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="CheckAsync"/>, this method does not catch exceptions.
    /// If <paramref name="condition"/> throws or faults, the exception propagates normally.
    /// </remarks>
    public static async Task<ResultAggregator> WhenAsync(
        this ResultAggregator aggregator,
        Func<Task<bool>> condition,
        Error error
    )
    {
        if (!await condition().ConfigureAwait(false))
        {
            aggregator.AppendError(error);
        }
        return aggregator;
    }

    /// <summary>
    /// Checks multiple asynchronous conditions concurrently and aggregates errors for any that evaluate to false.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="conditions">A collection of conditions paired with their respective errors.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Unlike <see cref="CheckAll(ResultAggregator, IEnumerable{Func{Task{Result}}})"/>, this method does not catch exceptions from individual conditions.
    /// If any condition throws or faults, the exception propagates via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
    /// and aborts the batch. This is by design — the <c>When*</c> family carries no exception safety net.
    /// </remarks>
    public static async Task<ResultAggregator> WhenAll(
        this ResultAggregator aggregator,
        IEnumerable<WhenCondition> conditions
    )
    {
        List<WhenCondition> conditionList = [.. conditions];

        if (conditionList.Count == 0)
        {
            return aggregator;
        }

        Task<bool>[] tasks = [.. conditionList.Select(c => c.Condition())];

        bool[] results = await Task.WhenAll(tasks).ConfigureAwait(false);

        for (int i = 0; i < results.Length; i++)
        {
            if (!results[i])
            {
                aggregator.AppendError(conditionList[i].Error);
            }
        }

        return aggregator;
    }

    /// <summary>
    /// Executes an asynchronous validation check. If the check returns a failure or throws an exception,
    /// the error is aggregated into the final result.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="action">The asynchronous validation check to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Any exception thrown by the factory or the returned task is caught and converted to an error.
    /// The returned task will not fault; exceptions are aggregated as errors.
    /// </remarks>
    public static async Task<ResultAggregator> CheckAsync(
        this ResultAggregator aggregator,
        Func<Task<Result>> action
    )
    {
        Result result;
        try
        {
            result = await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
            return aggregator;
        }

        if (result.IsFailure)
        {
            aggregator.AppendError(result.Error);
        }

        return aggregator;
    }

    /// <summary>
    /// Executes multiple asynchronous validation checks concurrently. All checks are started
    /// simultaneously and awaited together via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="actions">A collection of asynchronous validation checks to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    /// <remarks>
    /// Each check's exceptions (both synchronous factory throws and task faults) are caught and converted to errors.
    /// No check failure or exception aborts the batch — all checks run to completion.
    /// Errors are aggregated in a single pass after all tasks complete.
    /// </remarks>
    [OverloadResolutionPriority(1)]
    public static async Task<ResultAggregator> CheckAll(
        this ResultAggregator aggregator,
        IEnumerable<Func<Task<Result>>> actions
    )
    {
        List<Func<Task<Result>>> actionList = [.. actions];

        if (actionList.Count == 0)
        {
            return aggregator;
        }

        var tasks = new List<Task<Result>>(actionList.Count);

        foreach (var factory in actionList)
        {
            tasks.Add(RunSafelyAsync(factory));
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                aggregator.AppendError(result.Error);
            }
        }

        return aggregator;
    }

    /// <summary>
    /// Executes an asynchronous validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="factory">The asynchronous function that creates the value.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> CheckAsync<T>(
        this ResultAggregator aggregator,
        Func<Task<T>> factory,
        Action<T?> onSuccess
    )
    {
        T value;
        try
        {
            value = await factory().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
            onSuccess(default);
            return aggregator;
        }

        onSuccess(value);
        return aggregator;
    }

    /// <summary>
    /// Executes a validation check that returns a <see cref="Result{T}"/>. If the result is a failure,
    /// the error is aggregated. If the factory throws, the exception is converted to an error.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="factory">The function that returns a <see cref="Result{T}"/>.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public static ResultAggregator Check<T>(
        this ResultAggregator aggregator,
        Func<Result<T>> factory,
        Action<T?> onSuccess
    )
    {
        Result<T> result;
        try
        {
            result = factory();
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
            onSuccess(default);
            return aggregator;
        }

        if (result.IsFailure)
        {
            aggregator.AppendError(result.Error);
            onSuccess(default);
            return aggregator;
        }

        onSuccess(result.Value);
        return aggregator;
    }

    /// <summary>
    /// Executes an asynchronous validation check that returns a <see cref="Result{T}"/>. If the result is a failure,
    /// the error is aggregated. If the factory throws, the exception is converted to an error.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="aggregator">The aggregator to extend.</param>
    /// <param name="factory">The asynchronous function that returns a <see cref="Result{T}"/>.</param>
    /// <param name="onSuccess">Action to invoke with the captured value on success.</param>
    /// <returns>A task representing the asynchronous operation, with the current <see cref="ResultAggregator"/> instance.</returns>
    public static async Task<ResultAggregator> CheckAsync<T>(
        this ResultAggregator aggregator,
        Func<Task<Result<T>>> factory,
        Action<T?> onSuccess
    )
    {
        Result<T> result;
        try
        {
            result = await factory().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            aggregator.AppendError(ex.ToError());
            onSuccess(default);
            return aggregator;
        }

        if (result.IsFailure)
        {
            aggregator.AppendError(result.Error);
            onSuccess(default);
            return aggregator;
        }

        onSuccess(result.Value);
        return aggregator;
    }

    /// <remarks>
    /// LARC032 fires on this method (see suppression below) but is a false positive here:
    /// <c>ResultExceptionHandler</c> is ASP.NET Core middleware — it only intercepts exceptions
    /// that escape an HTTP request pipeline, which has no bearing on this library-internal
    /// method. This code may run outside any HTTP context entirely (console apps, background
    /// jobs), and even under ASP.NET Core, the middleware cannot prevent a single faulted task
    /// from aborting the surrounding <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
    /// call in <see cref="CheckAll(ResultAggregator, IEnumerable{Func{Task{Result}}})"/> before the middleware would ever see anything. The
    /// try/catch here is required for <see cref="CheckAll(ResultAggregator, IEnumerable{Func{Task{Result}}})"/>'s per-check fault isolation.
    /// </remarks>
    private static async Task<Result> RunSafelyAsync(Func<Task<Result>> factory)
    {
        try
        {
            return await factory().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToError());
        }
    }

    /// <summary>
    /// Runs a single async check safely, capturing exceptions as <see cref="Result"/> failures,
    /// and invoking the optional <see cref="AsyncCheck.OnComplete"/> callback with the result
    /// the moment it resolves.
    /// </summary>
    /// <param name="check">The async check to run, including its optional callback.</param>
    /// <returns>The result of the check, or a failure result if an exception occurred.</returns>
    /// <remarks>
    /// The <see cref="AsyncCheck.OnComplete"/> callback is invoked outside the try/catch block
    /// that protects <see cref="AsyncCheck.Action"/>. A throwing callback is a caller bug and
    /// propagates normally — it is not caught or converted to an error. This asymmetry is
    /// intentional: <see cref="AsyncCheck.Action"/> throwing is an expected validation-domain
    /// outcome; <see cref="AsyncCheck.OnComplete"/> throwing is a side effect bug.
    /// </remarks>
    private static async Task<Result> RunSafelyAsync(AsyncCheck check)
    {
        Result result;
        try
        {
            result = await check.Action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            result = Result.Failure(ex.ToError());
        }

        // Deliberately outside the try/catch above: a throwing OnComplete is a caller bug,
        // not a check failure, and must not be silently folded into the aggregated Error.
        check.OnComplete?.Invoke(result);

        return result;
    }

    /// <summary>
    /// Executes multiple asynchronous validation checks concurrently. All checks are started
    /// simultaneously and awaited together via <see cref="Task.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>.
    /// Each check may have an optional per-check callback invoked when that check resolves.
    /// </summary>
    /// <param name="aggregator">The aggregator to extend.</param>
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
        this ResultAggregator aggregator,
        IEnumerable<AsyncCheck> checks
    )
    {
        List<AsyncCheck> checkList = [.. checks];

        if (checkList.Count == 0)
        {
            return aggregator;
        }

        var tasks = new List<Task<Result>>(checkList.Count);

        tasks.AddRange(checkList.Select(RunSafelyAsync));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                aggregator.AppendError(result.Error);
            }
        }

        return aggregator;
    }
}
