using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="IpAddress"/> with the Result pattern.
    /// </summary>
    public static class IpAddressResultsExtensions
    {
        /// <summary>
        /// Validates a string and returns an <see cref="IpAddress"/> encapsulated in a <see cref="Result{TValue}"/>.
        /// </summary>
        /// <param name="value">The IP address string.</param>
        /// <returns>A successful result containing the IpAddress, or a validation error.</returns>
        public static Result<IpAddress> AsIpAddress(this string value)
        {
            if (IpAddress.IsValid(value, out string? errorMessage))
            {
                return new IpAddress(value);
            }

            return Error.Validation.InvalidParameter(errorMessage!);
        }
    }
}
