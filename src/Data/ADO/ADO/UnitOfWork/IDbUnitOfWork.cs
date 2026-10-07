using System.Data.Common;
using LightningArc.Data.Abstractions.UnitOfWork;

namespace LightningArc.Data.ADO.UnitOfWork;

/// <summary>
/// Specialized Unit of Work for database-level operations (Dapper/ADO.NET).
/// Exposes the underlying connection and transaction.
/// </summary>
public interface IDbUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Gets the active database connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is not <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the active database transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">The unit of work is not <see cref="UnitOfWorkState.Active"/>.</exception>
    /// <exception cref="ObjectDisposedException">The unit of work is <see cref="UnitOfWorkState.Disposed"/>.</exception>
    DbTransaction Transaction { get; }
}
