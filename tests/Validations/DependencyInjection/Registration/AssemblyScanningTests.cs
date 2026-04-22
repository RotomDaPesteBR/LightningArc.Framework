using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Validations.DependencyInjection.Tests.Registration;

public sealed record ScanCommand(string Value);

public sealed class ScanValidator : AbstractValidator<ScanCommand>
{
}

public class AssemblyScanningTests
{
    [Test]
    public async Task AddValidators_ScansAssemblyContainingAndRegistersClosedValidators()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddValidators(options => options.ScanAssemblyContaining<ScanValidator>())
            .BuildServiceProvider();

        IValidator<ScanCommand>? validator = provider.GetService<IValidator<ScanCommand>>();

        await Assert.That(validator).IsTypeOf<ScanValidator>();
    }
}
