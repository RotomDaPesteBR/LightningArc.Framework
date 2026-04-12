using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to data and formats.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.Validation.InvalidFormatError"/>,
    /// <see cref="Error.Validation.InvalidSchemaError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapValidation(this ErrorMappingService service)
    {
        // VALIDATION module
        service.Map<Error.Validation.InvalidFormatError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_InvalidFormat"), "urn:api-errors:data-invalid-format");
        service.Map<Error.Validation.InvalidSchemaError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_InvalidSchema"), "urn:api-errors:data-invalid-schema");
        service.Map<Error.Validation.DeserializationFailedError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_DeserializationFailed"), "urn:api-errors:data-deserialization-failed");
        service.Map<Error.Validation.MissingFieldError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_MissingField"), "urn:api-errors:data-missing-field");
        service.Map<Error.Validation.ValueOutOfRangeError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_ValueOutOfRange"), "urn:api-errors:data-out-of-range");
        service.Map<Error.Validation.InvalidParameterError>(HttpStatusCode.BadRequest, LocalizationManager.GetErrorTitle("Validation_InvalidParameter"), "urn:api-errors:invalid-parameter");

        return service;
    }
}




