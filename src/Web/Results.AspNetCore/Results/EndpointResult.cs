using LightningArc.Results;

namespace LightningArc.Results.AspNetCore;

/// <summary>
/// Represents an endpoint result that maps a <see cref="Result"/>
/// to an appropriate HTTP response.
/// </summary>
/// <remarks>
/// This class acts as an adapter between the business logic that returns a <see cref="Result"/>
/// and the presentation layer (API) that needs to produce an HTTP response.
/// It implements <see cref="IResult"/> from ASP.NET Core to be directly returnable from endpoints.
/// </remarks>
public sealed class EndpointResult : IResult
{
    private readonly IResult _result;

    /// <summary>
    /// Private constructor to create a new instance of <see cref="EndpointResult"/>.
    /// </summary>
    /// <param name="result">The internal <see cref="IResult"/> that will be executed.</param>
    private EndpointResult(IResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Executes the HTTP result asynchronously, writing the response to the <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    /// <summary>
    /// Allows implicit conversion from a <see cref="Result"/> to an <see cref="EndpointResult"/>.
    /// </summary>
    /// <param name="result">The <see cref="Result"/> to be converted.</param>
    /// <returns>An <see cref="EndpointResult"/> that encapsulates the corresponding HTTP response.</returns>
    public static implicit operator EndpointResult(Result result)
    {
        return result.IsSuccess ? new EndpointResult(new SuccessResult(result.SuccessDetails)) : new EndpointResult(new ErrorResult(result.Error));
    }
}

/// <summary>
/// Represents a generic endpoint result that maps a <see cref="Result{TValue}"/>
/// to an appropriate HTTP response, including the status code and the body.
/// </summary>
/// <typeparam name="TValue">The type of the success value contained in the <see cref="Result{TValue}"/>.</typeparam>
/// <remarks>
/// This class acts as an adapter between the business logic that returns a <see cref="Result{TValue}"/>
/// and the presentation layer (API) that needs to produce an HTTP response.
/// It implements <see cref="IResult"/> from ASP.NET Core to be directly returnable from endpoints.
/// </remarks>
public sealed class EndpointResult<TValue> : IResult
{
    private readonly IResult _result;

    /// <summary>
    /// Private constructor to create a new instance of <see cref="EndpointResult{TValue}"/>.
    /// </summary>
    /// <param name="result">The internal <see cref="IResult"/> that will be executed.</param>
    private EndpointResult(IResult result)
    {
        _result = result;
    }

    /// <summary>
    /// Executes the HTTP result asynchronously, writing the response to the <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    /// <summary>
    /// Creates an EndpointResult from a Result with a specific content type.
    /// </summary>
    /// <param name="result">The success or error result.</param>
    /// <param name="contentType">The desired content type (e.g., "text/plain").</param>
    /// <returns>A new EndpointResult instance.</returns>
    public static EndpointResult<TValue> FromResult(Result<TValue> result, string? contentType)
    {
        return result.IsSuccess
            ? new EndpointResult<TValue>(
                new SuccessResult<TValue>(result.SuccessDetails, contentType)
            )
            : new EndpointResult<TValue>(new ErrorResult(result.Error!));
    }

    /// <summary>
    /// Allows implicit conversion from a <see cref="Result{TValue}"/> to an <see cref="EndpointResult{TValue}"/>.
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue}"/> to be converted.</param>
    /// <returns>An <see cref="EndpointResult{TValue}"/> that encapsulates the corresponding HTTP response.</returns>
    public static implicit operator EndpointResult<TValue>(Result<TValue> result)
    {
        return result.IsSuccess ? new EndpointResult<TValue>(new SuccessResult<TValue>(result.SuccessDetails)) : new EndpointResult<TValue>(new ErrorResult(result.Error));
    }

    /// <summary>
    /// Allows implicit conversion from a TValue to an <see cref="EndpointResult{TValue}"/>.
    /// </summary>
    /// <param name="value">The TValue to be converted.</param>
    /// <returns>An <see cref="EndpointResult{TValue}"/> that encapsulates the corresponding HTTP response.</returns>
    public static implicit operator EndpointResult<TValue>(TValue value) =>
        new(new SuccessResult<TValue>(Success.Ok(value)));

    /// <summary>
    /// Allows implicit conversion from an <see cref="Error"/> into an <see cref="EndpointResult{TValue}"/>.
    /// </summary>
    /// <param name="error">The <see cref="Error"/> to be converted.</param>
    /// <returns>An <see cref="EndpointResult{TValue}"/> that encapsulates the corresponding HTTP response.</returns>
    public static implicit operator EndpointResult<TValue>(Error error) =>
        new(new ErrorResult(error));
}




