using System.Linq;
using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ResultErrorShadowingTests
{
    private const string Usings = """
        using LightningArc.Results;
        """;

    [Test]
    public async Task Shadowing_In_IfFailure_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return [|Error.Validation.InvalidParameter("Shadowed")|];
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Propagating_Original_Error_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return result.Error;
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Error_Then_Returning_New_One_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        var msg = result.Error.Message;
                        return Error.Validation.InvalidParameter("New Error");
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Via_MapError_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    return result.MapError(e => Error.Validation.InvalidParameter("Mapped"));
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Via_TryGetError_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.TryGetError(out var err))
                    {
                        return Error.Validation.InvalidParameter("From TryGet");
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Passing_To_Method_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        Log(result.Error);
                        return Error.Validation.InvalidParameter("Logged");
                    }
                    return Result.Success();
                }

                void Log(Error e) {}
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Shadowing_CodeFix_Should_Combine_Guard_Error_With_Plus()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return [|Error.Validation.InvalidParameter("Shadowed")|];
                    }
                    return Result.Success();
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return result.Error + Error.Validation.InvalidParameter("Shadowed");
                    }
                    return Result.Success();
                }
            }
            """;

        await CodeFixVerifier<
            ResultErrorShadowingAnalyzer,
            ResultErrorShadowingCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Shadowing_CodeFix_With_Negated_Guard_Should_Combine()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (!result.IsSuccess)
                    {
                        return [|Error.Validation.MissingField("id")|];
                    }
                    return Result.Success();
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (!result.IsSuccess)
                    {
                        return result.Error + Error.Validation.MissingField("id");
                    }
                    return Result.Success();
                }
            }
            """;

        await CodeFixVerifier<
            ResultErrorShadowingAnalyzer,
            ResultErrorShadowingCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task CodeFix_With_Result_Failure_Return_Offers_No_Fix()
    {
        // `Error + Result` has no operator overload, so a `Result.Failure(...)`
        // return still warns but gets no fix. Drive the provider directly with
        // real references so the withholding is meaningful, not vacuous.
        const string source = """
            using LightningArc.Results;

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return Result.Failure(Error.Validation.InvalidParameter("Shadowed"));
                    }
                    return Result.Success();
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    private static async Task<int> CountFixesOfferedAsync(string source)
    {
        using AdhocWorkspace workspace = new();
        var project = workspace
            .AddProject("Guards", LanguageNames.CSharp)
            .AddMetadataReference(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(LightningArc.Results.Result).Assembly.Location
                )
            );
        Document document = project.AddDocument("Guard.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var failureCall = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(i => i.Expression.ToString().Contains("Failure"));

        Diagnostic diagnostic = Diagnostic.Create(
            ResultErrorShadowingAnalyzer.Rule,
            Location.Create(tree, failureCall.Span)
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new ResultErrorShadowingCodeFixProvider().RegisterCodeFixesAsync(context);

        return fixesOffered;
    }
}
