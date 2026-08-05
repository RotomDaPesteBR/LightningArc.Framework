using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Results;
using LightningArc.Validations.Internal;
using LightningArc.Validations.Rules;

namespace LightningArc.Validations.Builders;

/// <summary>
/// Builds validation rules for a specific property.
/// </summary>
/// <typeparam name="T">The type that owns the property being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
public sealed partial class PropertyRuleBuilder<T, TProperty>
{
    internal PropertyRuleBuilder(
        PropertyValidationRule<T, TProperty> rule,
        Action<IValidationRule<T>> registerRule)
    {
        Rule = rule;
        RegisterRule = registerRule;
    }

    internal PropertyValidationRule<T, TProperty> Rule { get; }

    private Action<IValidationRule<T>> RegisterRule { get; }

    /// <summary>
    /// Requires the property value to be non-null.
    /// </summary>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> NotNull(
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
    {
        Rule.Add(value => value is not null
            ? null
            : new ValidationFailure(
                Rule.Path,
                message ?? $"{Rule.Path} must not be null",
                errorFactory ?? ((path, text) => Error.Validation.MissingField(text, [(path, text)]))));

        return this;
    }

    /// <summary>
    /// Requires the property value to be non-empty.
    /// </summary>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> NotEmpty(
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
    {
        Rule.Add(value => !IsEmpty(value)
            ? null
            : new ValidationFailure(
                Rule.Path,
                message ?? $"{Rule.Path} must not be empty",
                errorFactory ?? ((path, text) => Error.Validation.MissingField(text, [(path, text)]))));

        return this;
    }

    /// <summary>
    /// Requires the property value to be empty.
    /// </summary>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> Empty(
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
    {
        Rule.Add(value => IsEmpty(value)
            ? null
            : new ValidationFailure(
                Rule.Path,
                message ?? $"{Rule.Path} must be empty",
                errorFactory ?? ((path, text) => Error.Validation.InvalidParameter(text, [(path, text)]))));

        return this;
    }

    /// <summary>
    /// Requires the property value to satisfy the provided predicate.
    /// </summary>
    /// <param name="predicate">The predicate that must evaluate to <see langword="true"/>.</param>
    /// <param name="message">The validation message used when the predicate fails.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> Must(
        Func<TProperty, bool> predicate,
        string message,
        Func<TProperty, string, string, Error>? errorFactory = null)
    {
        Rule.Add(value => predicate(value)
            ? null
            : new ValidationFailure(
                Rule.Path,
                message,
                (path, text) => errorFactory is null
                    ? Error.Validation.InvalidParameter(text, [(path, text)])
                    : errorFactory(value, path, text)));

        return this;
    }

    /// <summary>
    /// Requires the property value to satisfy the provided asynchronous predicate.
    /// </summary>
    /// <param name="predicate">The asynchronous predicate that must evaluate to <see langword="true"/>.</param>
    /// <param name="message">The validation message used when the predicate fails.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> MustAsync(
        Func<TProperty, CancellationToken, Task<bool>> predicate,
        string message,
        Func<TProperty, string, string, Error>? errorFactory = null)
    {
        Rule.AddAsync(async (value, ct) =>
        {
            bool result = await predicate(value, ct).ConfigureAwait(false);
            return result
                ? null
                : new ValidationFailure(
                    Rule.Path,
                    message,
                    (path, text) => errorFactory is null
                        ? Error.Validation.InvalidParameter(text, [(path, text)])
                        : errorFactory(value, path, text));
        });

        return this;
    }

    /// <summary>
    /// Applies a child validator to the selected property and prefixes any reported member paths.
    /// </summary>
    /// <param name="validator">The validator used to validate the nested property value.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> SetValidator(IValidator<TProperty> validator)
    {
        RegisterRule(new NestedValidatorRule<T, TProperty>(Rule.Accessor, Rule.Path, validator));
        return this;
    }

    /// <summary>
    /// Specifies a condition that must be met for the rule to execute.
    /// </summary>
    /// <param name="condition">The condition predicate evaluated against the parent instance.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> When(Func<T, bool> condition)
    {
        Rule.SetCondition(condition);
        return this;
    }

    /// <summary>
    /// Specifies a condition that, when met, skips the rule execution.
    /// </summary>
    /// <param name="condition">The condition predicate evaluated against the parent instance.</param>
    /// <returns>The current builder instance.</returns>
    public PropertyRuleBuilder<T, TProperty> Unless(Func<T, bool> condition)
    {
        Rule.SetCondition(instance => !condition(instance));
        return this;
    }

    private static bool IsEmpty(TProperty value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        if (value is IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();
            using IDisposable? disposable = enumerator as IDisposable;
            return !enumerator.MoveNext();
        }

        return false;
    }
}
