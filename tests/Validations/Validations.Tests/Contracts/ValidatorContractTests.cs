using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Contracts;

public sealed record EmptyCommand(string Name);

public sealed class EmptyValidator : AbstractValidator<EmptyCommand>
{
}

public class ValidatorContractTests
{
    [Test]
    public async Task Validate_WhenNoRulesAreRegistered_ReturnsSuccess()
    {
        Result result = new EmptyValidator().Validate(new EmptyCommand("Alice"));

        await Assert.That(result.IsSuccess).IsTrue();
    }
}
