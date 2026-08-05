using System.Runtime.CompilerServices;

namespace LightningArc.Results;

/// <summary>
/// Provides asynchronous extension methods for the <see cref="Result"/> and <see cref="Result{TValue}"/> classes,
/// focused on the transformation (Map) of the internal success value.
/// </summary>
public static partial class ResultExtensions
{
    #region Result

    #region Result => Task<Result<TValue>>

    #region () => TOut

    /// <summary>
    /// Asynchronously maps a non-generic <see cref="Result"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous transformation function.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform it into a new result containing a different type of value.
    /// The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input non-generic <see cref="Result"/>.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes no arguments and returns a <see cref="Task{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the input <paramref name="result"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Result result,
        Func<Task<TOut>> mapper
    ) =>
        result.IsSuccess
            ? await result.SuccessState.MapAsync<TOut>(mapper).ConfigureAwait(false)
            : result.Error;

    #endregion

    #region () => Success<TOut>

    /// <summary>
    /// Asynchronously maps a non-generic <see cref="Result"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform it into a new result containing a different type of value,
    /// while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input non-generic <see cref="Result"/>.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes no arguments and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the input <paramref name="result"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Result result,
        Func<Task<Success<TOut>>> mapper
    ) => result.IsSuccess ? await mapper().ConfigureAwait(false) : result.Error;

    #endregion

    #endregion

    #region Task<Result> => Result<TValue>

    #region () => TOut
    /// <summary>
    /// Asynchronously maps a <see cref="Task{Result}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous transformation function.
    /// This method awaits the input <see cref="Task{Result}"/> and then applies a synchronous transformation function
    /// if the awaited result is successful. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Task{Result}"/> representing an asynchronous operation that yields a non-generic result.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes no arguments and returns a value of type <typeparamref name="TOut"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Task<Result> resultTask,
        Func<TOut> mapper
    ) => (await resultTask.ConfigureAwait(false)).Map(mapper);
    #endregion

    #region () => Success<TOut>
    /// <summary>
    /// Asynchronously maps a <see cref="Task{Result}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method awaits the input <see cref="Task{Result}"/> and then applies a synchronous transformation function
    /// if the awaited result is successful, while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Task{Result}"/> representing an asynchronous operation that yields a non-generic result.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes no arguments and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Task<Result> resultTask,
        Func<Success<TOut>> mapper
    ) => (await resultTask.ConfigureAwait(false)).Map(mapper);
    #endregion

    #endregion

    #region Task<Result> => Task<Result<TValue>>

    #region () => TOut
    /// <summary>
    /// Asynchronously maps a <see cref="Task{Result}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous transformation function.
    /// This method awaits the input <see cref="Task{Result}"/> and then applies an asynchronous transformation function
    /// if the awaited result is successful. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The input <see cref="Task{Result}"/> representing an asynchronous operation that yields a non-generic result.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes no arguments and returns a <see cref="Task{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Task<Result> resultTask,
        Func<Task<TOut>> mapper
    ) => await (await resultTask.ConfigureAwait(false)).MapAsync(mapper).ConfigureAwait(false);
    #endregion

    #region () => Success<TOut>
    /// <summary>
    /// Asynchronously maps a <see cref="Task{Result}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method awaits the input <see cref="Task{Result}"/> and then applies an asynchronous transformation function
    /// if the awaited result is successful, while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="resultTask">The input <see cref="Task{Result}"/> representing an asynchronous operation that yields a non-generic result.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes no arguments and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TOut>(
        this Task<Result> resultTask,
        Func<Task<Success<TOut>>> mapper
    ) => await (await resultTask.ConfigureAwait(false)).MapAsync(mapper).ConfigureAwait(false);
    #endregion

    #endregion

    #endregion

    #region Result<TValue>

    #region Result<TValue> => Task<Result<TValue>>

    #region TIn => TOut
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous transformation function to its contained value.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform its contained value of type <typeparamref name="TIn"/> into a new value of type <typeparamref name="TOut"/>.
    /// The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input <see cref="Result{TIn}"/>.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Task{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the input <paramref name="result"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper
    ) =>
        result.IsSuccess
            ? await result.SuccessState.MapAsync<TIn, TOut>(mapper).ConfigureAwait(false)
            : result.Error;
    #endregion

    #region TIn => Success<TOut>
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous function that returns a <see cref="Success{TOut}"/> to its contained value.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform its contained value of type <typeparamref name="TIn"/> into a new result containing a different type of value,
    /// while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input <see cref="Result{TIn}"/>.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the input <paramref name="result"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Success<TOut>>> mapper
    ) => result.IsSuccess ? await result.MapAsync(mapper).ConfigureAwait(false) : result.Error;
    #endregion

    #endregion

    #region Task<Result<TValue>> => Result<TValue>

    #region TIn => TOut
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous transformation function to its contained value.
    /// This method awaits the input <see cref="Result{TIn}"/> and then applies a synchronous transformation function
    /// if the awaited result is successful. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Result{TIn}"/> representing an asynchronous operation that yields a generic result.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a value of type <typeparamref name="TOut"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper
    ) => (await resultTask.ConfigureAwait(false)).Map(mapper);
    #endregion

    #region TIn => Success<TOut>
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method awaits the input <see cref="Result{TIn}"/> and then applies a synchronous transformation function
    /// if the awaited result is successful, while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Result{TIn}"/> representing an asynchronous operation that yields a generic result.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Success<TOut>> mapper
    )
    {
        var result = await resultTask.ConfigureAwait(false);

        return result.IsSuccess ? mapper(result.Value) : result.Error;
    }
    #endregion

    #endregion

    #region Task<Result<TValue>> => Task<Result<TValue>>

    #region TIn => TOut
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous transformation function to its contained value.
    /// This method awaits the input <see cref="Result{TIn}"/> and then applies an asynchronous transformation function
    /// if the awaited result is successful. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Result{TIn}"/> representing an asynchronous operation that yields a generic result.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Task{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> mapper
    ) => await (await resultTask.ConfigureAwait(false)).MapAsync(mapper).ConfigureAwait(false);
    #endregion

    #region TIn => Success<TOut>
    /// <summary>
    /// Asynchronously maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying an asynchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method awaits the input <see cref="Result{TIn}"/> and then applies an asynchronous transformation function
    /// if the awaited result is successful, while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="resultTask">The input <see cref="Result{TIn}"/> representing an asynchronous operation that yields a generic result.</param>
    /// <param name="mapper">
    /// The asynchronous function to apply if the awaited <paramref name="resultTask"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the asynchronous operation:
    /// If the awaited <paramref name="resultTask"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the awaited <paramref name="resultTask"/> is a failure, a <see cref="Result{TOut}"/> containing the original error is returned.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Success<TOut>>> mapper
    )
    {
        var result = await resultTask.ConfigureAwait(false);

        return result.IsSuccess ? await mapper(result.Value).ConfigureAwait(false) : result.Error;
    }
    #endregion

    #endregion

    #endregion
}
