using System;

namespace LightningArc.Validations;

/// <summary>
/// Configuration options for validation execution.
/// </summary>
public sealed record ValidationOptions
{
    /// <summary>
    /// Gets the execution mode (collect all failures or fail fast on first).
    /// </summary>
    public ValidationExecutionMode ExecutionMode { get; init; } = ValidationExecutionMode.CollectAll;

    /// <summary>
    /// Gets the rule set name to execute. Default is "default".
    /// </summary>
    public string RuleSet { get; init; } = "default";

    /// <summary>
    /// Gets the cancellation token for async validation.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = default;

    /// <summary>
    /// Creates default validation options.
    /// </summary>
    public static ValidationOptions Default => new();

    /// <summary>
    /// Creates options with fail-fast execution mode.
    /// </summary>
    public static ValidationOptions FailFast => new() { ExecutionMode = ValidationExecutionMode.FailFast };

    /// <summary>
    /// Creates options for a specific rule set.
    /// </summary>
    public static ValidationOptions ForRuleSet(string ruleSet) => new() { RuleSet = ruleSet };

    /// <summary>
    /// Creates options with a cancellation token.
    /// </summary>
    public static ValidationOptions WithCancellation(CancellationToken cancellationToken) => new() { CancellationToken = cancellationToken };
}