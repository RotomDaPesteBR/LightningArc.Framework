using System.Globalization;
using LightningArc.Results.Messages;

namespace LightningArc.Results
{
    /// <summary>
    /// Represents a standardized error object for the application.
    /// </summary>
    /// <remarks>
    /// This base class is used in conjunction with the Result pattern to provide
    /// clear information about operation failures. Errors can be mapped to HTTP
    /// status codes in the application's presentation layer.
    /// <para>
    /// The error code is composed of a prefix (category) and a suffix (specific error).
    /// For example: 1001 (prefix 10 for "Application", suffix 01 for "Internal").
    /// </para>
    /// </remarks>
    public partial class Error : IEquatable<Error>
    {
        private readonly IMessageProvider _messageProvider;

        /// <summary>
        /// Gets the full error code, composed of a category prefix and a specific suffix.
        /// </summary>
        public int Code => CodePrefix * 1000 + CodeSuffix;

        /// <summary>
        /// Gets the error code prefix, representing the error category (e.g., 10 for Application).
        /// </summary>
        private protected int CodePrefix { get; }

        /// <summary>
        /// Gets the error code suffix, representing the specific error within a category (e.g., 01 for Internal Error).
        /// </summary>
        private protected int CodeSuffix { get; }

        /// <summary>
        /// Gets the descriptive message for the error.
        /// This message is either a literal string or a localized string retrieved using a resource key.
        /// </summary>
        public string Message => _messageProvider.GetMessage(CultureInfo.CurrentCulture);

        /// <summary>
        /// Gets a list of additional error details, useful for validations with multiple issues.
        /// </summary>
        public IReadOnlyList<ErrorDetail> Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class with a literal message.
        /// </summary>
        /// <param name="codePrefix">The error code prefix, representing the category.</param>
        /// <param name="codeSuffix">The error code suffix, representing the specific error.</param>
        /// <param name="messageProvider">The message provider for the error.</param>
        /// <param name="details">Additional error details.</param>
        protected Error(
            int codePrefix,
            int codeSuffix,
            IMessageProvider messageProvider,
            IEnumerable<ErrorDetail>? details = null
        )
        {
            if (codePrefix <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(codePrefix),
                    "Code prefix must be a positive integer."
                );
            }

            if (codeSuffix <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(codeSuffix),
                    "Code suffix must be a positive integer."
                );
            }

            CodePrefix = codePrefix;
            CodeSuffix = codeSuffix;
            _messageProvider = messageProvider;
            Details = details?.ToList() ?? [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class with a literal message.
        /// </summary>
        /// <param name="codePrefix">The error code prefix, representing the category.</param>
        /// <param name="codeSuffix">The error code suffix, representing the specific error.</param>
        /// <param name="message">The message provider for the error.</param>
        /// <param name="details">Additional error details.</param>
        protected Error(
            int codePrefix,
            int codeSuffix,
            string message,
            IEnumerable<ErrorDetail>? details = null
        )
        {
            if (codePrefix <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(codePrefix),
                    "Code prefix must be a positive integer."
                );
            }

            if (codeSuffix <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(codeSuffix),
                    "Code suffix must be a positive integer."
                );
            }

            CodePrefix = codePrefix;
            CodeSuffix = codeSuffix;
            _messageProvider = new LiteralMessageProvider(message);
            Details = details?.ToList() ?? [];
        }

        /// <inheritdoc />
        public virtual bool Equals(Error? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return CodePrefix == other.CodePrefix
                && CodeSuffix == other.CodeSuffix
                && Details.SequenceEqual(other.Details);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Error other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NETSTANDARD2_0
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + CodePrefix.GetHashCode();
                hash = hash * 23 + CodeSuffix.GetHashCode();
                foreach (ErrorDetail detail in Details)
                {
                    hash = hash * 23 + detail.GetHashCode();
                }
                return hash;
            }
#else
            HashCode hash = new();
            hash.Add(CodePrefix);
            hash.Add(CodeSuffix);
            foreach (ErrorDetail detail in Details)
            {
                hash.Add(detail);
            }
            return hash.ToHashCode();
#endif
        }

        /// <summary>
        /// Deconstructs the error into its code and message.
        /// </summary>
        /// <param name="code">The full error code.</param>
        /// <param name="message">The descriptive message.</param>
        public void Deconstruct(out int code, out string message)
        {
            code = Code;
            message = Message;
        }

        /// <summary>
        /// Deconstructs the error into its code, message, and details.
        /// </summary>
        /// <param name="code">The full error code.</param>
        /// <param name="message">The descriptive message.</param>
        /// <param name="details">The list of additional error details.</param>
        public void Deconstruct(
            out int code,
            out string message,
            out IReadOnlyList<ErrorDetail> details
        )
        {
            code = Code;
            message = Message;
            details = Details;
        }

        /// <summary>
        /// Combines two errors into an <see cref="AggregateError"/>.
        /// If both errors have the same code, the result maintains that code.
        /// If codes are different, a general aggregate code (99000) is used.
        /// </summary>
        /// <param name="left">The first error to combine.</param>
        /// <param name="right">The second error to combine.</param>
        /// <returns>An <see cref="AggregateError"/> containing both errors.</returns>
        public static Error operator +(Error left, Error right)
        {
            var errors = new List<Error> { left, right };

            return left.Code == right.Code
                ? new AggregateError(
                    left.CodePrefix,
                    left.CodeSuffix,
                    left._messageProvider,
                    errors
                )
                : new AggregateError(
                    (int)ModuleCodes.General,
                    1,
                    "Multiple errors occurred.",
                    errors
                );
        }

        /// <summary>
        /// Checks if two <see cref="Error"/> instances are equal.
        /// </summary>
        public static bool operator ==(Error? left, Error? right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if two <see cref="Error"/> instances are different.
        /// </summary>
        public static bool operator !=(Error? left, Error? right)
        {
            return !(left == right);
        }
    }
}
