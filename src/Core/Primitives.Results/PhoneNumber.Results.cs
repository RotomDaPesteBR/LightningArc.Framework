using LightningArc.Results;

namespace LightningArc.Primitives.Results
{
    /// <summary>
    /// Extension methods that integrate <see cref="PhoneNumber"/> with the Result pattern.
    /// </summary>
    public static class PhoneNumberExtensions
    {
        /// <summary>
        /// Wraps the phone number in a success <see cref="Result{TValue}"/>.
        /// </summary>
        public static Result<PhoneNumber> ToResult(this PhoneNumber phone) => Result.Success(phone);

        /// <summary>
        /// Attempts to create a <see cref="PhoneNumber"/> and returns it as a Result.
        /// </summary>
        public static Result<PhoneNumber> CreatePhoneNumberResult(this string value)
        {
            if (PhoneNumber.TryCreate(value, out var result))
            {
                return Result.Success(result!);
            }

            return Error.Validation($"'{value}' is not a valid phone number.", nameof(value));
        }
    }
}
