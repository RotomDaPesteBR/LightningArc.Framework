using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using LightningArc.Data.ADO.Factories;

namespace LightningArc.Data.ADO.Oracle.Factories;

/// <summary>
/// Implementation of <see cref="IConnectionFactory"/> for Oracle databases.
/// </summary>
public class OracleConnectionFactory : IConnectionFactory
{
    /// <summary>
    /// The connection string used to connect to the Oracle database.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The logger instance for recording connection-related diagnostic information.
    /// </summary>
    private readonly ILogger<OracleConnectionFactory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class using <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection string is not found.</exception>
    public OracleConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DatabaseConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DatabaseConnection' not found."
            );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class using a direct connection string.
    /// </summary>
    /// <param name="connectionString">The Oracle connection string.</param>
    public OracleConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class with logging support.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="config">The configuration containing the connection string.</param>
    public OracleConnectionFactory(ILogger<OracleConnectionFactory> logger, IConfiguration config)
        : this(config)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public DbConnection GetConnection()
    {
        DbConnection connection = new OracleConnection(_connectionString);

        if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
        {
            _logger?.LogDebug("Connection created");
        }

        return connection;
    }
}

