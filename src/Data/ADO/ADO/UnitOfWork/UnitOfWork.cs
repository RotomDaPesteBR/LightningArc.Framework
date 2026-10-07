using System.Data;
using System.Data.Common;
using LightningArc.Data.Abstractions.UnitOfWork;
using LightningArc.Data.ADO.Factories;

namespace LightningArc.Data.ADO.UnitOfWork;

/// <summary>
/// Implementation of the Unit of Work pattern for database operations using ADO.NET.
/// Manages the lifecycle of a <see cref="DbConnection"/> and its associated <see cref="DbTransaction"/>.
/// </summary>
/// <remarks>
/// Instances are reusable: after a commit or rollback the underlying resources are released and a new
/// transaction can be started with <see cref="Begin"/> or <see cref="BeginAsync"/>.
/// Only <see cref="Dispose"/> and <see cref="DisposeAsync"/> make the instance permanently unusable.
/// This type is not thread-safe.
/// </remarks>
/// <param name="connectionFactory">The factory responsible for providing database connections.</param>
public sealed class UnitOfWork(IConnectionFactory connectionFactory) : IDbUnitOfWork
{
    private DbConnection? _connection;
    private DbTransaction? _transaction;

    /// <inheritdoc />
    public UnitOfWorkState State { get; private set; } = UnitOfWorkState.NotStarted;

    /// <inheritdoc />
    public DbConnection Connection =>
        State == UnitOfWorkState.Active ? _connection! : throw CreateNotActiveException();

    /// <inheritdoc />
    public DbTransaction Transaction =>
        State == UnitOfWorkState.Active ? _transaction! : throw CreateNotActiveException();

