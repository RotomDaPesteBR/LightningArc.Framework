using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.Oracle.Factories;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering Oracle connection factories within the <see cref="IServiceCollection"/>.
/// </summary>
public static class OracleServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the default <see cref="OracleConnectionFactory"/> for Oracle using the specified configuration connection string name.
        /// </summary>
        /// <param name="connectionName">The name of the connection string in the configuration (defaults to 'DatabaseConnection').</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddOracleConnectionFactory(
            string connectionName = "DatabaseConnection"
        ) =>
            services.AddSingleton<IConnectionFactory, OracleConnectionFactory>(sp =>
                ActivatorUtilities.CreateInstance<OracleConnectionFactory>(sp, connectionName)
            );

        /// <summary>
        /// Registers a Keyed <see cref="OracleConnectionFactory"/> for a secondary Oracle database.
        /// </summary>
        /// <param name="serviceKey">The unique key used to resolve this specific connection factory instance.</param>
        /// <param name="connectionName">The name of the connection string in the configuration.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddKeyedOracleConnectionFactory(
            string serviceKey,
            string connectionName
        ) =>
            services.AddKeyedSingleton<IConnectionFactory, OracleConnectionFactory>(
                serviceKey,
                (sp, key) =>
                    ActivatorUtilities.CreateInstance<OracleConnectionFactory>(sp, connectionName)
            );
    }
}
