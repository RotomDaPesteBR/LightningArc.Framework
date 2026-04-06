namespace LightningArc.Results.Exceptions;

/// <summary>
/// Provides extension methods for converting exceptions to <see cref="Error"/> objects.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Transforms the exception into an <see cref="Error"/> object using the <see cref="ExceptionMapper"/>.
    /// </summary>
    /// <param name="exception">The exception to transform.</param>
    /// <returns>The resulting <see cref="Error"/> object.</returns>
    public static Error FromException(this Exception exception)
    {
        return ExceptionMapper.Map(exception);
    }
}
