namespace LightningArc.Metalama.Results.Tests;

public class MetalamaResultsSetupTests
{
    [Test]
    public async Task Setup_ShouldBeValid()
    {
        // Simple placeholder to ensure the project is recognized as a test project
        bool isValid = true;
        await Assert.That(isValid).IsTrue();
    }
}
