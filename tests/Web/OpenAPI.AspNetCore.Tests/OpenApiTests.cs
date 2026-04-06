using LightningArc.OpenAPI.AspNetCore.Filters;
using LightningArc.Primitives.ValueObjects;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.OpenAPI.AspNetCore.Tests;

public class OpenApiTests
{
    [Test]
    public async Task EmailSchemaTransformer_ShouldTransformEmailType()
    {
        // Arrange
        var transformer = new EmailSchemaTransformer();
        var schema = new OpenApiSchema();
        
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        var typeInfo = options.GetTypeInfo(typeof(Email));
        
        var services = new ServiceCollection().BuildServiceProvider();

        var context = new OpenApiSchemaTransformerContext
        {
            JsonTypeInfo = typeInfo,
            DocumentName = "v1",
            ParameterDescription = null,
            JsonPropertyInfo = null,
            ApplicationServices = services
        };

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        await Assert.That(schema.Format).IsEqualTo("email");
        
#if NET10_0
        await Assert.That(schema.Type).IsEqualTo(JsonSchemaType.String);
        await Assert.That(schema.Example?.ToString()).IsEqualTo("usuario@exemplo.com");
#endif
    }
}
