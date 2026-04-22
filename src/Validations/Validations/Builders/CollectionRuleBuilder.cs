using LightningArc.Validations.Rules;

namespace LightningArc.Validations.Builders;

/// <summary>
/// Builds validation rules for each element in a collection property.
/// </summary>
/// <typeparam name="T">The type that owns the collection being validated.</typeparam>
/// <typeparam name="TElement">The type of each collection element.</typeparam>
public sealed class CollectionRuleBuilder<T, TElement>
    where TElement : class
{
    internal CollectionRuleBuilder(
        Action<IValidationRule<T>> registerRule,
        Func<T, IEnumerable<TElement>?> accessor,
        string path)
    {
        RegisterRule = registerRule;
        Accessor = accessor;
        Path = path;
    }

    private Action<IValidationRule<T>> RegisterRule { get; }

    private Func<T, IEnumerable<TElement>?> Accessor { get; }

    private string Path { get; }

    /// <summary>
    /// Applies a validator to each element in the selected collection.
    /// </summary>
    /// <param name="validator">The validator used to validate each collection element.</param>
    /// <returns>The current builder instance.</returns>
    public CollectionRuleBuilder<T, TElement> SetValidator(IValidator<TElement> validator)
    {
        RegisterRule(new CollectionValidationRule<T, TElement>(Accessor, Path, validator));
        return this;
    }
}
