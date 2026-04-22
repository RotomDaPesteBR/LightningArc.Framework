using LightningArc.Results;
using LightningArc.Validations.Builders;
using LightningArc.Validations.Internal;
using LightningArc.Validations.Rules;
using System.Linq.Expressions;

namespace LightningArc.Validations;

/// <summary>
/// Provides a base implementation for validators composed of registered validation rules.
/// </summary>
/// <typeparam name="T">The type of instance validated by this validator.</typeparam>
public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    /// <summary>
    /// Gets the execution mode used by this validator.
    /// </summary>
    protected ValidationExecutionMode ExecutionMode { get; private set; } = ValidationExecutionMode.CollectAll;

    /// <summary>
    /// Validates the provided instance against all registered rules.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when all rules pass; otherwise, a failed <see cref="Result"/>
    /// containing the aggregated validation errors.
    /// </returns>
    public Result Validate(T instance)
    {
        Error? aggregated = null;

        foreach (ValidationFailure failure in ValidateFailures(instance))
        {
            Error error = failure.ToError();
            aggregated = aggregated is null ? error : FlattenIfNeeded(aggregated + error);

            if (ExecutionMode == ValidationExecutionMode.FailFast)
            {
                return aggregated;
            }
        }

        return aggregated is null ? Result.Success() : aggregated;
    }

    /// <summary>
    /// Enumerates the validation failures produced by the registered rules.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="stopOnFirstFailure">When <see langword="true"/>, stops after the first failure if the validator runs in fail-fast mode.</param>
    /// <returns>The validation failures yielded by all registered rules.</returns>
    internal IEnumerable<ValidationFailure> ValidateFailures(T instance, bool stopOnFirstFailure = false)
    {
        foreach (var rule in _rules)
        {
            foreach (ValidationFailure failure in rule.Validate(instance))
            {
                yield return failure;

                if (stopOnFirstFailure && ExecutionMode == ValidationExecutionMode.FailFast)
                {
                    yield break;
                }
            }
        }
    }

    /// <summary>
    /// Configures the validator to stop when the first validation error is produced.
    /// </summary>
    protected void UseFailFast() => ExecutionMode = ValidationExecutionMode.FailFast;

    /// <summary>
    /// Registers a rule that validates a specific property selected from the instance.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated.</typeparam>
    /// <param name="expression">The expression used to access the property and infer its member path.</param>
    /// <returns>A builder used to configure validators for the selected property.</returns>
    protected PropertyRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var accessor = expression.Compile();
        string path = MemberPath.GetPath(expression);

        var rule = new PropertyValidationRule<T, TProperty>(accessor, path);
        Register(rule);

        return new PropertyRuleBuilder<T, TProperty>(rule, Register);
    }

    /// <summary>
    /// Registers a rule that validates each element in a collection selected from the instance.
    /// </summary>
    /// <typeparam name="TElement">The type of element contained in the collection.</typeparam>
    /// <param name="expression">The expression used to access the collection and infer its member path.</param>
    /// <returns>A builder used to configure validation for each collection element.</returns>
    protected CollectionRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>?>> expression)
        where TElement : class
    {
        var accessor = expression.Compile();
        string path = MemberPath.GetPath(expression);

        return new CollectionRuleBuilder<T, TElement>(Register, accessor, path);
    }

    /// <summary>
    /// Registers a rule that validates the full object using a predicate.
    /// </summary>
    /// <param name="predicate">The predicate that determines whether the instance is valid.</param>
    /// <param name="message">The validation message to use when the rule fails.</param>
    /// <param name="errorFactory">The factory that materializes the final <see cref="Error"/>.</param>
    /// <param name="path">The optional path associated with the validation failure.</param>
    protected void Rule(
        Func<T, bool> predicate,
        string message,
        Func<string, string, Error> errorFactory,
        string? path = null)
    {
        Register(new ObjectValidationRule<T>(predicate, path, message, errorFactory));
    }

    private static Error FlattenIfNeeded(Error error) =>
        error is AggregateError aggregate ? aggregate.Flatten() : error;

    private protected void Register(IValidationRule<T> rule) => _rules.Add(rule);
}
