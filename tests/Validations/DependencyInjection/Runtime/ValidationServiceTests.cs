using LightningArc.Results;
using LightningArc.Validations;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection.Tests.Runtime;

public sealed record PingCommand(string Value);

public sealed class PingValidator : AbstractValidator<PingCommand>
{
    public PingValidator()
    {
        RuleFor(command => command.Value)
            .NotEmpty("Value is required");
    }
}

public sealed record ScopedPingCommand(string Value);

public sealed class ScopedPingValidator(ScopedDependency dependency) : IValidator<ScopedPingCommand>
{
    public ScopedDependency Dependency { get; } = dependency;

    public Result Validate(ScopedPingCommand instance)
    {
        return Result.Success();
    }
}

public sealed class ScopedDependency
{
}

public class ValidationServiceTests
{
    [Test]
    public async Task Validate_WhenNoValidatorIsRegisteredViaAddValidators_ReturnsSuccess()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidators()
            .BuildServiceProvider();

        IValidationService service = provider.GetRequiredService<IValidationService>();
        Result result = service.Validate(new PingCommand("ok"));

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Validate_WhenValidatorIsRegisteredViaAssemblyScanning_ExecutesTheValidator()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidators(options => options.ScanAssemblyContaining<PingValidator>())
            .BuildServiceProvider();

        IValidationService service = provider.GetRequiredService<IValidationService>();
        Result result = service.Validate(new PingCommand(""));

        await Assert.That(result.TryGetError(out Error? error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(PingCommand.Value), "Value is required"));
    }

    [Test]
    public async Task Validate_WhenValidatorIsRegisteredViaConcreteAddValidator_ResolvesTheFacadeAndExecutesTheValidator()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<PingValidator>()
            .BuildServiceProvider();

        IValidationService service = provider.GetRequiredService<IValidationService>();
        Result result = service.Validate(new PingCommand(""));

        await Assert.That(result.TryGetError(out Error? error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(PingCommand.Value), "Value is required"));
    }

    [Test]
    public async Task Validate_WhenValidatorIsRegisteredViaTypedAddValidator_ResolvesTheFacadeAndExecutesTheValidator()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidator<PingCommand, PingValidator>()
            .BuildServiceProvider();

        IValidationService service = provider.GetRequiredService<IValidationService>();
        Result result = service.Validate(new PingCommand(""));

        await Assert.That(result.TryGetError(out Error? error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(PingCommand.Value), "Value is required"));
    }

    [Test]
    public async Task Validate_WhenFacadeIsResolvedFromScope_CanUseScopedValidatorDependencies()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddScoped<ScopedDependency>()
            .AddValidator<ScopedPingCommand, ScopedPingValidator>(ServiceLifetime.Scoped)
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });

        using IServiceScope scope = provider.CreateScope();
        IValidationService service = scope.ServiceProvider.GetRequiredService<IValidationService>();
        ScopedDependency scopedDependency = scope.ServiceProvider.GetRequiredService<ScopedDependency>();
        ScopedPingValidator validator = scope.ServiceProvider.GetRequiredService<IValidator<ScopedPingCommand>>() as ScopedPingValidator
            ?? throw new InvalidOperationException("Expected scoped validator instance.");

        Result result = service.Validate(new ScopedPingCommand("ok"));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(validator.Dependency).IsSameReferenceAs(scopedDependency);
    }
}
