using LightningArc.Results;
using System.Net;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Represents a custom error mapping configuration.
/// </summary>
public sealed class CustomErrorMapping
{
    /// <summary>
    /// The type of the error to be mapped.
    /// </summary>
    public Type ErrorType { get; set; } = null!;

    /// <summary>
    /// The HTTP status code to be returned for this error.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// The problem details title for this error.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The problem details type URL for this error.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a custom success mapping configuration.
/// </summary>
public sealed class CustomSuccessMapping
{
    /// <summary>
    /// The type of the success to be mapped.
    /// </summary>
    public Type SuccessType { get; set; } = null!;

    /// <summary>
    /// The HTTP status code to be returned for this success.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// The title for the success.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Options class for configuring success and error mappings.
/// </summary>
public sealed class EndpointResultOptions
{
    /// <summary>
    /// A list to register custom error mappings.
    /// </summary>
    public List<CustomErrorMapping> ErrorMappings { get; } = [];

    /// <summary>
    /// A list to register custom success mappings.
    /// </summary>
    public List<CustomSuccessMapping> SuccessMappings { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether success responses should be wrapped in a standard object.
    /// If set to <c>true</c>, the <see cref="SuccessResponseBuilder"/> will be used to format the response.
    /// If set to <c>false</c>, the result value will be returned directly.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool WrapSuccessResponses { get; set; } = false;

    /// <summary>
    /// Gets or sets the function used to build the wrapped success response object.
    /// This function is only called if <see cref="WrapSuccessResponses"/> is <c>true</c>.
    /// </summary>
    public Func<SuccessDetail, HttpContext, object?>? SuccessResponseBuilder { get; set; } = null;

    /// <summary>
    /// Gets or sets the function used to build the error response object.
    /// If not provided, a default builder that creates an RFC 7807 <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> object will be used.
    /// </summary>
    public Func<Error, HttpContext, object>? ErrorResponseBuilder { get; set; }

    /// <summary>
    /// Gets or sets the default charset to be used for text-based responses.
    /// Defaults to "utf-8".
    /// </summary>
    public string DefaultCharset { get; set; } = "utf-8";
}




