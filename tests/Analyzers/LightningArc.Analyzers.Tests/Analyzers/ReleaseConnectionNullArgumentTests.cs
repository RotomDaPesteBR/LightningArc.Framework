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

public class ReleaseConnectionNullArgumentTests
{
    private const string Usings = """
        using System;
        using System.Data.Common;
        using LightningArc.Data.ADO.Repositories;
        """;

    [Test]
    public async Task ReleaseConnection_With_Null_Literal_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    [|ReleaseConnection(null)|];
                }
            }
            """;

        await AnalyzerVerifier<ReleaseConnectionNullArgumentAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ReleaseConnection_With_Default_Literal_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    [|ReleaseConnection(default)|];
                }
            }
            """;

        await AnalyzerVerifier<ReleaseConnectionNullArgumentAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ReleaseConnection_With_Object_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var conn = new object();
                    ReleaseConnection((DbConnection)conn);
                }
            }
            """;

        await AnalyzerVerifier<ReleaseConnectionNullArgumentAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ReleaseConnection_Null_From_Derived_Class_Reports()
    {
        // Regression pin (renumbering fix): ReleaseConnection is declared on
        // RepositoryBase itself, so methodSymbol.ContainingType IS RepositoryBase.
        // A naive InheritsFromRepositoryBase-only check walks ancestors and misses
        // this exact case; reverting it drops the diagnostic and fails.
        string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    [|ReleaseConnection(null)|];
                }
            }
            """;

        await AnalyzerVerifier<ReleaseConnectionNullArgumentAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Sync_GetConnection_CodeFix_Should_Substitute_Single_Candidate()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var conn = GetConnection();
                    try { }
                    finally { [|ReleaseConnection(null)|]; }
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var conn = GetConnection();
                    try { }
                    finally { ReleaseConnection(conn); }
                }
            }
            """;

        await CodeFixVerifier<
            ReleaseConnectionNullArgumentAnalyzer,
            ReleaseConnectionNullArgumentCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Async_GetConnection_CodeFix_Should_Substitute_Single_Candidate()
    {
        const string code = $$"""
            {{Usings}}
            using System.Threading.Tasks;

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task DoWorkAsync()
                {
                    var conn = await GetConnectionAsync();
                    try { }
                    finally { [|ReleaseConnection(default)|]; }
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}
            using System.Threading.Tasks;

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task DoWorkAsync()
                {
                    var conn = await GetConnectionAsync();
                    try { }
                    finally { ReleaseConnection(conn); }
                }
            }
            """;

        await CodeFixVerifier<
            ReleaseConnectionNullArgumentAnalyzer,
            ReleaseConnectionNullArgumentCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task CodeFix_With_No_Connection_Local_Offers_No_Fix()
    {
        const string source = """
            using System.Data.Common;
            using LightningArc.Data.ADO.Repositories;

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    ReleaseConnection(null);
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Two_Connection_Locals_Offers_No_Fix()
    {
        const string source = """
            using System.Data.Common;
            using LightningArc.Data.ADO.Repositories;

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var first = GetConnection();
                    var second = GetConnection();
                    ReleaseConnection(null);
                }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    private static async Task<int> CountFixesOfferedAsync(string source)
    {
        // The diagnostic still fires with zero/multiple candidates, but the
        // provider must decline. Drive the provider directly with a diagnostic
        // on the ReleaseConnection invocation, since CodeFixVerifier cannot
        // express "diagnostic present, no fix offered".
        using AdhocWorkspace workspace = new();
        var project = workspace
            .AddProject("Guards", LanguageNames.CSharp)
            .AddMetadataReference(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(System.Data.Common.DbConnection).Assembly.Location
                )
            )
            .AddMetadataReference(
                MetadataReference.CreateFromFile(
                    typeof(LightningArc.Data.ADO.Repositories.RepositoryBase).Assembly.Location
                )
            );
        Document document = project.AddDocument("Guard.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var invocation = root
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(i => i.Expression.ToString().Contains("ReleaseConnection"));

        Diagnostic diagnostic = Diagnostic.Create(
            ReleaseConnectionNullArgumentAnalyzer.Rule,
            Location.Create(tree, invocation.Span)
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new ReleaseConnectionNullArgumentCodeFixProvider().RegisterCodeFixesAsync(context);

        return fixesOffered;
    }

    [Test]
    public async Task Unrelated_ReleaseConnection_Method_With_Null_NoDiagnostic()
    {
        // Regression pin (renumbering fix): the call must resolve to
        // RepositoryBase.ReleaseConnection. A bare name match flags this
        // unrelated same-named method; reverting the check reports and fails.
        string code = $$"""
            {{Usings}}

            class Plain
            {
                public void ReleaseConnection(DbConnection conn) { }

                public void DoWork()
                {
                    ReleaseConnection(null);
                }
            }
            """;

        await AnalyzerVerifier<ReleaseConnectionNullArgumentAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
