using System;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Results;

namespace LightningArc.Validations;

/// <summary>
/// Defines the contract for validators that evaluate an instance and return a <see cref="Result"/>.
/// </summary>
/// <typeparam name="T">The type of instance validated by this validator.</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the provided instance using the default rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when validation passes; otherwise, a failed <see cref="Result"/>
    /// containing one or more validation errors.
    /// </returns>
    Result Validate(T instance);

    /// <summary>
    /// Validates the provided instance using the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when validation passes; otherwise, a failed <see cref="Result"/>
    /// containing one or more validation errors.
    /// </returns>
    Result Validate(T instance, string ruleSet);

    /// <summary>
    /// Validates the provided instance with custom options.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="options">Validation options controlling execution mode, rule set, and cancellation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when validation passes; otherwise, a failed <see cref="Result"/>
    /// containing one or more validation errors.
    /// </returns>
    Result Validate(T instance, ValidationOptions options);

    /// <summary>
    /// Validates the provided instance asynchronously using the default rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the provided instance asynchronously using the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    Task<Result> ValidateAsync(T instance, string ruleSet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the provided instance asynchronously with custom options.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="options">Validation options controlling execution mode, rule set, and cancellation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    Task<Result> ValidateAsync(T instance, ValidationOptions options);
}
