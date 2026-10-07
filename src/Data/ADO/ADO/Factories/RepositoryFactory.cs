using System.Collections.Concurrent;
using System.Data.Common;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Data.ADO.Factories;

/// <summary>
/// A concrete, generic implementation of IRepositoryFactory for ADO.NET/Dapper repositories.
/// Uses ActivatorUtilities to dynamically instantiate concrete implementations from their interfaces.
/// </summary>
public sealed class RepositoryFactory(
    IServiceProvider serviceProvider,
    IConnectionFactory connectionFactory,
    IMapper? mapper = null
) : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMapper? _mapper = mapper;
    private readonly ConcurrentDictionary<Type, Type> _implementationCache = new();

    /// <inheritdoc />
    public TRepository Create<TRepository>()
        where TRepository : class
    {
        return CreateInstance<TRepository>(_connectionFactory);
    }

    /// <inheritdoc />
    public TRepository Create<TRepository>(IConnectionFactory connectionFactory)
        where TRepository : class
    {
        return CreateInstance<TRepository>(connectionFactory);
    }

    /// <inheritdoc />
    public TRepository Create<TRepository>(DbConnection connection, DbTransaction transaction)
        where TRepository : class
    {
        return CreateInstance<TRepository>(connection, transaction);
    }

    /// <inheritdoc />
    public TRepository Create<TRepository>(IDbUnitOfWork dbUnitOfWork)
        where TRepository : class
    {
#if NETSTANDARD2_0
        if (dbUnitOfWork is null)
        {
            throw new ArgumentNullException(nameof(dbUnitOfWork));
        }
#else
        ArgumentNullException.ThrowIfNull(dbUnitOfWork);
#endif

        // Extracts connection and transaction from the specialized Unit of Work.
        // Both are null while the Unit of Work is not active: fail fast with a
        // clear message instead of a cryptic constructor-matching error.
        DbConnection? connection = dbUnitOfWork.Connection;
        DbTransaction? transaction = dbUnitOfWork.Transaction;

        if (connection is null || transaction is null)
        {
            throw new InvalidOperationException(
                "Unit of Work is not active. Call Begin() or BeginAsync() before creating repositories from it."
            );
        }

        return CreateInstance<TRepository>(connection, transaction);
    }

    /// <summary>
    /// Core method that handles the dynamic instantiation using ActivatorUtilities.
    /// Supports both interfaces and concrete classes as the generic argument.
    /// </summary>
    private TRepository CreateInstance<TRepository>(params object[] explicitArgs)
        where TRepository : class
    {
        Type targetType = typeof(TRepository);

        // 1. Resolve interfaces/abstract classes safely (cached: avoids
        // instantiating a throwaway repository on every call).
        if (targetType.IsInterface || targetType.IsAbstract)
        {
            targetType = _implementationCache.GetOrAdd(targetType, ResolveImplementation);
        }

        // 2. Only connection/transaction go explicit. IMapper and ILogger<T>
        //    are resolved from the provider by ActivatorUtilities: passing a
        //    non-generic ILogger explicitly breaks constructors declaring
        //    ILogger<TRepository>. _mapper is a fallback for providers
        //    without IMapper registered.
        List<object> args = [.. explicitArgs];
        if (_mapper != null && _serviceProvider.GetService<IMapper>() is null)
        {
            args.Add(_mapper);
        }

        // 3. Instantiate the type using ActivatorUtilities.
        object newInstance = ActivatorUtilities.CreateInstance(
            _serviceProvider,
            targetType,
            [.. args]
        );

        return (TRepository)newInstance;
    }

    private Type ResolveImplementation(Type contract)
    {
        object? registeredService = null;

        // Safe lookup using a temporary scope to prevent "scoped service from root provider" exception
        using (var scope = _serviceProvider.CreateScope())
        {
            registeredService = scope.ServiceProvider.GetService(contract);
        }

        if (registeredService != null)
        {
            return registeredService.GetType();
        }

        // Highly reliable scanning: Look for a concrete class implementing the contract
        Type? resolvedType = contract
            .Assembly.GetTypes()
            .FirstOrDefault(t => contract.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        resolvedType ??= AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => contract.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        return resolvedType
            ?? throw new InvalidOperationException(
                $"Could not automatically resolve a concrete implementation for '{contract.Name}'."
            );
    }
}
