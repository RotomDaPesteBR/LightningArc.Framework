using System.Collections.Generic;
using LightningArc.Results.Exceptions;

namespace LightningArc.Results;

/// <summary>
/// Provides a fluent builder for aggregating multiple validation results without short-circuiting.
/// </summary>
public sealed class ResultAggregator
{
    private readonly List<Error> _errors = [];

    internal ResultAggregator() { }

    /// <summary>
    /// Appends an error to the aggregator's error collection.
    /// </summary>
    /// <param name="error">The error to append.</param>
    internal void AppendError(Error error) => _errors.Add(error);

    /// <summary>
    /// Builds the final <see cref="Result"/> from all aggregated checks.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> if no errors were aggregated; otherwise, a failed <see cref="Result"/>.</returns>
    public Result Build()
    {
        if (_errors.Count == 0)
            return Result.Success();

        Error combined = Error.Aggregate(_errors)!;
        return Result.Failure(combined);
    }
}
