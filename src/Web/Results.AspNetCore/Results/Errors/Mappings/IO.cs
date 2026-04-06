
using LightningArc.Results;
using LightningArc.Results.AspNetCore.Localization;
using System.Net;

namespace LightningArc.Results.AspNetCore;

public static partial class ErrorMappingServiceExtensions
{
    /// <summary>
    /// Adds error mappings related to I/O (Input/Output) operations.
    /// </summary>
    /// <remarks>
    /// Includes errors such as <see cref="Error.IO.FileNotFoundError"/>,
    /// <see cref="Error.IO.PermissionDeniedError"/>, among others.
    /// </remarks>
    /// <param name="service">The error mapping service instance.</param>
    /// <returns>The service instance for call chaining.</returns>
    public static ErrorMappingService MapIO(this ErrorMappingService service)
    {
        // I/O module
        service.Map<Error.IO.FileNotFoundError>(HttpStatusCode.NotFound, LocalizationManager.GetErrorTitle("IO_FileNotFound"), "urn:api-errors:io-file-not-found");
        service.Map<Error.IO.DirectoryNotFoundError>(HttpStatusCode.NotFound, LocalizationManager.GetErrorTitle("IO_DirectoryNotFound"), "urn:api-errors:io-directory-not-found");
        service.Map<Error.IO.PermissionDeniedError>(HttpStatusCode.InternalServerError, LocalizationManager.GetErrorTitle("IO_PermissionDenied"), "urn:api-errors:io-permission-denied");
        service.Map<Error.IO.DiskFullError>(HttpStatusCode.InsufficientStorage, LocalizationManager.GetErrorTitle("IO_DiskFull"), "urn:api-errors:io-disk-full");
        service.Map<Error.IO.CorruptedFileError>(HttpStatusCode.InternalServerError, LocalizationManager.GetErrorTitle("IO_CorruptedFile"), "urn:api-errors:io-corrupted-file");

        return service;
    }
}




