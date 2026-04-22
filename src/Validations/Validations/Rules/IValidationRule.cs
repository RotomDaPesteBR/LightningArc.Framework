using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal interface IValidationRule<in T>
{
    IEnumerable<ValidationFailure> Validate(T instance);
}
