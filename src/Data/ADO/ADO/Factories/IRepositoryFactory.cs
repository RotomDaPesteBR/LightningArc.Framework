using System.Data.Common;
using LightningArc.Data.ADO.UnitOfWork;

namespace LightningArc.Data.ADO.Factories;

/// <summary>
/// Defines a factory for creating repository instances that depend on <see cref="DbConnection"/>.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    /// Creates a repository instance using default settings.
    /// </summary>
    TRepository Create<TRepository>()
        where TRepository : class;

    /// <summary>
    /// Creates a repository instance using a specific connection factory.
    /// </summary>
    TRepository Create<TRepository>(IConnectionFactory connectionFactory)
        where TRepository : class;

    /// <summary>
    /// Creates a repository instance using an existing connection and transaction.
    /// </summary>
    TRepository Create<TRepository>(DbConnection connection, DbTransaction transaction)
        where TRepository : class;

    /// <summary>
    /// Creates a repository instance managed by an active database Unit of Work.
    /// </summary>
    /// <typeparam name="TRepository">The repository interface or concrete type.</typeparam>
    /// <param name="dbUnitOfWork">The active database unit of work providing the connection and transaction.</param>
    /// <returns>A new instance of the repository bound to the Unit of Work's lifecycle.</returns>
    TRepository Create<TRepository>(IDbUnitOfWork dbUnitOfWork)
        where TRepository : class;
}
