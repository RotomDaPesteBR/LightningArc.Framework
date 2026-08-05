using System.Collections.Generic;

namespace LightningArc.Validations;

/// <summary>
/// Represents a structured validation failure with rich metadata.
/// </summary>
public interface IValidationFailure
{
    /// <summary>
    /// Gets the property path where the validation failed.
    /// </summary>
    string PropertyPath { get; }

    /// <summary>
    /// Gets the validation failure message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the attempted value that caused the validation failure, if available.
    /// </summary>
    object? AttemptedValue { get; }

    /// <summary>
    /// Gets the severity of the validation failure (e.g., "Error", "Warning", "Info").
    /// </summary>
    string? Severity { get; }

    /// <summary>
    /// Gets additional custom data associated with the validation failure.
    /// </summary>
    IReadOnlyDictionary<string, object?>? CustomData { get; }

    /// <summary>
    /// Converts this validation failure to an <see cref="LightningArc.Results.ErrorDetail"/> for serialization.
    /// </summary>
    LightningArc.Results.ErrorDetail ToErrorDetail();
}