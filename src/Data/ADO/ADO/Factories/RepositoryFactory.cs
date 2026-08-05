using System.Data.Common;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LightningArc.Data.ADO.Factories;

/// <summary>
/// A concrete, generic implementation of IRepositoryFactory for ADO.NET/Dapper repositories.
/// Uses ActivatorUtilities to dynamically instantiate concrete implementations from their interfaces.
/// </summary>
public sealed class RepositoryFactory(
    IServiceProvider serviceProvider,
    IConnectionFactory connectionFactory,
    IMapper? mapper = null,
    ILoggerFactory? loggerFactory = null
) : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMapper? _mapper = mapper;
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;

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

        // Extracts connection and transaction from the specialized Unit of Work
        return CreateInstance<TRepository>(dbUnitOfWork.Connection, dbUnitOfWork.Transaction);
    }

    /// <summary>
    /// Core method that handles the dynamic instantiation using ActivatorUtilities.
    /// Supports both interfaces and concrete classes as the generic argument.
    /// </summary>
    private TRepository CreateInstance<TRepository>(params object[] explicitArgs)
        where TRepository : class
    {
        Type targetType = typeof(TRepository);

        // 1. Resolve interfaces/abstract classes safely
        if (targetType.IsInterface || targetType.IsAbstract)
        {
            object? registeredService = null;

            // Safe lookup using a temporary scope to prevent "scoped service from root provider" exception
            using (var scope = _serviceProvider.CreateScope())
            {
                registeredService = scope.ServiceProvider.GetService<TRepository>();
            }

            if (registeredService != null)
            {
                targetType = registeredService.GetType();
            }
            else
            {
                // Highly reliable scanning: Look for a concrete class implementing TRepository
                Type? resolvedType = targetType
                    .Assembly.GetTypes()
                    .FirstOrDefault(t =>
                        targetType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
                    );

                resolvedType ??= AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t =>
                        targetType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
                    );

                targetType =
                    resolvedType
                    ?? throw new InvalidOperationException(
                        $"Could not automatically resolve a concrete implementation for '{targetType.Name}'."
                    );
            }
        }

        // 2. Build argument list: explicit args + mapper + logger
        List<object> args = [.. explicitArgs];
        if (_mapper != null)
        {
            args.Add(_mapper);
        }

        ILogger? logger = null;
        if (_loggerFactory != null)
        {
            logger = _loggerFactory.CreateLogger(targetType);
        }

        if (logger != null)
        {
            args.Add(logger);
        }

        // 3. Instantiate the type using ActivatorUtilities.
        object newInstance = ActivatorUtilities.CreateInstance(
            _serviceProvider,
            targetType,
            [.. args]
        );

        return (TRepository)newInstance;
    }
}
