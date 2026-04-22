#nullable disable

using LightningArc.Results;
using LightningArc.Validations.Builders;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations;

/// <summary>
/// Provides string-specific validation rules for property builders.
/// </summary>
public static class StringPropertyRuleBuilderExtensions
{
    /// <summary>
    /// Requires the string property to have at least the specified number of characters.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="min">The minimum allowed length.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, string> MinLength<T>(
        this PropertyRuleBuilder<T, string> builder,
        int min,
        string message = null,
        Func<string, string, Error> errorFactory = null) =>
        AddLengthRule(
            builder,
            text => text.Length >= min,
            message ?? $"{builder.Rule.Path} must have at least {min} characters",
            errorFactory);

    /// <summary>
    /// Requires the string property to have at most the specified number of characters.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, string> MaxLength<T>(
        this PropertyRuleBuilder<T, string> builder,
        int max,
        string message = null,
        Func<string, string, Error> errorFactory = null) =>
        AddLengthRule(
            builder,
            text => text.Length <= max,
            message ?? $"{builder.Rule.Path} must have at most {max} characters",
            errorFactory);

    /// <summary>
    /// Requires the string property to have a length within the specified inclusive range.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="min">The minimum allowed length.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, string> Length<T>(
        this PropertyRuleBuilder<T, string> builder,
        int min,
        int max,
        string message = null,
        Func<string, string, Error> errorFactory = null) =>
        AddLengthRule(
            builder,
            text => text.Length >= min && text.Length <= max,
            message ?? $"{builder.Rule.Path} must have between {min} and {max} characters",
            errorFactory);

    private static PropertyRuleBuilder<T, string> AddLengthRule<T>(
        PropertyRuleBuilder<T, string> builder,
        Func<string, bool> predicate,
        string message,
        Func<string, string, Error> errorFactory)
    {
        builder.Rule.Add(value => value is string text && predicate(text)
            ? null
            : new ValidationFailure(
                builder.Rule.Path,
                message,
                errorFactory ?? ((path, text) => Error.Validation.ValueOutOfRange(text, [(path, text)]))));

        return builder;
    }
}

#nullable restore
