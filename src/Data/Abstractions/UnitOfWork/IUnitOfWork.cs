namespace LightningArc.Data.Abstractions.UnitOfWork;

/// <summary>
/// Defines the interface for the Unit of Work pattern, responsible for coordinating
/// business transactions and data persistence operations.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the current lifecycle state of the unit of work.
    /// </summary>
    /// <remarks>
    /// Implementations are not required to be thread-safe. Reading this property from another
    /// thread while an operation is in progress provides no guarantees.
    /// </remarks>
    UnitOfWorkState State { get; }

    /// <summary>
    /// Starts a new business transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is already <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    void Begin();

    /// <summary>
    /// Asynchronously starts a new business transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is already <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the changes made during the transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is not <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    void Commit();

    /// <summary>
    /// Asynchronously commits the changes made during the transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is not <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the changes made during the transaction.
    /// </summary>
    /// <remarks>
    /// This operation is idempotent: when the unit of work is not <see cref="UnitOfWorkState.Active"/>,
    /// the call does nothing.
    /// </remarks>
    void Rollback();

    /// <summary>
    /// Asynchronously rolls back the changes made during the transaction.
    /// </summary>
    /// <remarks>
    /// This operation is idempotent: when the unit of work is not <see cref="UnitOfWorkState.Active"/>,
    /// the call does nothing.
    /// </remarks>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
