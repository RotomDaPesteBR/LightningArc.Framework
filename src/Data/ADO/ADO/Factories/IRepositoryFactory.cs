using System.Data.Common;
using LightningArc.Data.ADO.Repositories;

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
        where TRepository : IDbRepository<TRepository>;

    /// <summary>
    /// Creates a repository instance using a specific connection factory.
    /// </summary>
    TRepository Create<TRepository>(IConnectionFactory connectionFactory)
        where TRepository : IDbRepository<TRepository>;

    /// <summary>
    /// Creates a repository instance using an existing connection and transaction.
    /// </summary>
    TRepository Create<TRepository>(DbConnection connection, DbTransaction transaction)
        where TRepository : IDbRepository<TRepository>;
}

