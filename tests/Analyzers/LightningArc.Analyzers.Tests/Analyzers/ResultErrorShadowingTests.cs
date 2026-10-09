using System.Linq;
using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
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
    public async Task Nested_Guards_Report_One_Diagnostic_Per_Unconsumed_Guard()
    {
        // The analyzer reports one diagnostic per unconsumed guard on the
        // same return expression (outer `a`, then inner `b`).
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result a, Result b)
                {
                    if (a.IsFailure)
                    {
                        if (b.IsFailure)
                        {
                            return Error.Validation.InvalidParameter("Shadowed");
                        }
                    }
                    return Result.Success();
                }
            }
            """;

        DiagnosticResult a = AnalyzerVerifier<ResultErrorShadowingAnalyzer>
            .Diagnostic("LARC005")
            .WithSpan(11, 24, 11, 69)
            .WithArguments("a");
        DiagnosticResult b = AnalyzerVerifier<ResultErrorShadowingAnalyzer>
            .Diagnostic("LARC005")
            .WithSpan(11, 24, 11, 69)
            .WithArguments("b");

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code, a, b);
    }

    [Test]
    public async Task Shadowing_CodeFix_With_Nested_Guards_Should_Combine_Left_Associatively()
    {
        // Pin: with several active failure guards the fix chains every
        // unconsumed guard left-associatively, innermost first
        // (`b.Error + a.Error + <newError>`), rather than restricting the
        // fix to a single guard. Driven directly: the analyzer reports two
        // same-span diagnostics here, which the code-fix harness cannot
        // express.
        const string source = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result a, Result b)
                {
                    if (a.IsFailure)
                    {
                        if (b.IsFailure)
                        {
                            return Error.Validation.InvalidParameter("Shadowed");
                        }
                    }
                    return Result.Success();
                }
            }
            """;

        const string expectedFixed = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result a, Result b)
                {
                    if (a.IsFailure)
                    {
                        if (b.IsFailure)
                        {
                            return b.Error + a.Error + Error.Validation.InvalidParameter("Shadowed");
                        }
                    }
                    return Result.Success();
                }
            }
            """;

        await Assert.That(await ApplyFixAsync(source)).IsEqualTo(expectedFixed);
    }

    private static async Task<string> ApplyFixAsync(string source)
    {
        using AdhocWorkspace workspace = new();
        var project = workspace
            .AddProject("Pins", LanguageNames.CSharp)
            .AddMetadataReference(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(typeof(Result).Assembly.Location)
            );
        Document document = project.AddDocument("Pin.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var ret = root.DescendantNodes().OfType<ReturnStatementSyntax>().First(r =>
            r.Expression?.ToString().Contains("Shadowed") == true
        );

        Diagnostic diagnostic = Diagnostic.Create(
            ResultErrorShadowingAnalyzer.Rule,
            Location.Create(tree, ret.Expression!.Span)
        );

        CodeAction? action = null;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (a, diagnostics) => action = a,
            CancellationToken.None
        );

        await new ResultErrorShadowingCodeFixProvider().RegisterCodeFixesAsync(context);
        await Assert.That(action).IsNotNull();

        var operations = await action!.GetOperationsAsync(CancellationToken.None);
        var applyChanges = operations.OfType<ApplyChangesOperation>().Single();
        Document fixedDocument = applyChanges.ChangedSolution.GetDocument(document.Id)!;
        var text = await fixedDocument.GetTextAsync(CancellationToken.None);
        return text.ToString();
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
