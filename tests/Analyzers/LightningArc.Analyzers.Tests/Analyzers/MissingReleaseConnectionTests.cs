using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class MissingReleaseConnectionTests
{
    private const string Usings = """
        #nullable enable
        using System.Data.Common;
        using System.Threading.Tasks;
        using LightningArc.Data.ADO.Repositories;
        """;

    [Test]
    public async Task GetConnection_WithoutTryFinally_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var c = [|GetConnection()|];
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnectionAsync_WithFinallyLackingRelease_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task DoWorkAsync()
                {
                    var c = await [|GetConnectionAsync()|];
                    try { }
                    finally { }
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_WithFinallyCallingUnrelatedRelease_Reports()
    {
        const string code = $$"""
            {{Usings}}

            static class Helper
            {
                public static void ReleaseConnection(object? o) { }
            }

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var c = [|GetConnection()|];
                    try { }
                    finally { Helper.ReleaseConnection(c); }
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_WithProperTryFinallyRelease_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var c = GetConnection();
                    try { }
                    finally { ReleaseConnection(c); }
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_ForwardedViaArrowBody_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                protected DbConnection Forward() => GetConnection();
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_ForwardedViaReturn_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public DbConnection Forward()
                {
                    return GetConnection();
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnectionAsync_ForwardedViaReturnAwait_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task<DbConnection> ForwardAsync()
                {
                    return await GetConnectionAsync();
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_OnNonRepositoryBaseType_NoDiagnostic()
    {
        const string code = """
            class Plain
            {
                public object GetConnection() => new object();

                public void DoWork()
                {
                    var c = GetConnection();
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_InConstructorBody_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository : RepositoryBase
            {
                public MyRepository()
                    : base((LightningArc.Data.ADO.Factories.IConnectionFactory)null!)
                {
                    var c = GetConnection();
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    // Documents a known deliberate limitation (GAP-9): the analyzer only looks for
    // ReleaseConnection within the same method body as the acquisition, so splitting
    // acquire and release across two methods still reports. This pins CURRENT
    // behavior; do not "fix" the analyzer to satisfy this test.
    [Test]
    public async Task GetConnection_ReleasedInDifferentMethod_StillReports()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                private DbConnection? _c;

                public void Acquire()
                {
                    _c = [|GetConnection()|];
                }

                public void ReleaseIt()
                {
                    try { }
                    finally { ReleaseConnection(_c); }
                }
            }
            """;

        await AnalyzerVerifier<MissingReleaseConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
