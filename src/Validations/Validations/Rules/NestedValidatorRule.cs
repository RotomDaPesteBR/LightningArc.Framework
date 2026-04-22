using LightningArc.Results;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal sealed class NestedValidatorRule<T, TProperty>(
    Func<T, TProperty> accessor,
    string path,
    IValidator<TProperty> validator) : IValidationRule<T>
{
    public IEnumerable<ValidationFailure> Validate(T instance)
    {
        TProperty value = accessor(instance);
        if (value is null)
        {
            yield break;
        }

        if (validator is AbstractValidator<TProperty> nestedValidator)
        {
            foreach (ValidationFailure failure in nestedValidator.ValidateFailures(value, stopOnFirstFailure: true))
            {
                yield return failure.Prefix(path);
            }

            yield break;
        }

        Result result = validator.Validate(value);
        if (result.TryGetError(out Error error))
        {
            yield return new ValidationFailure(
                path,
                error.Message,
                (_, _) => ErrorPrefixing.Prefix(error, path));
        }
    }
}
