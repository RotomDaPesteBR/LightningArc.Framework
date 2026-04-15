using System.Data;
using System.Data.Common;
using LightningArc.Data.ADO.Factories;

namespace LightningArc.Data.ADO.UnitOfWork;

/// <summary>
/// Implementation of the Unit of Work pattern for database operations using ADO.NET.
/// Manages the lifecycle of a <see cref="DbConnection"/> and its associated <see cref="DbTransaction"/>.
/// </summary>
/// <param name="connectionFactory">The factory responsible for providing database connections.</param>
public sealed class UnitOfWork(IConnectionFactory connectionFactory) : IDbUnitOfWork
{
    private bool _isDisposed;
    private bool _isStarted;

    /// <inheritdoc />
    public DbConnection Connection
    {
        get => field ??
               throw new InvalidOperationException("Unit of Work not started. Call Begin() or BeginAsync() first.");
        private set;
    }

    /// <inheritdoc />
    public DbTransaction Transaction
    {
        get => field ??
               throw new InvalidOperationException("Unit of Work not started. Call Begin() or BeginAsync() first.");
        private set;
    }

    /// <inheritdoc />
    public void Begin()
    {
        if (_isStarted)
        {
            throw new InvalidOperationException("Unit of Work already started.");
        }

        Connection = connectionFactory.GetConnection();

        if (Connection.State != ConnectionState.Open)
        {
            Connection.Open();
        }

        Transaction = Connection.BeginTransaction();
        _isStarted = true;
    }

    /// <inheritdoc />
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
        {
            throw new InvalidOperationException("Unit of Work already started.");
        }

        Connection = connectionFactory.GetConnection();

        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync(cancellationToken);
        }

#if NETSTANDARD2_0
        Transaction = Connection.BeginTransaction();
        await Task.CompletedTask;
#else
        Transaction = await Connection.BeginTransactionAsync(cancellationToken);
#endif

        _isStarted = true;
    }

    /// <inheritdoc />
    public void Commit()
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException("Unit of Work not started. Call Begin() or BeginAsync() first.");
        }

        Transaction.Commit();
        Dispose();
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException("Unit of Work not started. Call Begin() or BeginAsync() first.");
        }

#if NETSTANDARD2_0
        Transaction.Commit();
        await Task.CompletedTask;
#else
        await Transaction.CommitAsync(cancellationToken);
#endif
        Dispose();
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (!_isStarted) return;

        Transaction.Rollback();
        Dispose();
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
        {
#if NETSTANDARD2_0
            Transaction.Rollback();
            await Task.CompletedTask;
#else
            await Transaction.RollbackAsync(cancellationToken);
#endif
            Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_isStarted)
        {
            try
            {
                Transaction.Dispose();
                Connection.Close();
                Connection.Dispose();
            }
            catch (InvalidOperationException)
            {
                // In case the connection or transaction were already disposed or closed.
            }
            finally
            {
                _isStarted = false;
                Connection = null!;
                Transaction = null!;
            }
        }

        _isDisposed = true;
    }
}

