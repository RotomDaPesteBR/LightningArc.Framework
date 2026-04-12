
#if NET8_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Security.Cryptography;
using LightningArc.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Handles global exceptions and converts them into standardized <see cref="Error"/> responses
/// consistent with the library's <see cref="EndpointResult"/> pattern.
/// </summary>
public sealed class ResultExceptionHandler(
    ILogger<ResultExceptionHandler> logger
) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        Error error = MapToError(exception);
        
        // We use the internal ErrorResult which handles status code, problem details and logging
        // according to the configuration in ErrorMappingService.
        ErrorResult result = new(error);
        
        await result.ExecuteAsync(httpContext);

        return true;
    }

    private static Error MapToError(Exception exception)
    {
        return exception switch
        {
            ValidationException => Error.Validation.InvalidParameter(),
            
            ArgumentNullException => Error.Validation.MissingField(),
            
            ArgumentException => Error.Application.InvalidParameter(),
            
            UnauthorizedAccessException => Error.Authentication.Forbidden(),
            
            AuthenticationException => Error.Authentication.Unauthorized(),
            
            CryptographicException => Error.Application.Internal(),
            
            InvalidOperationException => Error.Application.InvalidOperation(),
            
            TaskCanceledException or OperationCanceledException => Error.Application.TaskCanceled(),
            
            NotImplementedException => Error.Application.NotImplemented(),
            
            OutOfMemoryException => Error.System.OutOfMemory(),
            
            TimeoutException => Error.Network.RequestTimeout(),

            // Base case for database exceptions
            System.Data.Common.DbException => Error.Database.ConnectionFailed(),

            _ => Error.Application.Internal()
        };
    }
}
#endif




