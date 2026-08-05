using System.Data.Common;
using LightningArc.Data.ADO.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

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
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class using <see cref="IConfiguration"/>
    /// and a custom connection string name.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
    public OracleConnectionFactory(
        IConfiguration configuration,
        string connectionName = "DatabaseConnection"
    )
    {
#if NETSTANDARD2_1
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#else
        ArgumentNullException.ThrowIfNull(configuration);
#endif

        _connectionString =
            configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionName}' not found in the configuration."
            );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class using a direct connection string.
    /// </summary>
    /// <param name="connectionString">The Oracle connection string.</param>
    public OracleConnectionFactory(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentException(
                "Connection string cannot be null or empty.",
                nameof(connectionString)
            );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleConnectionFactory"/> class with logging and custom connection string support.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
    public OracleConnectionFactory(
        ILogger<OracleConnectionFactory> logger,
        IConfiguration configuration,
        string connectionName = "DatabaseConnection"
    )
        : this(configuration, connectionName)
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
