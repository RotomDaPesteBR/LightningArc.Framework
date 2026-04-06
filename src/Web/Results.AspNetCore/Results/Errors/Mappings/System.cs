using System.Net;
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to the system.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.System.ConfigurationError"/>,
    /// <see cref="Error.System.DependencyNotRegisteredError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapSystem(this ErrorMappingService service)
    {
        // SYSTEM module (Prefix 10)
        service.Map<Error.System.ConfigurationError>(
            HttpStatusCode.InternalServerError,
            LocalizationManager.GetErrorTitle("System_Configuration"),
            "urn:api-errors:sys-configuration-error"
        );
        service.Map<Error.System.DependencyNotRegisteredError>(
            HttpStatusCode.InternalServerError,
            LocalizationManager.GetErrorTitle("System_DependencyNotRegistered"),
            "urn:api-errors:sys-dependency-not-registered"
        );
        service.Map<Error.System.SystemMaintenanceError>(
            HttpStatusCode.ServiceUnavailable,
            LocalizationManager.GetErrorTitle("System_SystemMaintenance"),
            "urn:api-errors:sys-maintenance"
        );
        service.Map<Error.System.OutOfMemoryError>(
            HttpStatusCode.InternalServerError,
            LocalizationManager.GetErrorTitle("System_OutOfMemory"),
            "urn:api-errors:sys-out-of-memory"
        );
        service.Map<Error.System.ThreadAbortedError>(
            HttpStatusCode.InternalServerError,
            LocalizationManager.GetErrorTitle("System_ThreadAborted"),
            "urn:api-errors:sys-thread-aborted"
        );

        return service;
    }
}
