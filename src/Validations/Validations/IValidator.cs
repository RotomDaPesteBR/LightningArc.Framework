using LightningArc.Results;

namespace LightningArc.Validations;

/// <summary>
/// Defines the contract for validators that evaluate an instance and return a <see cref="Result"/>.
/// </summary>
/// <typeparam name="T">The type of instance validated by this validator.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the provided instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when validation passes; otherwise, a failed <see cref="Result"/>
    /// containing one or more validation errors.
    /// </returns>
    Result Validate(T instance);
}
