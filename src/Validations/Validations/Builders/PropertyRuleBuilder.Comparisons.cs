using System.Globalization;
using LightningArc.Results;
using LightningArc.Validations.Builders;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations;

/// <summary>
/// Provides comparison-based validation rules for comparable property builders.
/// </summary>
public static class ComparablePropertyRuleBuilderExtensions
{
    /// <summary>
    /// Requires the property value to be equal to the expected value.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, TProperty> Equal<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> builder,
        TProperty expected,
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
    {
        builder.Rule.Add(value => EqualityComparer<TProperty>.Default.Equals(value, expected)
            ? null
            : new ValidationFailure(
                builder.Rule.Path,
                message ?? $"{builder.Rule.Path} must be equal to {expected}",
                errorFactory ?? ((path, text) => Error.Validation.InvalidParameter(text, [(path, text)]))));

        return builder;
    }

    /// <summary>
    /// Requires the property value to be different from the unexpected value.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="unexpected">The unexpected value.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, TProperty> NotEqual<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> builder,
        TProperty unexpected,
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
    {
        builder.Rule.Add(value => !EqualityComparer<TProperty>.Default.Equals(value, unexpected)
            ? null
            : new ValidationFailure(
                builder.Rule.Path,
                message ?? $"{builder.Rule.Path} must not be equal to {unexpected}",
                errorFactory ?? ((path, text) => Error.Validation.InvalidParameter(text, [(path, text)]))));

        return builder;
    }

    /// <summary>
    /// Requires the property value to be greater than the specified threshold.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="threshold">The exclusive lower bound.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> builder,
        TProperty threshold,
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
        where TProperty : IComparable<TProperty>
    {
        builder.Rule.Add(value => IsGreaterThan(value, threshold)
            ? null
            : new ValidationFailure(
                builder.Rule.Path,
                message ?? $"{builder.Rule.Path} must be greater than {FormatThreshold(threshold)}",
                errorFactory ?? ((path, text) => Error.Validation.ValueOutOfRange(text, [(path, text)]))));

        return builder;
    }

    /// <summary>
    /// Requires the property value to be less than the specified threshold.
    /// </summary>
    /// <param name="builder">The builder that configures the property rule.</param>
    /// <param name="threshold">The exclusive upper bound.</param>
    /// <param name="message">The optional validation message.</param>
    /// <param name="errorFactory">The optional error factory used when validation fails.</param>
    /// <returns>The current builder instance.</returns>
    public static PropertyRuleBuilder<T, TProperty> LessThan<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> builder,
        TProperty threshold,
        string? message = null,
        Func<string, string, Error>? errorFactory = null)
        where TProperty : IComparable<TProperty>
    {
        builder.Rule.Add(value => IsLessThan(value, threshold)
            ? null
            : new ValidationFailure(
                builder.Rule.Path,
                message ?? $"{builder.Rule.Path} must be less than {FormatThreshold(threshold)}",
                errorFactory ?? ((path, text) => Error.Validation.ValueOutOfRange(text, [(path, text)]))));

        return builder;
    }

    private static bool IsGreaterThan<TProperty>(TProperty value, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        if (value is null)
        {
            return false;
        }

        if (threshold is null)
        {
            return true;
        }

        return value.CompareTo(threshold) > 0;
    }

    private static bool IsLessThan<TProperty>(TProperty value, TProperty threshold)
        where TProperty : IComparable<TProperty>
    {
        if (value is null || threshold is null)
        {
            return false;
        }

        return value.CompareTo(threshold) < 0;
    }

    private static string FormatThreshold<TProperty>(TProperty value) => value switch
    {
        null => string.Empty,
        DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };
}
