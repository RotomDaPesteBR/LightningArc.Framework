using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.Repositories;
using LightningArc.Data.ADO.Tests.Mocks;
using Microsoft.Extensions.Logging;

namespace LightningArc.Data.Tests.Repositories;

public class RepositoryBaseTests
{
    private class TestDbConnection : DbConnection
    {
        public int CloseCount { get; private set; }
        public int DisposeCount { get; private set; }

        [AllowNull]
        public override string ConnectionString { get; set; } = "";
        public override string Database => "";
        public override string DataSource => "";
        public override string ServerVersion => "";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }

        public override void Close() => CloseCount++;

        public override void Open() { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null!;

        protected override DbCommand CreateDbCommand() => null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private class ConnectionFactoryStub(DbConnection connection) : IConnectionFactory
    {
        public DbConnection GetConnection() => connection;
    }

    private class InternalTestRepository : RepositoryBase
    {
        public InternalTestRepository(IConnectionFactory factory)
            : base(factory) { }

        public InternalTestRepository(IConnectionFactory factory, IMapper? mapper, ILogger? logger)
            : base(factory, mapper, logger) { }

        public InternalTestRepository(DbConnection conn, DbTransaction trans)
            : base(conn, trans) { }

        public new DbConnection GetConnection() => base.GetConnection();

        public new void ReleaseConnection(DbConnection conn) => base.ReleaseConnection(conn);

        public new ILogger Logger => base.Logger;
        public new IMapper Mapper => base.Mapper;
    }

    [Test]
    public async Task Logger_ShouldNotBeNull_EvenWhenNullProvided()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        InternalTestRepository repo = new(factory, null, null);

        // Act
        ILogger logger = repo.Logger;

        // Assert
        await Assert.That(logger).IsNotNull();
    }

    [Test]
    public async Task Mapper_ShouldThrow_WhenNotProvided()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        InternalTestRepository repo = new(factory);

        // Act & Assert
        await Assert.That(() => repo.Mapper).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Mapper_ShouldReturnProvidedInstance()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        MockMapper mockMapper = new();
        InternalTestRepository repo = new(factory, mockMapper, null);

        // Act
        IMapper mapper = repo.Mapper;

        // Assert
        await Assert.That(mapper).IsEqualTo(mockMapper);
    }

    [Test]
    public async Task GetConnection_ShouldReturnFactoryConnection_WhenNoConnectionProvided()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        InternalTestRepository repo = new(factory);

        // Act
        DbConnection conn = repo.GetConnection();

        // Assert
        await Assert.That(conn).IsEqualTo(mockConn);
    }

    [Test]
    public async Task GetConnection_ShouldReturnProvidedConnection_WhenConnectionProvidedInConstructor()
    {
        // Arrange
        TestDbConnection mockConn = new();
        InternalTestRepository repo = new(mockConn, null!);

        // Act
        DbConnection conn = repo.GetConnection();

        // Assert
        await Assert.That(conn).IsEqualTo(mockConn);
    }

    [Test]
    public async Task ReleaseConnection_ShouldNotClose_WhenConnectionWasProvidedInConstructor()
    {
        // Arrange
        TestDbConnection mockConn = new();
        InternalTestRepository repo = new(mockConn, null!);

        // Act
        repo.ReleaseConnection(mockConn);

        // Assert
        await Assert.That(mockConn.CloseCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReleaseConnection_ShouldCloseAndDispose_WhenConnectionWasCreatedLocally()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        InternalTestRepository repo = new(factory);

        // Act
        repo.ReleaseConnection(mockConn);

        // Assert
        await Assert.That(mockConn.CloseCount).IsEqualTo(1);
        await Assert.That(mockConn.DisposeCount).IsEqualTo(1);
    }
}


