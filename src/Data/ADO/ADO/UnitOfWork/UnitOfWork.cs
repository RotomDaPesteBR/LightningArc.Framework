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
        get =>
            field
            ?? throw new InvalidOperationException(
                "Unit of Work not started. Call Begin() or BeginAsync() first."
            );
        private set;
    }

    /// <inheritdoc />
    public DbTransaction Transaction
    {
        get =>
            field
            ?? throw new InvalidOperationException(
                "Unit of Work not started. Call Begin() or BeginAsync() first."
            );
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
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

#if NETSTANDARD2_0
        Transaction = Connection.BeginTransaction();
        await Task.CompletedTask;
#else
        Transaction = await Connection
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
#endif

        _isStarted = true;
    }

    /// <inheritdoc />
    public void Commit()
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException(
                "Unit of Work not started. Call Begin() or BeginAsync() first."
            );
        }

        try
        {
            Transaction.Commit();
        }
        finally
        {
            Dispose();
        }
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException(
                "Unit of Work not started. Call Begin() or BeginAsync() first."
            );
        }

        try
        {
#if NETSTANDARD2_0
            Transaction.Commit();
            await Task.CompletedTask;
#else
            await Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
#endif
        }
        finally
        {
#if NETSTANDARD2_0
            Dispose();
#else
            await DisposeAsync().ConfigureAwait(false);
#endif
        }
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (!_isStarted)
        {
            return;
        }

        try
        {
            Transaction.Rollback();
        }
        catch (Exception)
        {
            // Suppress or log rollback exceptions (e.g., connection lost) to ensure Dispose runs
        }
        finally
        {
            Dispose();
        }
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
        {
            return;
        }

        try
        {
#if NETSTANDARD2_0
            Transaction.Rollback();
            await Task.CompletedTask;
#else
            // CRITICAL: We intentionally use CancellationToken.None here.
            // If the user's token is canceled, we MUST still attempt to rollback the transaction
            // to release database locks and avoid leaving the transaction in an uncommitted limbo state.
            await Transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
#endif
        }
        catch (Exception)
        {
            // Suppress or log rollback exceptions to ensure Dispose/DisposeAsync runs smoothly
        }
        finally
        {
#if NETSTANDARD2_0
            Dispose();
#else
            await DisposeAsync().ConfigureAwait(false);
#endif
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Performs asynchronous disposal of managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing && _isStarted)
        {
            try
            {
                // Synchronous cleanup fallback
                Transaction.Dispose();

                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }

                Connection.Dispose();
            }
            catch (Exception)
            {
                // Prevent exceptions from bubbling up during disposal phases
            }
            finally
            {
                ResetState();
            }
        }

        _isDisposed = true;
    }

    /// <summary>
    /// Performs asynchronous cleanup of database resources.
    /// </summary>
    private async ValueTask DisposeAsyncCore()
    {
        if (_isDisposed || !_isStarted)
        {
            return;
        }

        try
        {
#if !NETSTANDARD2_0
            await Transaction.DisposeAsync().ConfigureAwait(false);

            if (Connection.State != ConnectionState.Closed)
            {
                await Connection.CloseAsync().ConfigureAwait(false);
            }

            await Connection.DisposeAsync().ConfigureAwait(false);
#else
            // Fallback for .NET Standard 2.0 which lacks async dispose for ADO.NET
            Transaction?.Dispose();
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
            await Task.CompletedTask;
#endif
        }
        catch (Exception)
        {
            // Prevent exceptions from bubbling up during asynchronous disposal
        }
        finally
        {
            ResetState();
        }
    }

    /// <summary>
    /// Resets the internal operational state of the Unit of Work.
    /// </summary>
    private void ResetState()
    {
        _isStarted = false;
        Connection = null!;
        Transaction = null!;
    }
}
