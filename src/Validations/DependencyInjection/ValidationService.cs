using LightningArc.Results;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IValidationService"/>.
/// </summary>
public sealed class ValidationService(IServiceProvider serviceProvider) : IValidationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public Result Validate<T>(T instance)
    {
        IValidator<T>? validator = _serviceProvider.GetService<IValidator<T>>();
        return validator is null ? Result.Success() : validator.Validate(instance);
    }
}
