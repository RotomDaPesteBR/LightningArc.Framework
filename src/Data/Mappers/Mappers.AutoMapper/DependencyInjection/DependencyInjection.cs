using LightningArc.Mappers.AutoMapper.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Mappers.AutoMapper.DependencyInjection;

/// <summary>
/// Extension methods for registering AutoMapper adapter in the <see cref="IServiceCollection"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers AutoMapper adapter implementation of <see cref="LightningArc.Data.Abstractions.Mappers.IMapper"/>.
    /// </summary>
    /// <remarks>
    /// Note: You still need to register AutoMapper itself using <c>AddAutoMapper</c>
    /// from the <c>AutoMapper.Extensions.Microsoft.DependencyInjection</c> package.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAutoMapperAdapter(this IServiceCollection services)
    {
        services.AddScoped<LightningArc.Data.Abstractions.Mappers.IMapper, AutoMapperAdapter>();
        return services;
    }
}
