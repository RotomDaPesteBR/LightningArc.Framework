using LightningArc.Results;
using System.Net;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Provides a fluent API for configuring custom success mappings.
/// </summary>
/// <param name="options">The options object where mappings will be registered.</param>
public sealed class SuccessMappingConfigurator(EndpointResultOptions options)
{
    /// <summary>
    /// Maps a success type to an HTTP response.
    /// </summary>
    /// <typeparam name="TSuccess">The type of success to map.</typeparam>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The success title.</param>
    public void Map<TSuccess>(HttpStatusCode statusCode, string title)
        where TSuccess : Success => options.SuccessMappings.Add(new CustomSuccessMapping
        {
            SuccessType = typeof(TSuccess),
            StatusCode = statusCode,
            Title = title
        });
}

/// <summary>
/// Provides a fluent API for configuring custom error mappings.
/// </summary>
/// <param name="options">The options object where mappings will be registered.</param>
public sealed class ErrorMappingConfigurator(EndpointResultOptions options)
{
    /// <summary>
    /// Maps an error type to an HTTP response.
    /// </summary>
    /// <typeparam name="TError">The type of error to map.</typeparam>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The problem title.</param>
    /// <param name="type">The problem type URL.</param>
    public void Map<TError>(HttpStatusCode statusCode, string title, string type)
        where TError : Error => options.ErrorMappings.Add(new CustomErrorMapping
        {
            ErrorType = typeof(TError),
            StatusCode = statusCode,
            Title = title,
            Type = type
        });
}




