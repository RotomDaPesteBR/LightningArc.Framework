using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal sealed class PropertyValidationRule<T, TProperty>(
    Func<T, TProperty> accessor,
    string path) : IValidationRule<T>
{
    private readonly List<Func<TProperty, ValidationFailure?>> _validators = [];

    public Func<T, TProperty> Accessor { get; } = accessor;
    public string Path { get; } = path;

    public void Add(Func<TProperty, ValidationFailure?> validator) => _validators.Add(validator);

    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        TProperty value = Accessor(instance);

        foreach (var validator in _validators)
        {
            ValidationFailure? failure = validator(value);
            if (failure is not null)
            {
                yield return failure;
            }
        }
    }
}
