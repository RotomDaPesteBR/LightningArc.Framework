using LightningArc.Results;

namespace LightningArc.Validations.DependencyInjection;

/// <summary>
/// Provides a DI-friendly façade for executing validators.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates an instance using a registered validator when available.
    /// </summary>
    /// <typeparam name="T">The instance type.</typeparam>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A success result when no validator is registered or when validation succeeds; otherwise a failure result.</returns>
    Result Validate<T>(T instance);
}
