using Microsoft.Extensions.DependencyInjection;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Mappers.Mapster.Adapters;
using Mapster;
using MapsterMapper;

namespace LightningArc.Mappers.Mapster.DependencyInjection;

/// <summary>
/// Extension methods for registering Mapster adapter in the <see cref="IServiceCollection"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Mapster and its adapter implementation of <see cref="LightningArc.Data.Abstractions.Mappers.IMapper"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">Optional Mapster configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMapsterAdapter(this IServiceCollection services, TypeAdapterConfig? config = null)
    {
        if (config != null)
        {
            services.AddSingleton(config);
        }
        else
        {
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        }

        // Use the standard Mapster registration
        services.AddMapster();
        
        // Register our specific adapter for our abstraction
        services.AddSingleton<LightningArc.Data.Abstractions.Mappers.IMapper, MapsterAdapter>();

        return services;
    }
}

