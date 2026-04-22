using LightningArc.Validations.DependencyInjection.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LightningArc.Validations.DependencyInjection;

/// <summary>
/// Provides extension methods for registering validators into a service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers validators using the supplied registration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional configuration callback for validator registration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Action<ValidationRegistrationOptions>? configure = null)
    {
        ValidationRegistrationOptions options = new();
        configure?.Invoke(options);

        ValidatorRegistrationScanner.RegisterFromAssemblies(
            services,
            options.Assemblies,
            options.DefaultLifetime);

        services.TryAddScoped<IValidationService, ValidationService>();

        return services;
    }

    /// <summary>
    /// Registers a validator implementation against its closed validator interface.
    /// </summary>
    /// <typeparam name="TValidator">The validator implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The validator service lifetime.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TValidator"/> does not implement exactly one closed <see cref="IValidator{T}"/> interface.</exception>
    public static IServiceCollection AddValidator<TValidator>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TValidator : class
    {
        Type implementationType = typeof(TValidator);
        Type[] serviceTypes = implementationType
            .GetInterfaces()
            .Where(interfaceType =>
                interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToArray();

        if (serviceTypes.Length != 1)
        {
            throw new InvalidOperationException(
                $"Type '{implementationType.FullName}' must implement exactly one closed IValidator<T> contract. Use AddValidator<TModel, TValidator>() for explicit registration.");
        }

        services.TryAddScoped<IValidationService, ValidationService>();
        services.Add(new ServiceDescriptor(serviceTypes[0], implementationType, lifetime));
        return services;
    }

    /// <summary>
    /// Registers a validator implementation against the specified closed validator interface.
    /// </summary>
    /// <typeparam name="TModel">The validated model type.</typeparam>
    /// <typeparam name="TValidator">The validator implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The validator service lifetime.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddValidator<TModel, TValidator>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TValidator : class, IValidator<TModel>
    {
        services.TryAddScoped<IValidationService, ValidationService>();
        services.Add(new ServiceDescriptor(typeof(IValidator<TModel>), typeof(TValidator), lifetime));
        return services;
    }
}
