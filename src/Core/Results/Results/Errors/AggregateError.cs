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
    ///
    /// <para>
    /// <see cref="Errors"/> contains the immediate child errors as they were combined (not flattened).
    /// <see cref="FlattenedErrors"/> and the inherited <see cref="Error.Details"/> provide the fully flattened view of all leaf errors.
    /// </para>
    /// </remarks>
    public sealed class AggregateError : Error
    {
        /// <summary>
        /// Gets the immediate child errors as they were combined.
        /// <para>
        /// This is <b>not</b> flattened — nested <see cref="AggregateError"/> instances appear as single entries.
        /// For the fully flattened list of leaf errors, use <see cref="FlattenedErrors"/> or <see cref="Error.Details"/>.
        /// </para>
        /// </summary>
        public IReadOnlyList<Error> Errors { get; }

        /// <summary>
        /// Gets all leaf errors from the entire error tree, recursively flattened.
        /// <para>
        /// Unlike <see cref="Errors"/>, this recursively descends into nested <see cref="AggregateError"/> instances
        /// and returns only non-aggregate errors. Equivalent to <c>Flatten().Errors</c> but without allocation
        /// when the error tree is already flat.
        /// </para>
        /// </summary>
        public IReadOnlyList<Error> FlattenedErrors { get; }

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
            List<Error> errorList = [.. errors];
            Errors = new ReadOnlyCollection<Error>(errorList);
            FlattenedErrors = ComputeFlattenedErrors(errorList);
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
            List<Error> errorList = [.. errors];
            Errors = new ReadOnlyCollection<Error>(errorList);
            FlattenedErrors = ComputeFlattenedErrors(errorList);
        }

        /// <summary>
        /// Flattens the <see cref="AggregateError"/> instances into a single list of non-aggregate errors.
        /// </summary>
        /// <returns>A new <see cref="AggregateError"/> containing only non-aggregate errors, or <c>this</c> if already flat.</returns>
        public AggregateError Flatten()
        {
            if (Errors.All(e => e is not AggregateError))
                return this;

            var flattenedList = FlattenedErrors.ToList();
            return new AggregateError(CodePrefix, CodeSuffix, Message, flattenedList);
        }

        private static IReadOnlyList<Error> ComputeFlattenedErrors(IReadOnlyList<Error> errors)
        {
            if (errors.All(e => e is not AggregateError))
                return errors;

            return [.. FlattenErrors(errors)];
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

        /// <summary>
        /// Compares flattened errors as a multiset (order-independent).
        /// Two AggregateErrors are equal if they contain the same leaf errors regardless of order.
        /// </summary>
        protected override bool DetailsEqual(Error other)
        {
            if (other is not AggregateError otherAggregate)
                return base.DetailsEqual(other);

            var thisFlattened = FlattenedErrors;
            var otherFlattened = otherAggregate.FlattenedErrors;

            if (thisFlattened.Count != otherFlattened.Count)
                return false;

            // Use a dictionary to count occurrences (multiset comparison)
            var counts = new Dictionary<int, int>();

            foreach (var error in thisFlattened)
            {
                int key = error.Code;
                if (counts.TryGetValue(key, out int count))
                    counts[key] = count + 1;
                else
                    counts[key] = 1;
            }

            foreach (var error in otherFlattened)
            {
                int key = error.Code;
                if (!counts.TryGetValue(key, out int count) || count == 0)
                    return false;
                counts[key] = count - 1;
            }

            return true;
        }
    }
}
