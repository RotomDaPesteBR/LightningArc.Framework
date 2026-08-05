using System.Data.Common;
using LightningArc.Data.ADO.Factories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LightningArc.Data.ADO.SqlServer.Factories;

/// <summary>
/// Implementation of <see cref="IConnectionFactory"/> for SQL Server databases.
/// </summary>
public class SqlConnectionFactory : IConnectionFactory
{
    /// <summary>
    /// The connection string used to connect to the SQL Server database.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The logger instance for recording connection-related diagnostic information.
    /// </summary>
    private readonly ILogger<SqlConnectionFactory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class using <see cref="IConfiguration"/>
    /// and a custom connection string name.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
    public SqlConnectionFactory(
        IConfiguration configuration,
        string connectionName = "DatabaseConnection"
    )
    {
#if NETSTANDARD2_0
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
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class using a direct connection string.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentException(
                "Connection string cannot be null or empty.",
                nameof(connectionString)
            );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class with logging and custom connection string support.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
    public SqlConnectionFactory(
        ILogger<SqlConnectionFactory> logger,
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
        DbConnection connection = new SqlConnection(_connectionString);

        if (_logger?.IsEnabled(LogLevel.Debug) ?? false)
        {
            _logger?.LogDebug("Connection created");
        }

        return connection;
    }
}
