using LightningArc.Results;

namespace LightningArc.Validations.Internal;

internal sealed record ValidationFailure(
    string Path,
    string Message,
    Func<string, string, Error> ErrorFactory)
{
    public ValidationFailure Prefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return this;
        }

        string path = string.IsNullOrWhiteSpace(Path) ? prefix : $"{prefix}.{Path}";
        return this with { Path = path };
    }

    public Error ToError() => ErrorFactory(Path, Message);
}
