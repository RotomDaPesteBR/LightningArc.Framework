using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.UnitOfWork;

namespace LightningArc.Data.Tests.UnitOfWork;

public class UnitOfWorkTests
{
    private class TestDbConnection : DbConnection
    {
        public int CloseCount { get; private set; }
        public int DisposeCount { get; private set; }
        public int OpenCount { get; private set; }
        public TestDbTransaction? LastTransaction { get; private set; }

        [AllowNull]
        public override string ConnectionString { get; set; } = "";
        public override string Database => "";
        public override string DataSource => "";
        public override string ServerVersion => "";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }

        public override void Close() => CloseCount++;

        public override void Open() => OpenCount++;

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            LastTransaction = new TestDbTransaction(this);
            return LastTransaction;
        }

        protected override DbCommand CreateDbCommand() => null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private class TestDbTransaction(DbConnection connection) : DbTransaction
    {
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public int DisposeCount { get; private set; }

        protected override DbConnection DbConnection => connection;
        public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;

        public override void Commit() => CommitCount++;
        public override void Rollback() => RollbackCount++;

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

    [Test]
    public async Task Begin_ShouldStartTransaction()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);

        // Act
        uow.Begin();

        // Assert
        await Assert.That(uow.Connection).IsEqualTo(mockConn);
        await Assert.That(uow.Transaction).IsNotNull();
        await Assert.That(uow.Transaction).IsEqualTo(mockConn.LastTransaction);
    }

    [Test]
    public async Task Begin_ShouldThrow_WhenAlreadyStarted()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);
        uow.Begin();

        // Act & Assert
        await Assert.That(() => uow.Begin()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Commit_ShouldCommitAndDispose()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);
        uow.Begin();
        TestDbTransaction trans = (TestDbTransaction)uow.Transaction!;

        // Act
        uow.Commit();

        // Assert
        await Assert.That(trans.CommitCount).IsEqualTo(1);
        await Assert.That(trans.DisposeCount).IsEqualTo(1);
        await Assert.That(mockConn.CloseCount).IsEqualTo(1);
        await Assert.That(mockConn.DisposeCount).IsEqualTo(1);
    }

    [Test]
    public async Task Commit_ShouldThrow_WhenNotStarted()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);

        // Act & Assert
        await Assert.That(() => uow.Commit()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Rollback_ShouldRollbackAndDispose()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);
        uow.Begin();
        TestDbTransaction trans = (TestDbTransaction)uow.Transaction!;

        // Act
        uow.Rollback();

        // Assert
        await Assert.That(trans.RollbackCount).IsEqualTo(1);
        await Assert.That(trans.DisposeCount).IsEqualTo(1);
        await Assert.That(mockConn.CloseCount).IsEqualTo(1);
        await Assert.That(mockConn.DisposeCount).IsEqualTo(1);
    }

    [Test]
    public async Task Rollback_ShouldNotThrow_WhenNotStarted()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);

        // Act
        uow.Rollback();

        // Assert (No exception expected)
    }

    [Test]
    public async Task Connection_ShouldThrow_WhenAccessedBeforeBegin()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);

        // Act & Assert
        await Assert.That(() => uow.Connection).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Transaction_ShouldThrow_WhenAccessedBeforeBegin()
    {
        // Arrange
        TestDbConnection mockConn = new();
        ConnectionFactoryStub factory = new(mockConn);
        ADO.UnitOfWork.UnitOfWork uow = new(factory);

        // Act & Assert
        await Assert.That(() => uow.Transaction).Throws<InvalidOperationException>();
    }
}

