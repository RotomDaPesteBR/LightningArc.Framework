using System.Reflection;
using System.Text.RegularExpressions;
using LightningArc.Analyzers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public sealed class AnalyzerIdConsistencyTests
{
    [Test]
    public async Task AllDiagnosticIds_MatchFormat_AreUnique_InBand_AndTracked()
    {
        var ids = typeof(ResultDiscardedAnalyzer).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.GetField("DiagnosticId") is not null)
            .Select(t => (Type: t, Id: (string)t.GetField("DiagnosticId")!.GetValue(null)!))
            .ToList();

        await Assert.That(ids).IsNotEmpty();
        await Assert.That(ids.Select(i => i.Id).Distinct().Count()).IsEqualTo(ids.Count);
        foreach (var (type, id) in ids)
        {
            await Assert.That(Regex.IsMatch(id, @"^LARC\d{3}$")).IsTrue();
            int n = int.Parse(id[4..]);
            bool inBand = (n is >= 1 and <= 19) || (n is >= 20 and <= 39)
                || (n is >= 40 and <= 59) || (n is >= 60 and <= 79) || (n is >= 80 and <= 99);
            await Assert.That(inBand).IsTrue();
        }

        string shipped = await File.ReadAllTextAsync(FindReleaseFile("AnalyzerReleases.Shipped.md"));
        string unshipped = await File.ReadAllTextAsync(FindReleaseFile("AnalyzerReleases.Unshipped.md"));
        foreach (var (_, id) in ids)
        {
            bool tracked = shipped.Contains(id) ^ unshipped.Contains(id);
            await Assert.That(tracked).IsTrue();
        }
    }

    private static string FindReleaseFile(string fileName)
    {
        // Tests run with the working directory set to the test output dir
        // (.artifacts/bin/...), so walk up to the repo root to locate the
        // release-tracking files that live under src/Analyzers/Analyzers/.
        string? dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            string candidate = Path.Combine(dir, "src", "Analyzers", "Analyzers", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new FileNotFoundException($"Could not locate {fileName} walking up from test output.");
    }
}
