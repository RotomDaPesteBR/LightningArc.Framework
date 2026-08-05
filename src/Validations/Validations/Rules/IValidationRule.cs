using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LightningArc.Validations.Internal;

namespace LightningArc.Validations.Rules;

internal interface IValidationRule<in T>
{
    IEnumerable<ValidationFailure> Validate(T instance);

    IAsyncEnumerable<ValidationFailure> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
