using System.Collections.Generic;
using LightningArc.Results;

namespace LightningArc.Validations;

/// <summary>
/// Default implementation of <see cref="IValidationFailure"/>.
/// </summary>
internal sealed class ValidationFailureDetail : IValidationFailure
{
    /// <inheritdoc />
    public string PropertyPath { get; }

    /// <inheritdoc />
    public string Message { get; }

    /// <inheritdoc />
    public object? AttemptedValue { get; }

    /// <inheritdoc />
    public string? Severity { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?>? CustomData { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailureDetail"/> class.
    /// </summary>
    public ValidationFailureDetail(
        string propertyPath,
        string message,
        object? attemptedValue = null,
        string? severity = null,
        IReadOnlyDictionary<string, object?>? customData = null)
    {
        PropertyPath = propertyPath;
        Message = message;
        AttemptedValue = attemptedValue;
        Severity = severity;
        CustomData = customData;
    }

    /// <inheritdoc />
    public ErrorDetail ToErrorDetail()
    {
        var detailMessage = Message;
        if (AttemptedValue is not null)
        {
            detailMessage += $" Attempted value: {AttemptedValue}";
        }
        if (Severity is not null)
        {
            detailMessage += $" Severity: {Severity}";
        }
        if (CustomData is not null)
        {
            foreach (var kvp in CustomData)
            {
                detailMessage += $" {kvp.Key}: {kvp.Value}";
            }
        }

        return new ErrorDetail(PropertyPath, detailMessage);
    }
}