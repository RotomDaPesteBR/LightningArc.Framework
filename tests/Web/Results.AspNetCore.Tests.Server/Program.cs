using System.Net;
using System.Text.Json;
using LightningArc.CORS.AspNetCore;
using LightningArc.Json.Converters;
using LightningArc.Results;
using LightningArc.Results.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.AddJsonConverters();
    });

builder.Services.AddCorsPolicies();
builder.Services.AddEndpointResults(
    wrapSuccessResponses: true,
    configureMappings: (successes, errors) =>
    {
        errors.Map<Business.OrderRejectedError>(
            HttpStatusCode.UnprocessableEntity,
            "Pedido Rejeitado",
            "urn:api-errors:order-rejected"
        );
    }
);

WebApplication app = builder.Build();

app.UseRouting();
app.UseCorsPolicies();
app.UseAuthorization();
app.MapControllers();

app.Run();

namespace LightningArc.Results.AspNetCore.Tests.Server
{
    public partial class Program { }
}
