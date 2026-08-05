using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Results;
using LightningArc.Validations.Builders;
using LightningArc.Validations.Internal;
using LightningArc.Validations.Rules;

namespace LightningArc.Validations;

/// <summary>
/// Provides a base implementation for validators composed of registered validation rules.
/// </summary>
/// <typeparam name="T">The type of instance validated by this validator.</typeparam>
public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly Dictionary<string, List<IValidationRule<T>>> _ruleSets = new(StringComparer.OrdinalIgnoreCase);
    private string _currentRuleSet = "default";

    /// <summary>
    /// Gets the execution mode used by this validator.
    /// </summary>
    protected ValidationExecutionMode ExecutionMode { get; private set; } = ValidationExecutionMode.CollectAll;

    /// <summary>
    /// Gets the rules for the specified rule set, creating the set if it doesn't exist.
    /// </summary>
    private List<IValidationRule<T>> GetRules(string ruleSet)
    {
        if (!_ruleSets.TryGetValue(ruleSet, out List<IValidationRule<T>>? rules))
        {
            rules = [];
            _ruleSets[ruleSet] = rules;
        }
        return rules;
    }

    /// <summary>
    /// Validates the provided instance against all registered rules in the default rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when all rules pass; otherwise, a failed <see cref="Result"/>
    /// containing the aggregated validation errors.
    /// </returns>
    public Result Validate(T instance) => Validate(instance, "default");

    /// <summary>
    /// Validates the provided instance against all registered rules in the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when all rules pass; otherwise, a failed <see cref="Result"/>
    /// containing the aggregated validation errors.
    /// </returns>
    public Result Validate(T instance, string ruleSet)
    {
        Error? aggregated = null;

        foreach (ValidationFailure failure in ValidateFailures(instance, ruleSet))
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
    /// Validates the provided instance against all registered rules with custom options.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="options">Validation options controlling execution mode, rule set, and cancellation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when all rules pass; otherwise, a failed <see cref="Result"/>
    /// containing the aggregated validation errors.
    /// </returns>
    public Result Validate(T instance, ValidationOptions options)
    {
        ValidationExecutionMode previousMode = ExecutionMode;
        ExecutionMode = options.ExecutionMode;
        try
        {
            Error? aggregated = null;

            foreach (ValidationFailure failure in ValidateFailures(instance, options.RuleSet, options.ExecutionMode == ValidationExecutionMode.FailFast))
            {
                Error error = failure.ToError();
                aggregated = aggregated is null ? error : FlattenIfNeeded(aggregated + error);

                if (options.ExecutionMode == ValidationExecutionMode.FailFast)
                {
                    return aggregated;
                }
            }

            return aggregated is null ? Result.Success() : aggregated;
        }
        finally
        {
            ExecutionMode = previousMode;
        }
    }

    /// <summary>
    /// Validates the provided instance asynchronously against all registered rules in the default rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    public Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default)
        => ValidateAsync(instance, "default", cancellationToken);

    /// <summary>
    /// Validates the provided instance asynchronously against all registered rules in the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    public async Task<Result> ValidateAsync(T instance, string ruleSet, CancellationToken cancellationToken = default)
    {
        Error? aggregated = null;

        await foreach (ValidationFailure failure in ValidateFailuresAsync(instance, ruleSet, cancellationToken))
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
    /// Validates the provided instance asynchronously against all registered rules with custom options.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="options">Validation options controlling execution mode, rule set, and cancellation.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// </returns>
    public async Task<Result> ValidateAsync(T instance, ValidationOptions options)
    {
        ValidationExecutionMode previousMode = ExecutionMode;
        ExecutionMode = options.ExecutionMode;
        try
        {
            Error? aggregated = null;

            await foreach (ValidationFailure failure in ValidateFailuresAsync(instance, options.RuleSet, options.CancellationToken, options.ExecutionMode == ValidationExecutionMode.FailFast))
            {
                Error error = failure.ToError();
                aggregated = aggregated is null ? error : FlattenIfNeeded(aggregated + error);

                if (options.ExecutionMode == ValidationExecutionMode.FailFast)
                {
                    return aggregated;
                }
            }

            return aggregated is null ? Result.Success() : aggregated;
        }
        finally
        {
            ExecutionMode = previousMode;
        }
    }

    /// <summary>
    /// Enumerates the validation failures produced by the registered rules in the default rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="stopOnFirstFailure">When <see langword="true"/>, stops after the first failure if the validator runs in fail-fast mode.</param>
    /// <returns>The validation failures yielded by all registered rules.</returns>
    internal IEnumerable<ValidationFailure> ValidateFailures(T instance, bool stopOnFirstFailure = false)
        => ValidateFailures(instance, "default", stopOnFirstFailure);

    /// <summary>
    /// Enumerates the validation failures produced by the registered rules in the specified rule set.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <param name="stopOnFirstFailure">When <see langword="true"/>, stops after the first failure if the validator runs in fail-fast mode.</param>
    /// <returns>The validation failures yielded by all registered rules.</returns>
    internal IEnumerable<ValidationFailure> ValidateFailures(T instance, string ruleSet, bool stopOnFirstFailure = false)
    {
        foreach (var rule in GetRules(ruleSet))
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
    /// Enumerates the validation failures produced by the registered rules in the default rule set asynchronously.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <param name="stopOnFirstFailure">When <see langword="true"/>, stops after the first failure if the validator runs in fail-fast mode.</param>
    /// <returns>The validation failures yielded by all registered rules.</returns>
    internal async IAsyncEnumerable<ValidationFailure> ValidateFailuresAsync(
        T instance,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        bool stopOnFirstFailure = false)
    {
        await foreach (var failure in ValidateFailuresAsync(instance, "default", cancellationToken, stopOnFirstFailure))
        {
            yield return failure;
        }
    }

    /// <summary>
    /// Enumerates the validation failures produced by the registered rules in the specified rule set asynchronously.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="ruleSet">The name of the rule set to execute.</param>
    /// <param name="cancellationToken">A token to cancel the validation.</param>
    /// <param name="stopOnFirstFailure">When <see langword="true"/>, stops after the first failure if the validator runs in fail-fast mode.</param>
    /// <returns>The validation failures yielded by all registered rules.</returns>
    internal async IAsyncEnumerable<ValidationFailure> ValidateFailuresAsync(
        T instance,
        string ruleSet,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        bool stopOnFirstFailure = false)
    {
        foreach (var rule in GetRules(ruleSet))
        {
            await foreach (ValidationFailure failure in rule.ValidateAsync(instance, cancellationToken))
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

    private protected void Register(IValidationRule<T> rule)
    {
        GetRules(_currentRuleSet).Add(rule);
    }

    /// <summary>
    /// Defines a named rule set that groups related validation rules.
    /// </summary>
    /// <param name="name">The name of the rule set.</param>
    /// <param name="configure">The action that configures rules for this rule set.</param>
    protected void RuleSet(string name, Action configure)
    {
        string previous = _currentRuleSet;
        _currentRuleSet = name;
        try
        {
            configure();
        }
        finally
        {
            _currentRuleSet = previous;
        }
    }
}
