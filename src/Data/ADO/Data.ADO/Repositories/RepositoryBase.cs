using System.Data;
using System.Data.Common;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LightningArc.Data.ADO.Repositories;

/// <summary>
/// Provides the basic infrastructure for data access operations using <see cref="System.Data.Common.DbConnection"/>.
/// </summary>
public abstract class RepositoryBase
{
    /// <summary>
    /// The database connection instance, if provided directly.
    /// </summary>
    protected readonly DbConnection? DbConnection;

    /// <summary>
    /// The current database transaction, if provided.
    /// </summary>
    protected readonly DbTransaction? Transaction;

    /// <summary>
    /// The factory responsible for creating new <see cref="System.Data.Common.DbConnection"/> instances.
    /// </summary>
    protected readonly IConnectionFactory? ConnectionFactory;

    /// <summary>
    /// The logger instance for recording diagnostic and error information.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The mapper used for transforming data between models and entities.
    /// </summary>
    protected IMapper Mapper =>
        field
        ?? throw new InvalidOperationException(
            $"The Mapper instance has not been initialized for '{GetType().Name}'. Ensure that an IMapper implementation is provided via the constructor."
        );

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase"/> class using a connection factory.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper = null)
    {
        ConnectionFactory = connectionFactory;
        Mapper = mapper;
        Logger = NullLogger.Instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase"/> class using a connection factory and a logger.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper, ILogger? logger)
        : this(connectionFactory, mapper) => Logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase"/> class using an existing connection and transaction.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper = null
    )
    {
        DbConnection = dbConnection;
        Transaction = transaction;
        Mapper = mapper;
        Logger = NullLogger.Instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase"/> class using an existing connection, transaction, and a logger.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper,
        ILogger? logger
    )
        : this(dbConnection, transaction, mapper) => Logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// Retrieves a database connection. Returns the existing connection if provided,
    /// or creates a new one using the connection factory.
    /// </summary>
    /// <returns>An instance of <see cref="System.Data.Common.DbConnection"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the repository is not properly initialized with a connection or a factory.</exception>
    protected DbConnection GetConnection()
    {
        if (DbConnection != null)
        {
            return DbConnection;
        }

        if (ConnectionFactory == null)
            throw new InvalidOperationException("Repository not properly initialized.");

        DbConnection dbConnection = ConnectionFactory.GetConnection();

        if (dbConnection.State != ConnectionState.Open)
        {
            dbConnection.Open();
        }

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Connection opened");
        }

        return dbConnection;
    }

    /// <summary>
    /// Asynchronously retrieves a database connection. Returns the existing connection if provided,
    /// or creates a new one using the connection factory.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that contains an instance of <see cref="System.Data.Common.DbConnection"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the repository is not properly initialized with a connection or a factory.</exception>
    protected async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (DbConnection != null)
        {
            return DbConnection;
        }

        if (ConnectionFactory == null)
            throw new InvalidOperationException("Repository not properly initialized.");

        DbConnection dbConnection = ConnectionFactory.GetConnection();

        if (dbConnection.State != ConnectionState.Open)
        {
            await dbConnection.OpenAsync(cancellationToken);
        }

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Connection opened");
        }

        return dbConnection;
    }

    /// <summary>
    /// Closes and disposes of the provided database connection if it was not externally managed
    /// (i.e., if no transaction or persistent connection was provided).
    /// </summary>
    /// <param name="dbConnection">The database connection to release.</param>
    protected void ReleaseConnection(DbConnection? dbConnection)
    {
        if (Transaction != null || DbConnection != null)
            return;

        dbConnection?.Close();
        dbConnection?.Dispose();
    }
}

/// <summary>
/// Generic base for ADO.NET repositories where the entity is returned directly.
/// </summary>
/// <typeparam name="TEntity">The type of the database entity.</typeparam>
public abstract class RepositoryBase<TEntity> : RepositoryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class using a connection factory.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper = null)
        : base(connectionFactory, mapper) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class using a connection factory and a logger.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper, ILogger? logger)
        : base(connectionFactory, mapper, logger) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class using an existing connection and transaction.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper = null
    )
        : base(dbConnection, transaction, mapper) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class using an existing connection, transaction, and a logger.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper,
        ILogger? logger
    )
        : base(dbConnection, transaction, mapper, logger) { }
}

/// <summary>
/// Generic base for ADO.NET/Dapper repositories, supporting mapping between database entities and result models (e.g., DTOs).
/// </summary>
/// <typeparam name="TEntity">The type of the database entity (source).</typeparam>
/// <typeparam name="TResult">The type of the result model (destination).</typeparam>
public abstract class RepositoryBase<TEntity, TResult> : RepositoryBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TResult}"/> class using a connection factory.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper = null)
        : base(connectionFactory, mapper) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TResult}"/> class using a connection factory and a logger.
    /// </summary>
    /// <param name="connectionFactory">The factory to create database connections.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(IConnectionFactory connectionFactory, IMapper? mapper, ILogger? logger)
        : base(connectionFactory, mapper, logger) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TResult}"/> class using an existing connection and transaction.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper = null
    )
        : base(dbConnection, transaction, mapper) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TResult}"/> class using an existing connection, transaction, and a logger.
    /// </summary>
    /// <param name="dbConnection">The active database connection.</param>
    /// <param name="transaction">The active database transaction.</param>
    /// <param name="mapper">An optional mapper for data transformation.</param>
    /// <param name="logger">The logger instance for this repository.</param>
    protected RepositoryBase(
        DbConnection dbConnection,
        DbTransaction transaction,
        IMapper? mapper,
        ILogger? logger
    )
        : base(dbConnection, transaction, mapper, logger) { }
}

