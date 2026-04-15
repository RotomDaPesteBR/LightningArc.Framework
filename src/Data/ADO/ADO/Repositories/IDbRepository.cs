using System.Data.Common;
using Microsoft.Extensions.Logging;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Factories;

namespace LightningArc.Data.ADO.Repositories;

/// <summary>
/// Defines the contract for a self-creating ADO.NET/Dapper repository.
/// </summary>
public interface IDbRepository<TRepository>
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a new instance of the repository with an existing connection and transaction.
    /// </summary>
    public static abstract TRepository Create(
        DbConnection connection,
        DbTransaction transaction,
        IMapper? mapper = null,
        ILogger<TRepository>? logger = null
    );

    /// <summary>
    /// Creates a new instance of the repository using a connection factory.
    /// </summary>
    public static abstract TRepository Create(
        IConnectionFactory connectionFactory,
        IMapper? mapper = null,
        ILogger<TRepository>? logger = null
    );
#endif
}

