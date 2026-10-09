using System.Linq;
using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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

        // MVC is handled by implicit conversion, so LARC060 should not trigger here
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

    [Test]
    public async Task MapGet_Raw_Result_CodeFix_Should_Append_ToEndpointResult()
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

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapGet("/test", () => Result.Success().ToEndpointResult());
                }
            }
            """;

        await CodeFixVerifier<
            MinimalApiResultMappingAnalyzer,
            MinimalApiResultMappingCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task MapPost_Block_Lambda_CodeFix_Should_Append_To_Each_Return()
    {
        // Block-bodied lambdas never carry an inferable Result delegate return
        // type, so the analyzer cannot flag them — pin the provider's per-return
        // rewrite by driving it directly with a diagnostic on the lambda.
        const string source = """
            using LightningArc.Results;
            using LightningArc.Results.AspNetCore;
            using Microsoft.AspNetCore.Routing;

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapGet("/test", (int id) =>
                    {
                        if (id > 0)
                        {
                            return Result.Success();
                        }
                        return Result.Failure(Error.Validation.InvalidParameter("id"));
                    });
                }
            }
            """;

        const string expectedFixed = """
            using LightningArc.Results;
            using LightningArc.Results.AspNetCore;
            using Microsoft.AspNetCore.Routing;

            class Program
            {
                void Main(IEndpointRouteBuilder app)
                {
                    app.MapGet("/test", (int id) =>
                    {
                        if (id > 0)
                        {
                            return Result.Success().ToEndpointResult();
                        }
                        return Result.Failure(Error.Validation.InvalidParameter("id")).ToEndpointResult();
                    });
                }
            }
            """;

        await Assert.That(await ApplyFixAsync(source)).IsEqualTo(expectedFixed);
    }

    private static async Task<string> ApplyFixAsync(string source)
    {
        using AdhocWorkspace workspace = new();
        Document document = workspace
            .AddProject("Fixes", LanguageNames.CSharp)
            .AddDocument("Fix.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var lambda = root.DescendantNodes().OfType<LambdaExpressionSyntax>().First();

        Diagnostic diagnostic = Diagnostic.Create(
            MinimalApiResultMappingAnalyzer.Rule,
            Location.Create(tree, lambda.Span)
        );

        CodeAction? action = null;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (a, diagnostics) => action = a,
            CancellationToken.None
        );

        await new MinimalApiResultMappingCodeFixProvider().RegisterCodeFixesAsync(context);
        await Assert.That(action).IsNotNull();

        var operations = await action!.GetOperationsAsync(CancellationToken.None);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        Document fixedDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        var text = await fixedDocument.GetTextAsync(CancellationToken.None);
        return text.ToString();
    }
}