    /// <inheritdoc />
    public void Begin()
    {
        EnsureCanBegin();

        try
        {
            _connection = connectionFactory.GetConnection();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
            State = UnitOfWorkState.Active;
        }
        catch
        {
            // A failed Begin must not leave a stale State behind (e.g. Committed
            // from a previous cycle masking the failure).
            State = UnitOfWorkState.Failed;
            // Do not leak a connection that was obtained but could not be fully initialized.
            ReleaseResources();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        EnsureCanBegin();

        try
        {
            _connection = connectionFactory.GetConnection();

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

#if NETSTANDARD2_0
            _transaction = _connection.BeginTransaction();
#else
            _transaction = await _connection
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
#endif

            State = UnitOfWorkState.Active;
        }
        catch
        {
            // A failed Begin must not leave a stale State behind (see Begin()).
            State = UnitOfWorkState.Failed;
            // Do not leak a connection that was obtained but could not be fully initialized.
            await ReleaseResourcesAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public void Commit()
    {
        EnsureActive();

        try
        {
            _transaction!.Commit();
            State = UnitOfWorkState.Committed;
        }
        catch
        {
            State = UnitOfWorkState.Failed;
            throw;
        }
        finally
        {
            ReleaseResources();
        }
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureActive();

        try
        {
#if NETSTANDARD2_0
            _transaction!.Commit();
#else
            await _transaction!.CommitAsync(cancellationToken).ConfigureAwait(false);
#endif
            State = UnitOfWorkState.Committed;
        }
        catch
        {
            State = UnitOfWorkState.Failed;
            throw;
        }
        finally
        {
            await ReleaseResourcesAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (State != UnitOfWorkState.Active)
        {
            return;
        }

        try
        {
            _transaction!.Rollback();
            State = UnitOfWorkState.RolledBack;
        }
        catch (Exception)
        {
            // Suppress or log rollback exceptions (e.g., connection lost) to ensure resources are released.
            // The outcome could not be confirmed, so the state is Failed.
            State = UnitOfWorkState.Failed;
        }
        finally
        {
            ReleaseResources();
        }
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (State != UnitOfWorkState.Active)
        {
            return;
        }

        try
        {
#if NETSTANDARD2_0
            _transaction!.Rollback();
#else
            // CRITICAL: We intentionally use CancellationToken.None here.
            // If the user's token is canceled, we MUST still attempt to rollback the transaction
            // to release database locks and avoid leaving the transaction in an uncommitted limbo state.
            await _transaction!.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
#endif
            State = UnitOfWorkState.RolledBack;
        }
        catch (Exception)
        {
            // Suppress or log rollback exceptions to ensure resources are released smoothly.
            // The outcome could not be confirmed, so the state is Failed.
            State = UnitOfWorkState.Failed;
        }
        finally
        {
            await ReleaseResourcesAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (State == UnitOfWorkState.Disposed)
        {
            return;
        }

        // Disposing an active transaction without committing it causes an implicit rollback.
        ReleaseResources();
        State = UnitOfWorkState.Disposed;
    }

    /// <summary>
    /// Performs asynchronous disposal of managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (State == UnitOfWorkState.Disposed)
        {
            return;
        }

        await ReleaseResourcesAsync().ConfigureAwait(false);
        State = UnitOfWorkState.Disposed;
    }

    /// <summary>
    /// Ensures a new transaction can be started from the current state.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The unit of work was disposed.</exception>
    /// <exception cref="InvalidOperationException">A transaction is already active.</exception>
    private void EnsureCanBegin()
    {
        if (State == UnitOfWorkState.Disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        if (State == UnitOfWorkState.Active)
        {
            throw new InvalidOperationException($"Unit of Work already started (state: {State}).");
        }
    }

    /// <summary>
    /// Ensures a transaction is currently active.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The unit of work was disposed.</exception>
    /// <exception cref="InvalidOperationException">No transaction is active.</exception>
    private void EnsureActive()
    {
        if (State != UnitOfWorkState.Active)
        {
            throw CreateNotActiveException();
        }
    }

    /// <summary>
    /// Creates the exception that describes why the unit of work cannot be used in its current state.
    /// </summary>
    /// <returns>An <see cref="ObjectDisposedException"/> when disposed; otherwise an <see cref="InvalidOperationException"/>.</returns>
    private Exception CreateNotActiveException() =>
        State == UnitOfWorkState.Disposed
            ? new ObjectDisposedException(nameof(UnitOfWork))
            : new InvalidOperationException(
                $"Unit of Work is not active (state: {State}). Call Begin() or BeginAsync() first."
            );

    /// <summary>
    /// Releases the current transaction and connection without changing <see cref="State"/>.
    /// Exceptions thrown during cleanup are suppressed so that every resource gets a disposal attempt.
    /// </summary>
    private void ReleaseResources()
    {
        var transaction = _transaction;
        var connection = _connection;
        _transaction = null;
        _connection = null;

        try
        {
            transaction?.Dispose();
        }
        catch (Exception)
        {
            // Prevent exceptions from bubbling up during disposal phases
        }

        try
        {
            // Disposing the connection also closes it.
            connection?.Dispose();
        }
        catch (Exception)
        {
            // Prevent exceptions from bubbling up during disposal phases
        }
    }

    /// <summary>
    /// Performs asynchronous cleanup of database resources without changing <see cref="State"/>.
    /// </summary>
    private ValueTask ReleaseResourcesAsync()
    {
#if NETSTANDARD2_0
        // Fallback for .NET Standard 2.0 which lacks async dispose for ADO.NET
        ReleaseResources();
        return default;
#else
        return ReleaseResourcesCoreAsync();
#endif
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Asynchronously disposes the current transaction and connection.
    /// Exceptions thrown during cleanup are suppressed so that every resource gets a disposal attempt.
    /// </summary>
    private async ValueTask ReleaseResourcesCoreAsync()
    {
        var transaction = _transaction;
        var connection = _connection;
        _transaction = null;
        _connection = null;

        if (transaction is not null)
        {
            try
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Prevent exceptions from bubbling up during asynchronous disposal
            }
        }

        if (connection is not null)
        {
            try
            {
                // Disposing the connection also closes it.
                await connection.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Prevent exceptions from bubbling up during asynchronous disposal
            }
        }
    }
#endif
}
