
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to authentication.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Authentication.UnauthorizedError"/>,
    /// <see cref="Error.Authentication.ForbiddenError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapAuthentication(this ErrorMappingService service)
    {
        // AUTHENTICATION module (Prefix 4)
        service.Map<Error.Authentication.UnauthorizedError>(HttpStatusCode.Unauthorized, LocalizationManager.GetErrorTitle("Authentication_Unauthorized"), "urn:api-errors:auth-failed");
        service.Map<Error.Authentication.ForbiddenError>(HttpStatusCode.Forbidden, LocalizationManager.GetErrorTitle("Authentication_Forbidden"), "urn:api-errors:auth-forbidden");
        service.Map<Error.Authentication.TokenExpiredError>(HttpStatusCode.Unauthorized, LocalizationManager.GetErrorTitle("Authentication_TokenExpired"), "urn:api-errors:auth-token-expired");
        service.Map<Error.Authentication.InvalidCredentialsError>(HttpStatusCode.Unauthorized, LocalizationManager.GetErrorTitle("Authentication_InvalidCredentials"), "urn:api-errors:auth-invalid-credentials");
        service.Map<Error.Authentication.InactiveAccountError>(HttpStatusCode.Forbidden, LocalizationManager.GetErrorTitle("Authentication_InactiveAccount"), "urn:api-errors:auth-inactive-account");
        service.Map<Error.Authentication.ExpiredSessionError>(HttpStatusCode.Unauthorized, LocalizationManager.GetErrorTitle("Authentication_ExpiredSession"), "urn:api-errors:auth-session-expired");


        return service;
    }
}




