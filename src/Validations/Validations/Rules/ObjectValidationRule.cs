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
}
