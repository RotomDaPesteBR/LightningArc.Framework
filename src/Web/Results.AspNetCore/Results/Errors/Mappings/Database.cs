
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to the database.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Database.ConnectionFailedError"/>,
    /// <see cref="Error.Database.QueryExecutionFailedError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapDatabase(this ErrorMappingService service)
    {
        // DATABASE module
        service.Map<Error.Database.ConnectionFailedError>(HttpStatusCode.InternalServerError, LocalizationManager.GetErrorTitle("Database_ConnectionFailed"), "urn:api-errors:db-connection-failed");
        service.Map<Error.Database.QueryExecutionFailedError>(HttpStatusCode.InternalServerError, LocalizationManager.GetErrorTitle("Database_QueryExecutionFailed"), "urn:api-errors:db-query-failed");
        service.Map<Error.Database.ConstraintViolationError>(HttpStatusCode.Conflict, LocalizationManager.GetErrorTitle("Database_ConstraintViolation"), "urn:api-errors:db-constraint-violation");
        service.Map<Error.Database.TransientError>(HttpStatusCode.ServiceUnavailable, LocalizationManager.GetErrorTitle("Database_Transient"), "urn:api-errors:db-transient-error");
        service.Map<Error.Database.DeadlockError>(HttpStatusCode.ServiceUnavailable, LocalizationManager.GetErrorTitle("Database_Deadlock"), "urn:api-errors:db-deadlock");

        return service;
    }
}




