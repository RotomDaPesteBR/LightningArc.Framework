using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.CORS.AspNetCore.Policies
{
    /// <summary>
    /// Provides extension methods for adding the "AllowAll" CORS policy.
    /// </summary>
    public static class AllowAll
    {
        /// <summary>
        /// Adds a CORS policy named "AllowAll" that allows all origins, methods, and headers.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the CORS policy will be added.</param>
        /// <returns>The modified <see cref="IServiceCollection"/>, for call chaining.</returns>
        /// <remarks>
        /// This policy configures CORS to allow:
        /// <list type="bullet">
        ///     <item><description>All headers (`*`)</description></item>
        ///     <item><description>All HTTP methods (`*`)</description></item>
        ///     <item><description>All origins (`*`)</description></item>
        /// </list>
        /// Use this policy with caution, as it removes default CORS security restrictions.
        /// </remarks>
        public static IServiceCollection AddAllowAllPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            return services;
        }
    }
}
