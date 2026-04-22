using LightningArc.Results;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection.Tests.Registration;

public sealed record CreateUserCommand(string Name);

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
}

public sealed record UpdateUserCommand(string Name);

public sealed class MultiContractValidator : IValidator<CreateUserCommand>, IValidator<UpdateUserCommand>
{
    public Result Validate(CreateUserCommand instance)
    {
        return Result.Success();
    }

    public Result Validate(UpdateUserCommand instance)
    {
        return Result.Success();
    }
}

public class ManualRegistrationTests
{
    [Test]
    public async Task AddValidator_ConcreteOverload_RegistersValidatorAgainstItsInterface()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<CreateUserValidator>()
            .BuildServiceProvider();

        IValidator<CreateUserCommand>? validator = provider.GetService<IValidator<CreateUserCommand>>();

        await Assert.That(validator).IsTypeOf<CreateUserValidator>();
    }

    [Test]
    public async Task AddValidator_ConcreteOverload_RegistersValidationServiceFacade()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<CreateUserValidator>()
            .BuildServiceProvider();

        IValidationService? validationService = provider.GetService<IValidationService>();

        await Assert.That(validationService).IsNotNull();
    }

    [Test]
    public async Task AddValidator_ConcreteOverload_WhenValidatorImplementsMultipleClosedContracts_ThrowsInvalidOperationException()
    {
        InvalidOperationException? exception = await Assert.That(() => new ServiceCollection()
            .AddValidator<MultiContractValidator>()).Throws<InvalidOperationException>();

        await Assert.That(exception?.Message).Contains("Use AddValidator<TModel, TValidator>() for explicit registration.");
    }

    [Test]
    public async Task AddValidator_GenericOverload_RegistersClosedValidatorInterface()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<CreateUserCommand, CreateUserValidator>()
            .BuildServiceProvider();

        IValidator<CreateUserCommand>? validator = provider.GetService<IValidator<CreateUserCommand>>();

        await Assert.That(validator).IsTypeOf<CreateUserValidator>();
    }

    [Test]
    public async Task AddValidator_GenericOverload_RegistersValidationServiceFacade()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<CreateUserCommand, CreateUserValidator>()
            .BuildServiceProvider();

        IValidationService? validationService = provider.GetService<IValidationService>();

        await Assert.That(validationService).IsNotNull();
    }
}
