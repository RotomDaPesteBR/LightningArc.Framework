namespace LightningArc.Data.Abstractions.UnitOfWork;

/// <summary>
/// Defines the interface for the Unit of Work pattern, responsible for coordinating
/// business transactions and data persistence operations.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Starts a new business transaction.
    /// </summary>
    void Begin();

    /// <summary>
    /// Asynchronously starts a new business transaction.
    /// </summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the changes made during the transaction.
    /// </summary>
    void Commit();

    /// <summary>
    /// Asynchronously commits the changes made during the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the changes made during the transaction.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Asynchronously rolls back the changes made during the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

