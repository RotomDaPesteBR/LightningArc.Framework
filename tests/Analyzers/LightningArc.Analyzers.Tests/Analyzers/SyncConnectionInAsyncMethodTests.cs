using System.Diagnostics.CodeAnalysis;
using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Data.ADO.Repositories;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class SyncConnectionInAsyncMethodTests
{
    private const string Usings = """
        using LightningArc.Data.ADO.Repositories;
        using System.Threading.Tasks;
        """;

    [Test]
    public async Task Sync_GetConnection_In_Async_Method_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task DoWorkAsync()
                {
                    var conn = [|GetConnection()|];
                }
            }
            """;

        await AnalyzerVerifier<SyncConnectionInAsyncMethodAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Async_GetConnection_In_Async_Method_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public async Task DoWorkAsync()
                {
                    var conn = await GetConnectionAsync();
                }
            }
            """;

        await AnalyzerVerifier<SyncConnectionInAsyncMethodAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Sync_GetConnection_In_Sync_Method_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class MyRepository() : RepositoryBase(null!)
            {
                public void DoWork()
                {
                    var conn = GetConnection();
                }
            }
            """;

        await AnalyzerVerifier<SyncConnectionInAsyncMethodAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
