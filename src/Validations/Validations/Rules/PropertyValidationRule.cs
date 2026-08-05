using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal sealed class PropertyValidationRule<T, TProperty>(
    Func<T, TProperty> accessor,
    string path) : IValidationRule<T>
{
    private readonly List<Func<TProperty, ValidationFailure?>> _validators = [];
    private readonly List<Func<TProperty, CancellationToken, Task<ValidationFailure?>>> _asyncValidators = [];
    private Func<T, bool>? _condition;

    public Func<T, TProperty> Accessor { get; } = accessor;
    public string Path { get; } = path;

    public void Add(Func<TProperty, ValidationFailure?> validator) => _validators.Add(validator);

    public void AddAsync(Func<TProperty, CancellationToken, Task<ValidationFailure?>> validator) => _asyncValidators.Add(validator);

    public void SetCondition(Func<T, bool> condition) => _condition = condition;

    public bool EvaluateCondition(T instance) => _condition is null || _condition(instance);

    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        if (!EvaluateCondition(instance))
        {
            yield break;
        }

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

    public async IAsyncEnumerable<ValidationFailure> ValidateAsync(
        T instance,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!EvaluateCondition(instance))
        {
            yield break;
        }

        TProperty value = Accessor(instance);

        foreach (var validator in _asyncValidators)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidationFailure? failure = await validator(value, cancellationToken).ConfigureAwait(false);
            if (failure is not null)
            {
                yield return failure;
            }
        }

        foreach (var validator in _validators)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidationFailure? failure = validator(value);
            if (failure is not null)
            {
                yield return failure;
            }
        }
    }
}
