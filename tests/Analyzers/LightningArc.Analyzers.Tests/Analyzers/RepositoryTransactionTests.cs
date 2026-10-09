using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
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

    // The shared CodeFixVerifier harness does not reference Dapper, so these
    // fix tests declare a minimal local stand-in for the Dapper call shape: a
    // method named QueryAsync with a "transaction" parameter. The analyzer keys
    // off the method name + transaction parameter, not the Dapper type.
    private const string FixUsings = """
        #nullable enable
        using System.Collections.Generic;
        using System.Data;
        using System.Data.Common;
        using System.Threading.Tasks;
        using LightningArc.Data.ADO.Repositories;
        """;

    private const string Stub = """
        public static class DbStubs
        {
            public static Task<IEnumerable<int>> QueryAsync(this IDbConnection connection, string sql, IDbTransaction? transaction = null)
            {
                return Task.FromResult<IEnumerable<int>>(new List<int>());
            }
        }
        """;

    [Test]
    public async Task Missing_Transaction_CodeFix_Should_Add_Transaction_Argument()
    {
        const string code = $$"""
            {{FixUsings}}

            {{Stub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await [|conn.QueryAsync("SELECT 1")|];
                }
            }
            """;

        const string fixedCode = $$"""
            {{FixUsings}}

            {{Stub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync("SELECT 1", transaction: Transaction);
                }
            }
            """;

        await CodeFixVerifier<
            RepositoryTransactionAnalyzer,
            RepositoryTransactionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Missing_Transaction_Generic_Call_CodeFix_Should_Append_Transaction_Argument()
    {
        const string genericStub = """
            public static class DbGenericStubs
            {
                public static Task<IEnumerable<int>> QueryAsync<T>(this IDbConnection connection, string sql, IDbTransaction? transaction = null)
                {
                    return Task.FromResult<IEnumerable<int>>(new List<int>());
                }
            }
            """;

        const string code = $$"""
            {{FixUsings}}

            {{genericStub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await [|conn.QueryAsync<int>("SELECT 1")|];
                }
            }
            """;

        const string fixedCode = $$"""
            {{FixUsings}}

            {{genericStub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync<int>("SELECT 1", transaction: Transaction);
                }
            }
            """;

        await CodeFixVerifier<
            RepositoryTransactionAnalyzer,
            RepositoryTransactionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Missing_Transaction_FixAll_Should_Fix_Both_Occurrences()
    {
        const string code = $$"""
            {{FixUsings}}

            {{Stub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await [|conn.QueryAsync("SELECT 1")|];
                    await [|conn.QueryAsync("SELECT 2")|];
                }
            }
            """;

        const string fixedCode = $$"""
            {{FixUsings}}

            {{Stub}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository(DbConnection conn, DbTransaction trans) : base(conn, trans) {}

                public async Task DoWork(IDbConnection conn)
                {
                    await conn.QueryAsync("SELECT 1", transaction: Transaction);
                    await conn.QueryAsync("SELECT 2", transaction: Transaction);
                }
            }
            """;

        await CodeFixVerifier<
            RepositoryTransactionAnalyzer,
            RepositoryTransactionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }
}
