using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Rules;

public sealed record StringRuleCommand(string Name);
public sealed record NullableStringRuleCommand(string? Nickname);

public sealed class StringRuleValidator : AbstractValidator<StringRuleCommand>
{
    public StringRuleValidator()
    {
        RuleFor(x => x.Name)
            .MinLength(3)
            .MaxLength(10)
            .Length(3, 10);
    }
}

public sealed class NullableStringRuleValidator : AbstractValidator<NullableStringRuleCommand>
{
    public NullableStringRuleValidator()
    {
        RuleFor(x => x.Nickname)
            .MinLength(2)
            .MaxLength(10)
            .Length(2, 10);
    }
}

public class StringRuleTests
{
    [Test]
    public async Task Validate_StringLengthRules_ReportValueOutOfRangeErrors()
    {
        Result result = new StringRuleValidator().Validate(new StringRuleCommand("AB"));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(StringRuleCommand.Name), "Name must have at least 3 characters"));
    }

    [Test]
    public async Task Validate_NullableStringLengthRules_ReportValueOutOfRangeErrors()
    {
        Result result = new NullableStringRuleValidator().Validate(new NullableStringRuleCommand(null));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(NullableStringRuleCommand.Nickname), "Nickname must have at least 2 characters"));
    }
}
