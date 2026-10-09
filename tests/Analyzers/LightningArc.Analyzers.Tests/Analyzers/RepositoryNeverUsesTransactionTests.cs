using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class RepositoryNeverUsesTransactionTests
{
    private const string Usings = """
        using System.Data;
        using System.Data.Common;
        using System.Threading.Tasks;
        using LightningArc.Data.ADO.Repositories;
        using Dapper;
        """;

    [Test]
    public async Task Repository_With_Db_Calls_And_No_Transaction_Reference_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class [|MyRepository|] : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1");
                    await conn.ExecuteAsync("UPDATE t SET x = 1");
                    var single = conn.QuerySingle<int>("SELECT 1");
                }
            }
            """;

        await AnalyzerVerifier<RepositoryNeverUsesTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Repository_With_Transaction_Reference_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1", transaction: Transaction);
                    await conn.ExecuteAsync("UPDATE t SET x = 1");
                }
            }
            """;

        await AnalyzerVerifier<RepositoryNeverUsesTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Abstract_Repository_With_Db_Calls_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public abstract class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1");
                }
            }
            """;

        await AnalyzerVerifier<RepositoryNeverUsesTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Repository_With_No_Db_Calls_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public void DoWork()
                {
                }
            }
            """;

        await AnalyzerVerifier<RepositoryNeverUsesTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Non_Repository_Class_With_Db_Calls_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class PlainService
            {
                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1");
                }
            }
            """;

        await AnalyzerVerifier<RepositoryNeverUsesTransactionAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
