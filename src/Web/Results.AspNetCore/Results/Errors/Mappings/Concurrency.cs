
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to concurrency.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Concurrency.ConflictError"/>,
    /// <see cref="Error.Concurrency.LockedError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapConcurrency(this ErrorMappingService service)
    {
        // CONCURRENCY module (Prefix 9)
        service.Map<Error.Concurrency.ConflictError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Concurrency_Conflict"), "urn:api-errors:concurrency-conflict");
        service.Map<Error.Concurrency.LockedError>(HttpStatusCode.Locked, LocalizationManager.GetErrorTitle("Concurrency_Locked"), "urn:api-errors:concurrency-locked");
        service.Map<Error.Concurrency.StaleDataError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Concurrency_StaleData"), "urn:api-errors:concurrency-stale-data");
        service.Map<Error.Concurrency.ResourceInUseError>(HttpStatusCode.Locked, LocalizationManager.GetErrorTitle("Concurrency_ResourceInUse"), "urn:api-errors:concurrency-resource-in-use");

        return service;
    }
}




