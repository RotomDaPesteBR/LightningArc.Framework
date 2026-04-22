using System.Reflection;
using LightningArc.Results;

namespace LightningArc.Validations.Internal;

internal static class ErrorPrefixing
{
    private static readonly MethodInfo MemberwiseCloneMethod = typeof(object).GetMethod(
        "MemberwiseClone",
        BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo DetailsBackingField = typeof(Error).GetField(
        "<Details>k__BackingField",
        BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static Error Prefix(Error error, string path)
    {
        if (error is AggregateError aggregateError)
        {
            return aggregateError.Errors
                .Select(inner => Prefix(inner, path))
                .Aggregate((left, right) => left + right);
        }

        if (error.Details.Count == 0)
        {
            return error;
        }

        ErrorDetail[] prefixedDetails = error.Details
            .Select(detail => new ErrorDetail(
                string.IsNullOrWhiteSpace(detail.Context) ? path : $"{path}.{detail.Context}",
                detail.Message))
            .ToArray();

        return CloneErrorWithDetails(error, prefixedDetails);
    }

    private static Error CloneErrorWithDetails(Error error, ErrorDetail[] prefixedDetails)
    {
        Error clone = (Error)MemberwiseCloneMethod.Invoke(error, null)!;
        DetailsBackingField.SetValue(clone, prefixedDetails);
        return clone;
    }
}
