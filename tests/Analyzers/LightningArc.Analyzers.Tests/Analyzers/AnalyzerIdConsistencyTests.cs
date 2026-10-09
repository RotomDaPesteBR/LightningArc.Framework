using System.Reflection;
using System.Text.RegularExpressions;
using LightningArc.Analyzers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public sealed class AnalyzerIdConsistencyTests
{
    // Every DiagnosticId declared in the assembly, mapped to its domain band.
    // A new analyzer must add its row here (the exact-count assertion fails
    // otherwise); a mis-banded ID fails the per-ID band assertion below.
    private static readonly IReadOnlyDictionary<string, (int Min, int Max, string Domain)> ExpectedBands =
        new Dictionary<string, (int Min, int Max, string Domain)>
        {
            ["LARC001"] = (1, 19, "Result"),
            ["LARC002"] = (1, 19, "Result"),
            ["LARC003"] = (1, 19, "Result"),
            ["LARC004"] = (1, 19, "Result"),
            ["LARC005"] = (1, 19, "Result"),
            ["LARC006"] = (1, 19, "Result"),
            ["LARC007"] = (1, 19, "Result"),
            ["LARC008"] = (1, 19, "Result"),
            ["LARC020"] = (20, 39, "ValueObject"),
            ["LARC021"] = (20, 39, "ValueObject"),
            ["LARC022"] = (20, 39, "ValueObject"),
            ["LARC023"] = (20, 39, "ValueObject"),
            ["LARC040"] = (40, 59, "Data"),
            ["LARC041"] = (40, 59, "Data"),
            ["LARC042"] = (40, 59, "Data"),
            ["LARC043"] = (40, 59, "Data"),
            ["LARC044"] = (40, 59, "Data"),
            ["LARC045"] = (40, 59, "Data"),
            ["LARC060"] = (60, 79, "Web"),
            ["LARC080"] = (80, 99, "Infra"),
        };

    [Test]
    public async Task AllDiagnosticIds_MatchFormat_AreUnique_InBand_AndTracked()
    {
        List<string> ids = typeof(ResultDiscardedAnalyzer).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.GetField("DiagnosticId") is not null)
            .Select(t => (string)t.GetField("DiagnosticId")!.GetValue(null)!)
            .ToList();

        await Assert.That(ids).IsNotEmpty();
        await Assert.That(ids.Count).IsEqualTo(ExpectedBands.Count);
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);

        // Collect per-ID problems so a red run names the offending ID
        // (this TUnit version has no `.Because()` on value assertions).
        var problems = new List<string>();
        foreach (string id in ids)
        {
            if (!Regex.IsMatch(id, @"^LARC\d{3}$"))
            {
                problems.Add($"'{id}' does not match ^LARCNNN");
            }
            else if (!ExpectedBands.TryGetValue(id, out (int Min, int Max, string Domain) band))
            {
                problems.Add($"'{id}' has no ExpectedBands row");
            }
            else
            {
                int n = int.Parse(id[4..]);
                if (n < band.Min || n > band.Max)
                {
                    problems.Add($"'{id}' is outside the {band.Domain} band");
                }
            }
        }

        await Assert.That(string.Join("; ", problems)).IsEqualTo(string.Empty);

        string shipped = await File.ReadAllTextAsync(FindReleaseFile("AnalyzerReleases.Shipped.md"));
        string unshipped = await File.ReadAllTextAsync(FindReleaseFile("AnalyzerReleases.Unshipped.md"));
        var untracked = new List<string>();
        foreach (string id in ids)
        {
            bool tracked = shipped.Contains(id) ^ unshipped.Contains(id);
            if (!tracked)
            {
                untracked.Add($"'{id}' is not in exactly one release-tracking file");
            }
        }

        await Assert.That(string.Join("; ", untracked)).IsEqualTo(string.Empty);
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
