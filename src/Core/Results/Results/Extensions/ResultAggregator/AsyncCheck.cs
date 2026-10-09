using System;
using System.Threading.Tasks;

namespace LightningArc.Results;

/// <summary>
/// Pairs an async check with an optional callback invoked with that specific check's
/// <see cref="Result"/> the moment it resolves — independent of when the rest of the batch
/// completes.
/// </summary>
/// <param name="action">The asynchronous action to execute.</param>
/// <param name="onComplete">Optional callback invoked with the check's <see cref="Result"/> when it resolves.</param>
public readonly struct AsyncCheck(Func<Task<Result>> action, Action<Result>? onComplete = null)
{
    /// <summary>
    /// Gets the asynchronous action to execute.
    /// </summary>
    public Func<Task<Result>> Action { get; } =
        action ?? throw new ArgumentNullException(nameof(action));

    /// <summary>
    /// Gets the optional callback invoked with the check's <see cref="Result"/> when it resolves.
    /// </summary>
    public Action<Result>? OnComplete { get; } = onComplete;

    /// <summary>
    /// Allows existing <c>IEnumerable&lt;Func&lt;Task&lt;Result&gt;&gt;&gt;</c> call sites to
    /// keep compiling unchanged against the new overload — a bare delegate becomes an
    /// <see cref="AsyncCheck"/> with no callback.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    public static implicit operator AsyncCheck(Func<Task<Result>> action) => new(action);
}
