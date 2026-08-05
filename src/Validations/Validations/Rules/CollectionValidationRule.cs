using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Results;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal sealed class CollectionValidationRule<T, TElement>(
    Func<T, IEnumerable<TElement>?> accessor,
    string path,
    IValidator<TElement> validator) : IValidationRule<T>
    where TElement : class
{
    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        var items = accessor(instance);
        if (items is null)
        {
            yield break;
        }

        int index = 0;
        foreach (TElement item in items)
        {
            if (item is null)
            {
                index++;
                continue;
            }

            string itemPath = $"{path}[{index}]";

            if (validator is AbstractValidator<TElement> nestedValidator)
            {
                foreach (ValidationFailure failure in nestedValidator.ValidateFailures(item, stopOnFirstFailure: true))
                {
                    yield return failure.Prefix(itemPath);
                }
            }
            else
            {
                Result result = validator.Validate(item);
                if (result.TryGetError(out Error error))
                {
                    yield return new ValidationFailure(
                        itemPath,
                        error.Message,
                        (_, _) => ErrorPrefixing.Prefix(error, itemPath));
                }
            }

            index++;
        }
    }

    public async IAsyncEnumerable<ValidationFailure> ValidateAsync(
        T instance,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var items = accessor(instance);
        if (items is null)
        {
            yield break;
        }

        int index = 0;

        foreach (TElement item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item is null)
            {
                index++;
                continue;
            }

            string itemPath = $"{path}[{index}]";

            if (validator is AbstractValidator<TElement> nestedValidator)
            {
                foreach (ValidationFailure failure in nestedValidator.ValidateFailures(item, stopOnFirstFailure: true))
                {
                    yield return failure.Prefix(itemPath);
                }
            }
            else
            {
                Result result = await validator.ValidateAsync(item, cancellationToken).ConfigureAwait(false);
                if (result.TryGetError(out Error error))
                {
                    yield return new ValidationFailure(
                        itemPath,
                        error.Message,
                        (_, _) => ErrorPrefixing.Prefix(error, itemPath));
                }
            }

            index++;
        }
    }
}
