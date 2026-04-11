using LightningArc.Results.Exceptions;

namespace LightningArc.Results;

/// <summary>
/// Provides a fluent builder for aggregating multiple validation results without short-circuiting.
/// </summary>
public sealed class ResultAggregator
{
    private Error? _currentError;
    private bool _isFailure;

    internal ResultAggregator() { }

    /// <summary>
    /// Executes a validation check. If the check returns a failure or throws an exception,
    /// the error is aggregated into the final result.
    /// </summary>
    /// <param name="action">The validation check to execute.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public ResultAggregator Check(Func<Result> action)
    {
        try
        {
            Result result = action();
            if (result.IsFailure)
            {
                AppendError(result.Error);
            }
        }
        catch (Exception ex)
        {
            AppendError(ex.FromException());
        }

        return this;
    }

    /// <summary>
    /// Executes a validation check and captures the value if it succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value to capture.</typeparam>
    /// <param name="factory">The function that creates the value.</param>
    /// <param name="value">The captured value if successful; otherwise, default.</param>
    /// <returns>The current <see cref="ResultAggregator"/> instance.</returns>
    public ResultAggregator Check<T>(Func<T> factory, out T? value)
    {
        try
        {
            value = factory();
            return this;
        }
        catch (Exception ex)
        {
            AppendError(ex.FromException());
            value = default;
            return this;
        }
    }

    /// <summary>
    /// Builds the final <see cref="Result"/> based on the aggregated errors.
    /// </summary>
    /// <returns>A success <see cref="Result"/> if no errors occurred; otherwise, a failure <see cref="Result"/>.</returns>
    public Result Build()
    {
        return _isFailure ? Result.Failure(_currentError!) : Result.Success();
    }

    private void AppendError(Error error)
    {
        _isFailure = true;
        _currentError = (_currentError == null) ? error : _currentError + error;
    }
}
