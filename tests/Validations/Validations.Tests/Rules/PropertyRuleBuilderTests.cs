using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Rules;

public sealed record ProfileCommand(string Name, string? Nickname, string? Department);

public sealed class ProfileValidator : AbstractValidator<ProfileCommand>
{
    public ProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => name.StartsWith("A"), "Name must start with A");

        RuleFor(x => x.Nickname)
            .NotNull();

        RuleFor(x => x.Department)
            .Empty("Department must be empty in V1");
    }
}

public class PropertyRuleBuilderTests
{
    [Test]
    public async Task Validate_PropertyRules_CreateExpectedDetailsAndValidationErrors()
    {
        Result result = new ProfileValidator().Validate(new ProfileCommand("Bob", null, "Platform"));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error).IsTypeOf<AggregateError>();

        AggregateError aggregate = (AggregateError)error;
        await Assert.That(aggregate.Errors.Count).IsEqualTo(3);
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ProfileCommand.Name), "Name must start with A"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ProfileCommand.Nickname), "Nickname must not be null"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ProfileCommand.Department), "Department must be empty in V1"));
    }
}
