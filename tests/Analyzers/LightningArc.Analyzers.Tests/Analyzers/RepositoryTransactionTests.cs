using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class RepositoryTransactionTests
{
    private const string Usings = """
        using System.Data;
        using System.Data.Common;
        using System.Threading.Tasks;
        using LightningArc.Data.ADO.Repositories;
        using Dapper;
        """;

    [Test]
    public async Task Dapper_Call_Without_Transaction_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await [|conn.QueryAsync<int>("SELECT 1")|];
                }
            }
            """;

        await AnalyzerVerifier<RepositoryTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Dapper_Call_With_Transaction_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1", transaction: Transaction);
                }
            }
            """;

        await AnalyzerVerifier<RepositoryTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
