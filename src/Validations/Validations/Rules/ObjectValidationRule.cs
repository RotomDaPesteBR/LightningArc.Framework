using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Results;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal sealed class ObjectValidationRule<T>(
    Func<T, bool> predicate,
    string? path,
    string message,
    Func<string, string, Error> errorFactory) : IValidationRule<T>
{
    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        if (predicate(instance))
        {
            return [];
        }

        return [new ValidationFailure(path ?? string.Empty, message, errorFactory)];
    }

    public async IAsyncEnumerable<ValidationFailure> ValidateAsync(
        T instance,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<ValidationFailure> failures = Validate(instance);
        foreach (var failure in failures)
        {
            yield return failure;
        }
    }
}
