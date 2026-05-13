using System.Collections.Concurrent;
using System.Data.Common;
using System.Security.Authentication;
using LightningArc.Results.Localization;

namespace LightningArc.Results.Exceptions;

/// <summary>
/// Provides a centralized registry and transformer for mapping exceptions to <see cref="Error"/> objects.
/// </summary>
public static class ExceptionMapper
{
    private static readonly ConcurrentDictionary<Type, Func<Exception, Error>> _mappers = new();

    static ExceptionMapper()
    {
        RegisterDefaultMappers();
    }

    /// <summary>
    /// Registers a custom mapper for a specific exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="mapper">The mapper function.</param>
    public static void Register<TException>(Func<TException, Error> mapper)
        where TException : Exception
    {
        _mappers[typeof(TException)] = ex => mapper((TException)ex);
    }

    /// <summary>
    /// Transforms an exception into an <see cref="Error"/> object using the registered mappers.
    /// </summary>
    /// <param name="exception">The exception to transform.</param>
    /// <returns>The resulting <see cref="Error"/> object.</returns>
    public static Error Map(Exception exception)
    {
#if NETSTANDARD2_0
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }
#else
        ArgumentNullException.ThrowIfNull(exception);
#endif

        // Special handling for AggregateException
        if (exception is AggregateException aggregate)
        {
            var errors = aggregate.InnerExceptions.Select(Map).ToList();
            if (errors.Count == 1)
            {
                return errors[0];
            }

            // Combine all errors using the '+' operator
            Error result = errors[0];
            for (int i = 1; i < errors.Count; i++)
            {
                result += errors[i];
            }
            return result;
        }

        Type? type = exception.GetType();

        // Exact match or closest ancestor
        while (type != null && type != typeof(object))
        {
            if (_mappers.TryGetValue(type, out var mapper))
            {
                return mapper(exception);
            }
            type = type.BaseType;
        }

        // Default fallback
        return Error.Application.Internal(
            LocalizationManager.GetErrorString("Application_InternalError")
        );
    }

    private static void RegisterDefaultMappers()
    {
        // Validation
        Register<ArgumentNullException>(ex =>
            CreateValidationError(
                ex.ParamName,
                "Validation_MissingField_WithParam",
                "Validation_MissingField",
                Error.Validation.MissingField
            )
        );

        Register<ArgumentOutOfRangeException>(ex =>
            CreateValidationError(
                ex.ParamName,
                "Validation_ValueOutOfRange_WithParam",
                "Validation_ValueOutOfRange",
                Error.Validation.ValueOutOfRange
            )
        );

        Register<ArgumentException>(ex =>
            CreateValidationError(
                ex.ParamName,
                "Validation_InvalidParameter_WithParam",
                "Validation_InvalidParameter",
                Error.Validation.InvalidParameter
            )
        );

        Register<FormatException>(_ => Error.Validation.InvalidFormat());
        Register<InvalidCastException>(_ => Error.Validation.InvalidFormat());

        // Authentication/Authorization
        Register<UnauthorizedAccessException>(_ => Error.Authentication.Forbidden());
        Register<AuthenticationException>(_ => Error.Authentication.Unauthorized());

        // IO
        Register<FileNotFoundException>(_ => Error.IO.FileNotFound());
        Register<DirectoryNotFoundException>(_ => Error.IO.DirectoryNotFound());
        Register<DriveNotFoundException>(_ => Error.IO.FileNotFound());
        Register<IOException>(_ => Error.IO.CorruptedFile());

        // Network
        Register<TimeoutException>(_ => Error.Network.RequestTimeout());
        Register<System.Net.Sockets.SocketException>(_ => Error.Network.ConnectionFailed());

        // Database (BCL Common)
        Register<DbException>(_ => Error.Database.QueryExecutionFailed());

        // Resource / Search
        Register<KeyNotFoundException>(_ => Error.Resource.NotFound());
        Register<IndexOutOfRangeException>(_ => Error.Resource.NotFound());

        // Application/System
        Register<InvalidOperationException>(_ => Error.Application.InvalidOperation());
        Register<NotImplementedException>(_ => Error.Application.NotImplemented());
        Register<OutOfMemoryException>(_ => Error.System.OutOfMemory());
        Register<TaskCanceledException>(_ => Error.Application.TaskCanceled());

        // Arithmetic
        Register<DivideByZeroException>(_ => Error.Application.InvalidOperation());
        Register<OverflowException>(_ => Error.Application.InvalidOperation());
    }

    private static Error CreateValidationError(
        string? paramName,
        string withParamKey,
        string genericKey,
        Func<string?, IEnumerable<ErrorDetail>?, Error> factory
    )
    {
        if (!string.IsNullOrWhiteSpace(paramName))
        {
            string template = LocalizationManager.GetErrorString(withParamKey);
            // Protect against templates that don't have {0} but were called with paramName
            try
            {
                string message = string.Format(
                    LocalizationManager.CurrentCulture,
                    template,
                    paramName
                );
                return factory(message, null);
            }
            catch (FormatException)
            {
                return factory(null, null);
            }
        }

        return factory(null, null);
    }
}
