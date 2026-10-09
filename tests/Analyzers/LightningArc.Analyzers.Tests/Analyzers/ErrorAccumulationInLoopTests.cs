using System.Linq;
using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ErrorAccumulationInLoopTests
{
    private const string Usings = """
        #nullable enable
        using System.Collections.Generic;
        using LightningArc.Results;
        """;

    [Test]
    public async Task ErrorPlusEquals_InsideForeach_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    foreach (var e in es) { [|errors += e|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideFor_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    for (int i = 0; i < es.Count; i++) { [|errors += es[i]|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideWhile_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    while (es.Count > 0) { [|errors += es[0]|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideDo_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    do { [|errors += es[0]|]; } while (false);
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_OutsideAnyLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(Error e)
                {
                    Error? errors = null;
                    errors += e;
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task IntPlusEquals_InsideLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(int[] xs)
                {
                    int total = 0;
                    foreach (var i in xs) { total += i; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task StringPlusEquals_InsideLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(string[] xs)
                {
                    string s = "";
                    foreach (var x in xs) { s += x; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InMethodCalledFromLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                Error? _errors;

                void M(List<Error> es)
                {
                    foreach (var e in es) { Add(e); }
                }

                void Add(Error e)
                {
                    _errors += e;
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideLocalFunction_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                Error? _errors;

                void M(List<Error> es)
                {
                    foreach (var e in es)
                    {
                        void Local() { _errors += e; }
                        Local();
                    }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Foreach_Accumulation_CodeFix_Should_Collect_And_Aggregate_Once()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    foreach (var e in es) { [|errors += e|]; }
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    var errorsList = new List<Error>();
                    foreach (var e in es) { errorsList.Add(e); }
                    errors = Error.Aggregate(errorsList);
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        await CodeFixVerifier<
            ErrorAccumulationInLoopAnalyzer,
            ErrorAccumulationInLoopCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task For_Accumulation_Without_Collections_Using_CodeFix_Should_Add_Using()
    {
        const string code = """
            #nullable enable
            using LightningArc.Results;

            class C
            {
                void M(System.Collections.Generic.List<Error> es)
                {
                    Error? errors = null;
                    for (int i = 0; i < es.Count; i++) { [|errors += es[i]|]; }
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        const string fixedCode = """
            #nullable enable
            using LightningArc.Results;
            using System.Collections.Generic;

            class C
            {
                void M(System.Collections.Generic.List<Error> es)
                {
                    Error? errors = null;
                    var errorsList = new List<Error>();
                    for (int i = 0; i < es.Count; i++) { errorsList.Add(es[i]); }
                    errors = Error.Aggregate(errorsList);
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        await CodeFixVerifier<
            ErrorAccumulationInLoopAnalyzer,
            ErrorAccumulationInLoopCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task While_Loop_That_May_Run_Zero_Times_CodeFix_Should_Keep_Unconditional_Aggregate()
    {
        // Zero-iteration semantics pin: `Error.Aggregate` returns null for an
        // empty list (see ErrorTests / Error.Aggregate contract), so the
        // unconditional `errors = Error.Aggregate(errorsList);` preserves the
        // accumulator's null value when the loop body never runs. No
        // count-guard is needed, and none must appear.
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    int i = 0;
                    while (i < es.Count) { [|errors += es[i++]|]; }
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    int i = 0;
                    var errorsList = new List<Error>();
                    while (i < es.Count) { errorsList.Add(es[i++]); }
                    errors = Error.Aggregate(errorsList);
                    if (errors is not null) { throw new System.InvalidOperationException(errors.Message); }
                }
            }
            """;

        await CodeFixVerifier<
            ErrorAccumulationInLoopAnalyzer,
            ErrorAccumulationInLoopCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Aggregate_Empty_List_Returns_Null_Preserving_Zero_Iteration_Semantics()
    {
        // Runtime half of the zero-iteration pin above: the unconditional
        // assignment the fix emits is semantics-preserving only because
        // `Error.Aggregate` maps an empty list back to null.
        await Assert.That(Error.Aggregate([])).IsNull();    }

    [Test]
    public async Task CodeFix_With_Field_Accumulator_Offers_No_Fix()
    {
        const string source = """
            #nullable enable
            using System.Collections.Generic;
            using LightningArc.Results;

            class C
            {
                Error? _errors;

                void M(List<Error> es)
                {
                    foreach (var e in es) { _errors += e; }
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Read_Before_Loop_Offers_No_Fix()
    {
        const string source = """
            #nullable enable
            using System.Collections.Generic;
            using LightningArc.Results;

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    System.Console.WriteLine(errors is null);
                    foreach (var e in es) { errors += e; }
                    System.Console.WriteLine(errors is null);
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Read_Inside_Loop_Offers_No_Fix()
    {
        const string source = """
            #nullable enable
            using System.Collections.Generic;
            using LightningArc.Results;

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    foreach (var e in es)
                    {
                        errors += e;
                        System.Console.WriteLine(errors is null);
                    }
                    System.Console.WriteLine(errors is null);
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Second_Loop_Accumulation_Offers_No_Fix()
    {
        // A second loop's `+=` is an outside write the rewrite cannot absorb,
        // so the provider must decline even though each loop alone is fine.
        const string source = """
            #nullable enable
            using System.Collections.Generic;
            using LightningArc.Results;

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    foreach (var e in es) { errors += e; }
                    foreach (var e in es) { errors += e; }
                    System.Console.WriteLine(errors is null);
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    private static async Task<int> CountFixesOfferedAsync(string source)
    {
        // The diagnostic still fires, but the provider must decline when the
        // accumulator is not a loop-local rewrite candidate. Drive the provider
        // directly, since CodeFixVerifier cannot express "no fix offered".
        using AdhocWorkspace workspace = new();
        var project = workspace
            .AddProject("Guards", LanguageNames.CSharp)
            .AddMetadataReference(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(System.Collections.Generic.List<>).Assembly.Location
                )
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(LightningArc.Results.Error).Assembly.Location
                )
            );
        Document document = project.AddDocument("Guard.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var accumulation = root
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .First(a =>
                a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddAssignmentExpression)
            );

        Diagnostic diagnostic = Diagnostic.Create(
            ErrorAccumulationInLoopAnalyzer.Rule,
            Location.Create(tree, accumulation.Span)
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new ErrorAccumulationInLoopCodeFixProvider().RegisterCodeFixesAsync(context);

        return fixesOffered;
    }
}
