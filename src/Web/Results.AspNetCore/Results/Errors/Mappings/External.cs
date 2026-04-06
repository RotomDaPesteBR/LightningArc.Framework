
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to external services.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.External.RateLimitExceededError"/>,
    /// <see cref="Error.External.ApiQuotaExceededError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapExternal(this ErrorMappingService service)
    {
        // EXTERNAL module (Prefix 8)
        service.Map<Error.External.RateLimitExceededError>(HttpStatusCode.TooManyRequests, LocalizationManager.GetErrorTitle("External_RateLimitExceeded"), "urn:api-errors:external-rate-limit");
        service.Map<Error.External.ApiQuotaExceededError>(HttpStatusCode.TooManyRequests, LocalizationManager.GetErrorTitle("External_ApiQuotaExceeded"), "urn:api-errors:external-quota-exceeded");
        service.Map<Error.External.InvalidApiResponseError>(HttpStatusCode.BadGateway, LocalizationManager.GetErrorTitle("External_InvalidApiResponse"), "urn:api-errors:external-invalid-api-response");
        service.Map<Error.External.ServiceUnavailableError>(HttpStatusCode.ServiceUnavailable, LocalizationManager.GetErrorTitle("External_ServiceUnavailable"), "urn:api-errors:external-service-unavailable");
        service.Map<Error.External.TimeoutError>(HttpStatusCode.GatewayTimeout, LocalizationManager.GetErrorTitle("External_Timeout"), "urn:api-errors:external-timeout");
        service.Map<Error.External.CommunicationError>(HttpStatusCode.BadGateway, LocalizationManager.GetErrorTitle("External_Communication"), "urn:api-errors:external-communication-failed");

        return service;
    }
}




