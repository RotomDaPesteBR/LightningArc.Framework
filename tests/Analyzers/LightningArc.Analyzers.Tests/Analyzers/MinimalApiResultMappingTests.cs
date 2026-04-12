using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class MinimalApiResultMappingTests
{
    private const string Usings = """
        using LightningArc.Results;
        using LightningArc.Results.AspNetCore;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;
        """;

    [Test]
    public async Task MapGet_Returning_Raw_Result_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapGet("/test", [|() => Result.Success()|]);
                }
            }
            """;

        await AnalyzerVerifier<MinimalApiResultMappingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task MapPost_Returning_Result_With_ToEndpointResult_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapPost("/test", () => Result.Success().ToEndpointResult());
                }
            }
            """;

        await AnalyzerVerifier<MinimalApiResultMappingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Controller_Returning_Raw_Result_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}
            using Microsoft.AspNetCore.Mvc;

            class MyController : ControllerBase
            {
                [HttpGet]
                public Result Get() => Result.Success();
            }
            """;

        // MVC is handled by implicit conversion, so LARC031 should not trigger here
        await AnalyzerVerifier<MinimalApiResultMappingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task MapGet_Returning_Other_Type_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapGet("/test", () => "hello");
                }
            }
            """;

        await AnalyzerVerifier<MinimalApiResultMappingAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
