using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
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
