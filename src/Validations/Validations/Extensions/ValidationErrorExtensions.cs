using System.Collections.Generic;
using LightningArc.Results;

namespace LightningArc.Validations;

/// <summary>
/// Extension methods for creating rich validation errors.
/// </summary>
public static class ValidationErrorExtensions
{
    /// <summary>
    /// Creates a validation error with structured failure details.
    /// </summary>
    /// <param name="validation">The validation error module.</param>
    /// <param name="message">The validation message.</param>
    /// <param name="failures">The structured validation failures.</param>
    /// <returns>A new <see cref="Error"/> with validation failure details.</returns>
    public static Error WithFailures(
        this Error.Validation validation,
        string message,
        params IValidationFailure[] failures)
    {
        var details = new List<ErrorDetail>();
        foreach (var failure in failures)
        {
            details.Add(failure.ToErrorDetail());
        }
        return Error.Validation.InvalidParameter(message, details);
    }

    /// <summary>
    /// Creates a validation error with a single structured failure.
    /// </summary>
    /// <param name="validation">The validation error module.</param>
    /// <param name="message">The validation message.</param>
    /// <param name="propertyPath">The property path where validation failed.</param>
    /// <param name="attemptedValue">The attempted value that caused the failure.</param>
    /// <param name="severity">The severity of the failure.</param>
    /// <param name="customData">Additional custom data.</param>
    /// <returns>A new <see cref="Error"/> with validation failure details.</returns>
    public static Error WithFailure(
        this Error.Validation validation,
        string message,
        string propertyPath,
        object? attemptedValue = null,
        string? severity = null,
        IReadOnlyDictionary<string, object?>? customData = null)
    {
        var failure = new ValidationFailureDetail(propertyPath, message, attemptedValue, severity, customData);
        return WithFailures(validation, message, failure);
    }
}