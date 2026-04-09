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
}
