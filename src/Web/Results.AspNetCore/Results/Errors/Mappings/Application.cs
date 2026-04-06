
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to the application domain.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Application.InternalError"/>,
    /// <see cref="Error.Application.InvalidParameterError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapApplication(this ErrorMappingService service)
    {
        // APPLICATION module (Prefix 1)
        service.Map<Error.Application.InternalError>(HttpStatusCode.InternalServerError, LocalizationManager.GetErrorTitle("Application_Internal"), "urn:api-errors:internal");
        service.Map<Error.Application.InvalidParameterError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Application_InvalidParameter"), "urn:api-errors:invalid-parameter");
        service.Map<Error.Application.InvalidOperationError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Application_InvalidOperation"), "urn:api-errors:invalid-operation");
        service.Map<Error.Application.TaskCanceledError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Application_TaskCanceled"), "urn:api-errors:task-canceled");
        service.Map<Error.Application.NotImplementedError>(HttpStatusCode.NotImplemented, LocalizationManager.GetErrorTitle("Application_NotImplemented"), "urn:api-errors:not-implemented");

        return service;
    }
}




