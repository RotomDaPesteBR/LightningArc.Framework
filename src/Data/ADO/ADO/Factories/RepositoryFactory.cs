using System.Data.Common;
using System.Reflection;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Repositories;
using Microsoft.Extensions.Logging;

namespace LightningArc.Data.ADO.Factories;

/// <summary>
/// A concrete, generic implementation of IRepositoryFactory for ADO.NET/Dapper repositories.
/// </summary>
public sealed class RepositoryFactory(
    IConnectionFactory connectionFactory,
    IMapper? mapper = null,
    ILoggerFactory? loggerFactory = null
) : IRepositoryFactory
{
    private readonly IConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMapper? _mapper = mapper;
    private readonly ILoggerFactory? _loggerFactory = loggerFactory;

    /// <inheritdoc />
    public TRepository Create<TRepository>()
        where TRepository : IDbRepository<TRepository>
    {
#if NET7_0_OR_GREATER
        return TRepository.Create(
            _connectionFactory,
            _mapper,
            _loggerFactory?.CreateLogger<TRepository>()
        );
#else
        return InvokeCreate<TRepository>(
            [_connectionFactory, _mapper, _loggerFactory?.CreateLogger<TRepository>()]
        );
#endif
    }

    /// <inheritdoc />
    public TRepository Create<TRepository>(IConnectionFactory connectionFactory)
        where TRepository : IDbRepository<TRepository>
    {
#if NET7_0_OR_GREATER
        return TRepository.Create(
            connectionFactory,
            _mapper,
            _loggerFactory?.CreateLogger<TRepository>()
        );
#else
        return InvokeCreate<TRepository>(
            [connectionFactory, _mapper, _loggerFactory?.CreateLogger<TRepository>()]
        );
#endif
    }

    /// <inheritdoc />
    public TRepository Create<TRepository>(DbConnection connection, DbTransaction transaction)
        where TRepository : IDbRepository<TRepository>
    {
#if NET7_0_OR_GREATER
        return TRepository.Create(
            connection,
            transaction,
            _mapper,
            _loggerFactory?.CreateLogger<TRepository>()
        );
#else
        return InvokeCreate<TRepository>(
            [connection, transaction, _mapper, _loggerFactory?.CreateLogger<TRepository>()]
        );
#endif
    }

#if !NET7_0_OR_GREATER
    private static TRepository InvokeCreate<TRepository>(object?[] args)
    {
        Type type = typeof(TRepository);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Create");

        foreach (MethodInfo? method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
                continue;

            bool match = !parameters
                .Where((t, i) => args[i] != null && !t.ParameterType.IsInstanceOfType(args[i]!))
                .Any();

            if (match)
                return (TRepository)method.Invoke(null, args)!;
        }

        throw new InvalidOperationException(
            $"Could not find a suitable static 'Create' method on type {type.Name}."
        );
    }
#endif
}

