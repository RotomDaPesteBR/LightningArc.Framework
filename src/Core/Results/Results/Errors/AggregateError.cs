using System.Collections.ObjectModel;
using LightningArc.Results.Messages;

namespace LightningArc.Results
{
    /// <summary>
    /// Represents a group of one or more errors that occurred during an operation.
    /// </summary>
    /// <remarks>
    /// This class is useful for aggregating multiple errors, such as those from multiple validation rules,
    /// into a single error object. It provides a structure similar to <see cref="AggregateException"/>.
    /// </remarks>
    public sealed class AggregateError : Error
    {
        /// <summary>
        /// Gets a read-only collection of the <see cref="Error"/> instances that caused the current error.
        /// </summary>
        public ReadOnlyCollection<Error> Errors { get; }

        internal AggregateError(
            int codePrefix,
            int codeSuffix,
            IMessageProvider messageProvider,
            IEnumerable<Error> errors
        )
            : base(
                codePrefix,
                codeSuffix,
                messageProvider,
                FlattenErrors(errors).SelectMany(e => e.Details)
            )
        {
            Errors = new ReadOnlyCollection<Error>([.. errors]);
        }

        internal AggregateError(
            int codePrefix,
            int codeSuffix,
            string message,
            IEnumerable<Error> errors
        )
            : base(
                codePrefix,
                codeSuffix,
                message,
                FlattenErrors(errors).SelectMany(e => e.Details)
            )
        {
            Errors = new ReadOnlyCollection<Error>([.. errors]);
        }

        /// <summary>
        /// Flattens the <see cref="AggregateError"/> instances into a single list of non-aggregate errors.
        /// </summary>
        /// <returns>A new <see cref="AggregateError"/> containing only non-aggregate errors.</returns>
        public AggregateError Flatten()
        {
            var flattenedList = FlattenErrors(Errors).ToList();
            return new AggregateError(CodePrefix, CodeSuffix, Message, flattenedList);
        }

        private static IEnumerable<Error> FlattenErrors(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
                if (error is AggregateError aggregate)
                {
                    foreach (Error inner in FlattenErrors(aggregate.Errors))
                    {
                        yield return inner;
                    }
                }
                else
                {
                    yield return error;
                }
            }
        }
    }
}
