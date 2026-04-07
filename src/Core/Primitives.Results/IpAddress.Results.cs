using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="IpAddress"/> with the Result pattern.
    /// </summary>
    public static class IpAddressExtensions
    {
        /// <summary>
        /// Wraps the IP address in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<IpAddress> ToResult(this IpAddress ipAddress) => Result.Success(ipAddress);

        /// <summary>
        /// Attempts to create an <see cref="IpAddress"/> and returns it as a Result.
        /// </summary>
        public static Result<IpAddress> CreateIpAddressResult(this string value)
        {
            if (IpAddress.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation.InvalidParameter($"'{value}' is not a valid IP address.");
        }
    }
}
