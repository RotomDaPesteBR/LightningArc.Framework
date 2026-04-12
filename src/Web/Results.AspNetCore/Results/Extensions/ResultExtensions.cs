using LightningArc.Results;

namespace LightningArc.Results.AspNetCore
{
    /// <summary>
    /// Provides extension methods for the <see cref="Result{TValue}"/> class,
    /// allowing configuration of the content type (Content-Type) for the HTTP response.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Creates an <see cref="EndpointResult"/> from a <see cref="Result"/>.
        /// </summary>
        /// <param name="result">The <see cref="Result"/> instance to be converted.</param>
        /// <returns>
        /// An <see cref="EndpointResult"/> that encapsulates the original result.
        /// </returns>
        public static EndpointResult ToEndpointResult(
            this Result result
        ) => result;

        /// <summary>
        /// Creates an <see cref="EndpointResult{TValue}"/> from a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the success value contained in the result.</typeparam>
        /// <param name="result">The <see cref="Result{TValue}"/> instance to be converted.</param>
        /// <returns>
        /// An <see cref="EndpointResult{TValue}"/> that encapsulates the original result.
        /// </returns>
        public static EndpointResult<TValue> ToEndpointResult<TValue>(
            this Result<TValue> result
        ) => EndpointResult<TValue>.FromResult(result, null);

        /// <summary>
        /// Creates an <see cref="EndpointResult{TValue}"/> from a <see cref="Result{TValue}"/>,
        /// specifying the content type for the HTTP response serialization.
        /// </summary>
        /// <typeparam name="TValue">The type of the success value contained in the result.</typeparam>
        /// <param name="result">The <see cref="Result{TValue}"/> instance to be converted.</param>
        /// <param name="contentType">The content type (e.g., "text/plain") to be used in the HTTP response.</param>
        /// <returns>
        /// An <see cref="EndpointResult{TValue}"/> that encapsulates the original result
        /// and the content type information.
        /// </returns>
        public static EndpointResult<TValue> WithContentType<TValue>(
            this Result<TValue> result,
            string contentType
        ) => EndpointResult<TValue>.FromResult(result, contentType);
    }
}




