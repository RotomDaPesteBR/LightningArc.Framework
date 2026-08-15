namespace LightningArc.Results;

/// <summary>
/// Represents a condition paired with its error for use with <see cref="ResultAggregatorExtensions.WhenAll"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WhenCondition"/> struct.
/// </remarks>
/// <param name="condition">The asynchronous condition to evaluate.</param>
/// <param name="error">The error to aggregate if the condition evaluates to false.</param>
public readonly struct WhenCondition(Func<Task<bool>> condition, Error error)
{
    /// <summary>
    /// Gets the asynchronous condition to evaluate.
    /// </summary>
    public Func<Task<bool>> Condition { get; } = condition;

    /// <summary>
    /// Gets the error to aggregate if the condition evaluates to false.
    /// </summary>
    public Error Error { get; } = error;

    /// <summary>
    /// Implicitly converts a tuple to a <see cref="WhenCondition"/> for ergonomic call sites.
    /// </summary>
    /// <param name="pair">A tuple containing the condition and error.</param>
    public static implicit operator WhenCondition((Func<Task<bool>> Condition, Error Error) pair) =>
        new(pair.Condition, pair.Error);
}
