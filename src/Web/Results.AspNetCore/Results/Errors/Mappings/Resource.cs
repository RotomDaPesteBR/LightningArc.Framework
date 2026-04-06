
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to resources.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Resource.NotFoundError"/>,
    /// <see cref="Error.Resource.AlreadyExistsError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapResource(this ErrorMappingService service)
    {
        // RESOURCE module (Prefix 3)
        service.Map<Error.Resource.NotFoundError>(HttpStatusCode.NotFound, LocalizationManager.GetErrorTitle("Resource_NotFound"), "urn:api-errors:resource-not-found");
        service.Map<Error.Resource.AlreadyExistsError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Resource_AlreadyExists"), "urn:api-errors:resource-already-exists");
        service.Map<Error.Resource.UnavailableError>(HttpStatusCode.ServiceUnavailable, LocalizationManager.GetErrorTitle("Resource_Unavailable"), "urn:api-errors:resource-unavailable");
        service.Map<Error.Resource.InvalidStateError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Resource_InvalidState"), "urn:api-errors:resource-invalid-state");
        service.Map<Error.Resource.ObsoleteError>(HttpStatusCode.Gone, LocalizationManager.GetErrorTitle("Resource_Obsolete"), "urn:api-errors:resource-obsolete");


        return service;
    }
}




