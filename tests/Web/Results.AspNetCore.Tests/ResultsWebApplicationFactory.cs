using TUnit.AspNetCore;

namespace LightningArc.Results.AspNetCore.Tests;

public class ResultsWebApplicationFactory : TestWebApplicationFactory<LightningArc.Results.AspNetCore.Tests.Server.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
