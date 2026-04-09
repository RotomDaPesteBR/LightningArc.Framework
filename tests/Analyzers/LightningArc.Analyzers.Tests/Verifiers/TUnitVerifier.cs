using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace LightningArc.Analyzers.Tests.Verifiers;

/// <summary>
/// A verbose synchronous verifier implementation for Roslyn's IVerifier interface.
/// Provides detailed diagnostic information on failure.
/// </summary>
public class TUnitVerifier : IVerifier
{
    public void Empty<T>(string collectionName, IEnumerable<T> collection)
    {
        if (!collection.Any())
            return;

        StringBuilder sb = new();
        sb.AppendLine(
            $"Collection '{collectionName}' was expected to be empty but contained {collection.Count()} items."
        );
        AppendCollectionItems(sb, collection);
        Fail(sb.ToString());
    }

    public void Equal<T>(T expected, T actual, string? message = null)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
            return;

        StringBuilder sb = new();
        sb.AppendLine(message ?? "Objects are not equal.");
        sb.AppendLine($"  Expected: {expected}");
        sb.AppendLine($"  Actual:   {actual}");

        // If actual is a diagnostic or contains diagnostics, try to format them
        if (actual is IEnumerable collection)
        {
            AppendCollectionItems(sb, collection);
        }

        Fail(sb.ToString());
    }

    public void False(bool condition, string? message = null)
    {
        if (condition)
        {
            Fail(message ?? "Condition expected to be false but was true.");
        }
    }

    public void NotEmpty<T>(string collectionName, IEnumerable<T> collection)
    {
        if (!collection.Any())
        {
            Fail($"Collection '{collectionName}' was expected to be not empty.");
        }
    }

    public void SequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        IEqualityComparer<T>? comparer = null,
        string? message = null
    )
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        if (expectedList.Count != actualList.Count)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{message ?? "Sequences are not equal"}.");
            sb.AppendLine($"  Expected count: {expectedList.Count}");
            sb.AppendLine($"  Actual count:   {actualList.Count}");
            sb.AppendLine("\nActual items found:");
            AppendCollectionItems(sb, actualList);
            Fail(sb.ToString());
        }

        var effectiveComparer = comparer ?? EqualityComparer<T>.Default;
        for (int i = 0; i < expectedList.Count; i++)
        {
            if (!effectiveComparer.Equals(expectedList[i], actualList[i]))
            {
                Fail(
                    $"{message ?? "Sequences differ at index " + i}.\nExpected: {expectedList[i]}\nActual: {actualList[i]}"
                );
            }
        }
    }

    public void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            Fail(message ?? "Condition expected to be true but was false.");
        }
    }

    [DoesNotReturn]
    public void Fail(string? message = null)
    {
        // TUnit will capture this exception and show the detailed message
        throw new AnalyzerAssertionException(message ?? "Assertion failed");
    }

    public void LanguageIsSupported(string language)
    {
        if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
        {
            Fail($"Language '{language}' is not supported.");
        }
    }

    public IVerifier PushContext(string context)
    {
        // You could potentially store context here to improve error messages further
        return this;
    }

    private static void AppendCollectionItems(StringBuilder sb, IEnumerable collection)
    {
        sb.AppendLine("\n--- Items in Collection ---");
        int i = 0;
        foreach (object? item in collection)
        {
            sb.AppendLine($"[{i++}]: {FormatItem(item)}");
        }
        sb.AppendLine("---------------------------");
    }

    private static string FormatItem(object? item)
    {
        if (item is not Diagnostic d)
            return item?.ToString() ?? "null";

        FileLinePositionSpan lineSpan = d.Location.GetLineSpan();
        return $"[{d.Id}] {d.GetMessage()} (Line: {lineSpan.StartLinePosition.Line + 1}, Col: {lineSpan.StartLinePosition.Character + 1})";
    }

    private class AnalyzerAssertionException(string message) : Exception(message);
}
