using System.Net;
using LightningArc.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Represents an HTTP result that maps a <see cref="Success"/> instance to a standard success response.
/// </summary>
/// <param name="success">The success instance to be mapped.</param>
public sealed class SuccessResult(Success success) : IResult
{
    /// <summary>
    /// Executes the response asynchronously, formatting the success result and writing it to the HTTP context.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        SuccessMappingService mappingService =
            httpContext.RequestServices.GetRequiredService<SuccessMappingService>();
        EndpointResultOptions options = httpContext
            .RequestServices.GetRequiredService<IOptions<EndpointResultOptions>>()
            .Value;

        SuccessMapping? mapping = mappingService.GetMapping(success);
        int statusCode = (int)(mapping?.StatusCode ?? HttpStatusCode.OK);

        httpContext.Response.StatusCode = statusCode;

        if (
            statusCode == (int)HttpStatusCode.NoContent
            || !options.WrapSuccessResponses
            || options.SuccessResponseBuilder == null
        )
        {
            return Task.CompletedTask;
        }

        SuccessDetail successDetails = new()
        {
            Status = mapping?.StatusCode ?? HttpStatusCode.OK,
            Message = success.Message ?? "",
            Data = null,
        };

        object? response = options.SuccessResponseBuilder(successDetails, httpContext);
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Represents an HTTP result that maps a <see cref="Success{TValue}"/> instance and a value to a standard success response.
/// </summary>
/// <typeparam name="TValue">The type of the success value to be returned in the response.</typeparam>
public sealed class SuccessResult<TValue>(Success<TValue> success, string? contentType = null)
    : IResult
{
    /// <summary>
    /// Executes the response asynchronously, formatting the success result with the value and writing it to the HTTP context.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        SuccessMappingService mappingService =
            httpContext.RequestServices.GetRequiredService<SuccessMappingService>();
        EndpointResultOptions options = httpContext
            .RequestServices.GetRequiredService<IOptions<EndpointResultOptions>>()
            .Value;

        SuccessMapping? mapping = mappingService.GetMapping(success);
        int statusCode = (int)(mapping?.StatusCode ?? HttpStatusCode.OK);

        httpContext.Response.StatusCode = statusCode;

        if (statusCode == (int)HttpStatusCode.NoContent)
        {
            return Task.CompletedTask;
        }

        if (contentType is not null)
        {
            if (contentType.Contains("text/") && !contentType.Contains("charset"))
            {
                httpContext.Response.ContentType =
                    $"{contentType}; charset={options.DefaultCharset}";
            }
            else
            {
                httpContext.Response.ContentType = contentType;
            }
            return httpContext.Response.WriteAsync(success.Value?.ToString() ?? string.Empty);
        }

        if (!options.WrapSuccessResponses || options.SuccessResponseBuilder == null)
        {
            return httpContext.Response.WriteAsJsonAsync(success.Value);
        }

        SuccessDetail successDetails = new()
        {
            Status = mapping?.StatusCode ?? HttpStatusCode.OK,
            Message = success.Message ?? "",
            Instance = httpContext.Request.Path,
            Data = success.Value,
        };

        object? response = options.SuccessResponseBuilder(successDetails, httpContext);
        return httpContext.Response.WriteAsJsonAsync(response);
    }
}
