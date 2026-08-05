using System.Net;
using System.Reflection;
using LightningArc.Results.Successes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Service to map specific success types (<see cref="Success"/>) to
/// appropriate HTTP responses (status code and problem details).
/// </summary>
/// <remarks>
/// This service is responsible for maintaining a dictionary of mappings,
/// allowing the application to convert instances of <see cref="Success"/> into
/// standardized HTTP responses based on their type.
/// </remarks>
public class SuccessMappingService
{
    /// <summary>
    /// Stores the mappings from success types to HTTP response details.
    /// The key is the success <see cref="Type"/> and the value is the corresponding <see cref="SuccessMapping"/>.
    /// </summary>
    private readonly Dictionary<Type, SuccessMapping> _mappings = [];

    private readonly ILogger<SuccessMappingService> _logger;

    /// <summary>
    /// The constructor is the best place to define default mappings explicitly.
    /// Maps all library successes to logical HTTP codes and success titles.
    /// </summary>
    /// <param name="logger">The logging service to record mapping information.</param>
    /// <param name="options"></param>
    public SuccessMappingService(
        ILogger<SuccessMappingService> logger,
        IOptions<EndpointResultOptions> options
    )
    {
        _logger = logger;
        _logger.LogInformation("Starting HTTP success mapping for the API...");

        // Library success mappings
        // ------------------------
        Map<OkSuccess>(HttpStatusCode.OK, "OK");
        Map<CreatedSuccess>(HttpStatusCode.Created, "Created");
        Map<AcceptedSuccess>(HttpStatusCode.Accepted, "Accepted");
        Map<NoContentSuccess>(HttpStatusCode.NoContent, "No Content");

        Map(typeof(OkSuccess<>), HttpStatusCode.OK, "OK");
        Map(typeof(CreatedSuccess<>), HttpStatusCode.Created, "Created");
        Map(typeof(AcceptedSuccess<>), HttpStatusCode.Accepted, "Accepted");
        Map(typeof(NoContentSuccess<>), HttpStatusCode.NoContent, "No Content");

        if (options.Value.SuccessMappings.Count > 0)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Adding {Count} custom success mappings.",
                    options.Value.SuccessMappings.Count
                );
            }
            foreach (CustomSuccessMapping mapping in options.Value.SuccessMappings)
            {
                _mappings[mapping.SuccessType] = new SuccessMapping(
                    mapping.StatusCode,
                    mapping.Title
                );
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "HTTP success mapping completed. {Count} successes registered.",
                _mappings.Count
            );
        }
    }

    /// <summary>
    /// Adds or overrides a mapping for a specific success type.
    /// </summary>
    /// <typeparam name="TSuccess">The type of success to map.</typeparam>
    /// <param name="statusCode">The HTTP status code to be returned for this success.</param>
    /// <param name="title">The problem title for this success.</param>
    public void Map<TSuccess>(HttpStatusCode statusCode, string title)
        where TSuccess : Success
    {
        _mappings[typeof(TSuccess)] = new SuccessMapping(statusCode, title);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Mapped success {SuccessType} to Status {StatusCode} and Title '{Title}'.",
                typeof(TSuccess).Name,
                (int)statusCode,
                title
            );
        }
    }

    /// <summary>
    /// Adds or overrides a mapping for a specific success type.
    /// </summary>
    /// <param name="successTypeDefinition">The type of success to map.</param>
    /// <param name="statusCode">The HTTP status code to be returned for this success.</param>
    /// <param name="title">The problem title for this success.</param>
    private void Map(Type successTypeDefinition, HttpStatusCode statusCode, string title)
    {
        _mappings[successTypeDefinition] = new SuccessMapping(statusCode, title);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Mapped success {SuccessType} to Status {StatusCode} and Title '{Title}'.",
                successTypeDefinition.Name,
                (int)statusCode,
                title
            );
        }
    }

    /// <summary>
    /// Gets the HTTP mapping for a success.
    /// </summary>
    /// <param name="success">The success instance for which the mapping is desired.</param>
    /// <returns>
    /// A <see cref="SuccessMapping"/> if a mapping is found for the success type,
    /// otherwise returns <c>null</c>.
    /// </returns>
    public SuccessMapping? GetMapping(Success success)
    {
        Type successType = success.GetType();

        // If it's a constructed generic type (e.g. OkSuccess<User>),
        // try to look up its generic type definition (e.g. OkSuccess<>).
        if (successType.IsGenericType && !successType.IsGenericTypeDefinition)
        {
            Type genericDef = successType.GetGenericTypeDefinition();
            if (_mappings.TryGetValue(genericDef, out SuccessMapping? mapping))
            {
                return mapping;
            }
        }

        // Try exact type lookup
        if (_mappings.TryGetValue(successType, out SuccessMapping? exactMapping))
        {
            return exactMapping;
        }

        // Log action for unmapped successes
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning(
                "No HTTP mapping found for success type '{SuccessType}'. Returning default.",
                success.GetType().Name
            );
        }

        return null;
    }
}
