using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class DirectDbConnectionTests
{
    private const string Usings = """
        using System.Data;
        using System.Data.Common;
        using LightningArc.Data.ADO.Repositories;
        """;

    [Test]
    public async Task New_DbConnection_In_Repository_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class FakeConnection : DbConnection 
            {
                protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new System.NotImplementedException();
                public override void ChangeDatabase(string databaseName) => throw new System.NotImplementedException();
                public override void Close() => throw new System.NotImplementedException();
                public override string ConnectionString { get; set; }
                public override string Database => "";
                public override string DataSource => "";
                public override void Open() => throw new System.NotImplementedException();
                public override string ServerVersion => "";
                public override ConnectionState State => ConnectionState.Closed;
                protected override DbCommand CreateDbCommand() => throw new System.NotImplementedException();
            }

            public class MyRepository : RepositoryBase
            {
                public MyRepository() : base((DbConnection)null!, (DbTransaction)null!) {}

                public void DoWork()
                {
                    var conn = [|new FakeConnection()|];
                }
            }
            """;

        await AnalyzerVerifier<DirectDbConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task GetConnection_In_Repository_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class MyRepository : RepositoryBase
            {
                public MyRepository() : base((DbConnection)null!, (DbTransaction)null!) {}

                public void DoWork()
                {
                    var conn = GetConnection();
                }
            }
            """;

        await AnalyzerVerifier<DirectDbConnectionAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
