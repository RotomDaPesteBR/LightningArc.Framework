namespace LightningArc.Validations;

/// <summary>
/// Defines how a validator processes validation rules when errors are found.
/// </summary>
public enum ValidationExecutionMode
{
    /// <summary>
    /// Executes all registered rules and aggregates every validation error.
    /// </summary>
    CollectAll = 0,

    /// <summary>
    /// Stops validation as soon as the first validation error is produced.
    /// </summary>
    FailFast = 1,
}
