using System.Runtime.CompilerServices;

namespace LightningArc.Results;

/// <summary>
/// Provides extension methods for the <see cref="Result"/> and <see cref="Result{TValue}"/> classes,
/// focused on the transformation (Map) of the internal success value.
/// </summary>
public static partial class ResultExtensions
{
    #region Result => Result<TValue>

    #region Result => TOut
    /// <summary>
    /// Maps a non-generic <see cref="Result"/> to a new <see cref="Result{TOut}"/> by applying a synchronous transformation function.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform it into a new result containing a different type of value.
    /// The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input non-generic <see cref="Result"/>.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes no arguments and returns a value of type <typeparamref name="TOut"/>.
    /// </param>
    /// <returns>
    /// A new <see cref="Result{TOut}"/>:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the input <paramref name="result"/> is a failure, the original error is propagated.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static Result<TOut> Map<TOut>(this Result result, Func<TOut> mapper) =>
        result.IsSuccess ? result.SuccessState.Map(mapper) : result.Error;
    #endregion

    #region Result => Success<TOut>
    /// <summary>
    /// Maps a non-generic <see cref="Result"/> to a new <see cref="Result{TOut}"/> by applying a synchronous function that returns a <see cref="Success{TOut}"/>.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform it into a new result containing a different type of value,
    /// while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input non-generic <see cref="Result"/>.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes no arguments and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A new <see cref="Result{TOut}"/>:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the input <paramref name="result"/> is a failure, the original error is propagated.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static Result<TOut> Map<TOut>(this Result result, Func<Success<TOut>> mapper) =>
        result.IsSuccess ? mapper() : result.Error;
    #endregion

    #endregion

    #region Result<TValue> => Result<TValue>

    #region TIn => TOut
    /// <summary>
    /// Maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous transformation function to its contained value.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform its contained value of type <typeparamref name="TIn"/> into a new value of type <typeparamref name="TOut"/>.
    /// The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input <see cref="Result{TIn}"/>.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a value of type <typeparamref name="TOut"/>.
    /// </param>
    /// <returns>
    /// A new <see cref="Result{TOut}"/>:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> containing the transformed value.
    /// If the input <paramref name="result"/> is a failure, the original error is propagated.
    /// </returns>
    [OverloadResolutionPriority(0)]
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper) =>
        result.IsSuccess ? result.SuccessState.Map(mapper) : result.Error;
    #endregion

    #region TIn => Success<TOut>
    /// <summary>
    /// Maps a <see cref="Result{TIn}"/> to a new <see cref="Result{TOut}"/> by applying a synchronous function that returns a <see cref="Success{TOut}"/> to its contained value.
    /// This method is used when the operation represented by the <paramref name="result"/> is successful,
    /// and you want to transform its contained value of type <typeparamref name="TIn"/> into a new result containing a different type of value,
    /// while also providing custom success details. The transformation itself is assumed to be non-failable.
    /// </summary>
    /// <typeparam name="TIn">The type of the value contained in the input <see cref="Result{TIn}"/>.</typeparam>
    /// <typeparam name="TOut">The type of the value contained in the output <see cref="Result{TOut}"/>.</typeparam>
    /// <param name="result">The input <see cref="Result{TIn}"/>.</param>
    /// <param name="mapper">
    /// The synchronous function to apply if the input <paramref name="result"/> is successful.
    /// This function takes the successful value of type <typeparamref name="TIn"/> and returns a <see cref="Success{TOut}"/>.
    /// </param>
    /// <returns>
    /// A new <see cref="Result{TOut}"/>:
    /// If the input <paramref name="result"/> is successful, a <see cref="Result{TOut}"/> encapsulating the <see cref="Success{TOut}"/> returned by the <paramref name="mapper"/>.
    /// If the input <paramref name="result"/> is a failure, the original error is propagated.
    /// </returns>
    [OverloadResolutionPriority(1)]
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Success<TOut>> mapper
    ) => result.IsSuccess ? mapper(result.Value) : result.Error;
    #endregion

    #endregion
}
