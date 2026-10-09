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

public class EmptyResultAggregatorChainTests
{
    private const string Usings = """
        using System;
        using System.Threading.Tasks;
        using LightningArc.Results;
        """;

    [Test]
    public async Task Aggregate_ThenBuild_WithNoChecks_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = [|Result.Aggregate().Build()|];
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    // NOTE: there is intentionally no "empty chain to BuildAsync()" positive test.
    // BuildAsync() extends Task<ResultAggregator>, and no API path produces a
    // Task<ResultAggregator> without passing through a named check step (Check /
    // CheckAsync / CheckAll / WhenAsync / ...), so an empty BuildAsync chain is not
    // expressible against the real assemblies. The async-terminator path is instead
    // pinned by the CheckAsync / CheckAll / WhenAsync / WhenAll negatives below,
    // which all terminate in BuildAsync().

    [Test]
    public async Task Aggregate_WithSingleCheck_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = Result.Aggregate().Check(() => Result.Success()).Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithCheckEach_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = Result.Aggregate().CheckEach(new Func<Result>[] { () => Result.Success() }).Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithCheckAsync_ThenBuildAsync_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                async Task M()
                {
                    var r = await Result.Aggregate().CheckAsync(() => Task.FromResult(Result.Success())).BuildAsync();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithCheckAll_ThenBuildAsync_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                async Task M()
                {
                    var r = await Result.Aggregate().CheckAll(new Func<Task<Result>>[] { () => Task.FromResult(Result.Success()) }).BuildAsync();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithEnsure_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = Result.Aggregate().Ensure(true, Error.Validation.MissingField("f")).Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithWhen_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = Result.Aggregate().When(() => true, Error.Validation.MissingField("f")).Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithWhenAsync_ThenBuildAsync_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                async Task M()
                {
                    var r = await Result.Aggregate().WhenAsync(() => Task.FromResult(true), Error.Validation.MissingField("f")).BuildAsync();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Aggregate_WithWhenAll_ThenBuildAsync_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                async Task M()
                {
                    var r = await Result.Aggregate().WhenAll(new WhenCondition[] { (() => Task.FromResult(true), Error.Validation.MissingField("f")) }).BuildAsync();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Build_OnStoredAggregatorVariable_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var a = Result.Aggregate();
                    a.Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Build_WithUnrecognizedChainMethod_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            static class AggExt
            {
                public static ResultAggregator Foo(this ResultAggregator a) => a;
            }

            class C
            {
                void M()
                {
                    var r = Result.Aggregate().Foo().Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task UnrelatedAggregateAndBuild_Type_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Other
            {
                public static Other Aggregate() => new Other();
                public int Build() => 0;
            }

            class C
            {
                void M()
                {
                    var r = Other.Aggregate().Build();
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Compilation_WithoutLightningArcResults_NoDiagnostic()
    {
        const string code = """
            class C
            {
                void M()
                {
                }
            }
            """;

        await AnalyzerVerifier<EmptyResultAggregatorChainAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Empty_Sync_Chain_CodeFix_Should_Replace_With_Success()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = [|Result.Aggregate().Build()|];
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class C
            {
                void M()
                {
                    var r = Result.Success();
                }
            }
            """;

        await CodeFixVerifier<
            EmptyResultAggregatorChainAnalyzer,
            EmptyResultAggregatorChainCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task CodeFix_With_BuildAsync_Offers_No_Fix()
    {
        // The provider must never offer the sync-only rewrite for BuildAsync.
        // No API path produces a check-less Task<ResultAggregator>, so drive
        // the provider directly with a diagnostic on a real BuildAsync chain.
        const string source = """
            using System.Threading.Tasks;
            using LightningArc.Results;

            class C
            {
                async Task M()
                {
                    var r = await Result.Aggregate().CheckAsync(() => Task.FromResult(Result.Success())).BuildAsync();
                }
            }
            """;

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
        var buildAsync = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(i =>
                i.Expression is MemberAccessExpressionSyntax member
                && member.Name.Identifier.ValueText == "BuildAsync"
            );

        Diagnostic diagnostic = Diagnostic.Create(
            EmptyResultAggregatorChainAnalyzer.Rule,
            Location.Create(tree, buildAsync.Span)
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new EmptyResultAggregatorChainCodeFixProvider().RegisterCodeFixesAsync(context);

        await Assert.That(fixesOffered).IsEqualTo(0);
    }
}
