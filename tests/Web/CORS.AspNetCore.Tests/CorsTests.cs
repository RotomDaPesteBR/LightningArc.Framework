using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace LightningArc.CORS.AspNetCore.Tests;

public class CorsTests
{
    [Test]
    public async Task AddCorsPolicies_ShouldRegisterCorsOptions()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        services.AddCorsPolicies();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<CorsOptions>>();

        // Assert
        await Assert.That(options).IsNotNull();
        await Assert.That(options!.Value.GetPolicy("AllowAll")).IsNotNull();
    }
}
