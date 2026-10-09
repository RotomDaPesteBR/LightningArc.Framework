using LightningArc.Data.Abstractions.UnitOfWork;
using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.UnitOfWork;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering core ADO.NET infrastructure services within the <see cref="IServiceCollection"/>.
/// </summary>
public static class AdoServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the <see cref="RepositoryFactory"/> as a Singleton service.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddRepositoryFactory() =>
            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

        /// <summary>
        /// Registers the ADO.NET-based <see cref="UnitOfWork"/> as a Scoped service under the <see cref="IDbUnitOfWork"/> contract.
        /// Also aliases <see cref="IUnitOfWork"/> to the same scoped instance, so services
        /// (via <see cref="IUnitOfWork"/>) and repositories (via <see cref="IDbUnitOfWork"/>)
        /// always share one transaction per scope. Registering them separately would
        /// silently split transactions.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
        public IServiceCollection AddDbUnitOfWork() =>
            services
                .AddScoped<IDbUnitOfWork, UnitOfWork>()
                .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IDbUnitOfWork>());
    }
}
