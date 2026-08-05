using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.SqlServer.Factories;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering SQL Server connection factories within the <see cref="IServiceCollection"/>.
/// </summary>
public static class SqlServerServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the default <see cref="SqlConnectionFactory"/> for SQL Server using the specified configuration connection string name.
        /// </summary>
        /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddSqlConnectionFactory(
            string connectionName = "DatabaseConnection"
        ) =>
            services.AddSingleton<IConnectionFactory, SqlConnectionFactory>(sp =>
                ActivatorUtilities.CreateInstance<SqlConnectionFactory>(sp, connectionName)
            );

        /// <summary>
        /// Registers a Keyed <see cref="SqlConnectionFactory"/> for a secondary SQL Server database.
        /// </summary>
        /// <param name="serviceKey">The unique key used to resolve this specific connection factory instance.</param>
        /// <param name="connectionName">The name of the connection string in the configuration.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddKeyedSqlConnectionFactory(
            string serviceKey,
            string connectionName
        ) =>
            services.AddKeyedSingleton<IConnectionFactory, SqlConnectionFactory>(
                serviceKey,
                (sp, key) =>
                    ActivatorUtilities.CreateInstance<SqlConnectionFactory>(sp, connectionName)
            );
    }
}
