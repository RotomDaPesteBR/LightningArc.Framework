using LightningArc.Results;
using LightningArc.Validations;

namespace LightningArc.Validations.Tests.Rules;

public sealed record ComparisonRuleCommand(int Age, int Score, DateTime StartDate);

public sealed class ComparisonRuleValidator : AbstractValidator<ComparisonRuleCommand>
{
    public ComparisonRuleValidator()
    {
        RuleFor(x => x.Age)
            .GreaterThan(17)
            .LessThan(66);

        RuleFor(x => x.Score)
            .Equal(100, "Score must be exactly 100")
            .NotEqual(0, "Score must not be zero");

        RuleFor(x => x.StartDate)
            .GreaterThan(new DateTime(2025, 1, 1))
            .LessThan(new DateTime(2026, 1, 1));
    }
}

public class ComparisonRuleTests
{
    [Test]
    public async Task Validate_ComparisonRules_ReportExpectedMessages()
    {
        Result result = new ComparisonRuleValidator().Validate(new ComparisonRuleCommand(16, 0, new DateTime(2024, 12, 31)));

        await Assert.That(result.TryGetError(out var error)).IsTrue();
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ComparisonRuleCommand.Age), "Age must be greater than 17"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ComparisonRuleCommand.Score), "Score must be exactly 100"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ComparisonRuleCommand.Score), "Score must not be zero"));
        await Assert.That(error.Details).Contains(new ErrorDetail(nameof(ComparisonRuleCommand.StartDate), "StartDate must be greater than 2025-01-01T00:00:00.0000000"));
    }
}
