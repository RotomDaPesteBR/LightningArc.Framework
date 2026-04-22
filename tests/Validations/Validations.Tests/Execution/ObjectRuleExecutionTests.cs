using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Execution;

public sealed record RegistrationCommand(string Name, int Age);

public sealed class RegistrationValidator : AbstractValidator<RegistrationCommand>
{
    public RegistrationValidator(bool failFast = false)
    {
        if (failFast)
        {
            UseFailFast();
        }

        Rule(
            command => !string.IsNullOrWhiteSpace(command.Name),
            path: nameof(RegistrationCommand.Name),
            message: "Name is required",
            errorFactory: (path, message) => Error.Validation.MissingField(message, [(path, message)])
        );

        Rule(
            command => command.Age >= 18,
            path: nameof(RegistrationCommand.Age),
            message: "Age must be at least 18",
            errorFactory: (path, message) => Error.Validation.ValueOutOfRange(message, [(path, message)])
        );
    }
}

public class ObjectRuleExecutionTests
{
    [Test]
    public async Task Validate_ByDefault_CollectsAllObjectRuleFailures()
    {
        Result result = new RegistrationValidator().Validate(new RegistrationCommand("", 12));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<AggregateError>();

        AggregateError aggregate = (AggregateError)error;
        await Assert.That(aggregate.Errors.Count).IsEqualTo(2);
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(RegistrationCommand.Name), "Name is required"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(RegistrationCommand.Age), "Age must be at least 18"));
    }

    [Test]
    public async Task Validate_InFailFastMode_StopsAfterTheFirstFailure()
    {
        Result result = new RegistrationValidator(failFast: true).Validate(new RegistrationCommand("", 12));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsNotTypeOf<AggregateError>();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(RegistrationCommand.Name), "Name is required"));
        await Assert.That(error.Details.Count).IsEqualTo(1);
    }
}
