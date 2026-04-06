using Microsoft.CodeAnalysis;

namespace LightningArc.Analyzers;

/// <summary>
/// Constants for analyzer diagnostic categories and tags.
/// </summary>
public static class DiagnosticCategory
{
    public const string Category = "LightningArc";

    public static readonly string[] EditAndContinueTags =
    [
        WellKnownDiagnosticTags.EditAndContinue
    ];
}
