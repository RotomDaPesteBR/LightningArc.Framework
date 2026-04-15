using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LightningArc.Data.ADO.Factories;

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
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class using <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The configuration containing the connection string.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection string is not found.</exception>
    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DatabaseConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DatabaseConnection' not found."
            );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class using a direct connection string.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class with logging support.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="config">The configuration containing the connection string.</param>
    public SqlConnectionFactory(ILogger<SqlConnectionFactory> logger, IConfiguration config)
        : this(config)
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

