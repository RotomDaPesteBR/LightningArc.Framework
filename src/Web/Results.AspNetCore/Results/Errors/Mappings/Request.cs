using System.Net;
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to the HTTP request.
    /// </summary>
    public static ErrorMappingService MapRequest(this ErrorMappingService service)
    {
        // REQUEST module (Prefix 11)
        service.Map<Error.Request.InvalidRequestError>(
            HttpStatusCode.BadRequest,
            LocalizationManager.GetErrorTitle("Request_InvalidRequest"),
            "urn:api-errors:req-invalid"
        );
        service.Map<Error.Request.TooLargeRequestError>(
            HttpStatusCode.RequestEntityTooLarge,
            LocalizationManager.GetErrorTitle("Request_TooLargeRequest"),
            "urn:api-errors:req-too-large"
        );
        service.Map<Error.Request.TooManyRequestsError>(
            HttpStatusCode.TooManyRequests,
            LocalizationManager.GetErrorTitle("Request_TooManyRequests"),
            "urn:api-errors:req-too-many-requests"
        );
        service.Map<Error.Request.NotAcceptableError>(
            HttpStatusCode.NotAcceptable,
            LocalizationManager.GetErrorTitle("Request_NotAcceptable"),
            "urn:api-errors:req-not-acceptable"
        );
        service.Map<Error.Request.UnsupportedMediaTypeError>(
            HttpStatusCode.UnsupportedMediaType,
            LocalizationManager.GetErrorTitle("Request_UnsupportedMediaType"),
            "urn:api-errors:req-unsupported-media-type"
        );

        return service;
    }
}
