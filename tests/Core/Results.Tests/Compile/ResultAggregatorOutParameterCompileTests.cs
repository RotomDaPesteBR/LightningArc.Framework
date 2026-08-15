using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReflectionAssembly = System.Reflection.Assembly;
using ReflectionAssemblyName = System.Reflection.AssemblyName;

namespace LightningArc.Results.Tests;

/// <summary>
/// Compile-time regression tests proving that <c>Check&lt;T&gt;(Func&lt;T&gt;, out T?)</c>
/// cannot appear after an awaited step in a <see cref="ResultAggregator"/> chain.
/// </summary>
/// <remarks>
/// These tests do not exercise runtime behavior — they compile small in-memory snippets via
/// Roslyn and assert on the resulting diagnostics. This is the correct tool for proving a
/// compile-time guarantee; TUnit/xUnit-style runtime tests cannot verify "this does not compile."
/// If a future change accidentally makes either snippet compile, these tests fail loudly instead
/// of the guarantee silently eroding.
/// </remarks>
public class ResultAggregatorOutParameterCompileTests
{
    /// <summary>
    /// Proves the underlying language constraint: an <c>async</c> method cannot declare an
    /// <c>out</c> parameter (CS1988). This is *why* no <c>Task&lt;ResultAggregator&gt;</c> mirror
    /// of the <c>out</c> overload exists — it is not a design choice that could be revisited later,
    /// it is compiler-enforced.
    /// </summary>
    [Test]
    public async Task AsyncMethodWithOutParameter_FailsToCompile_WithCS1988()
    {
        const string source =
            """
            using System.Threading.Tasks;

            public static class Snippet
            {
                // This must never compile. If it does, the compiler's behavior around
                // async + out parameters has changed and the design assumption behind
                // the missing Task<ResultAggregator> mirror needs to be re-evaluated.
                public static async Task<int> BadAsyncOut(out int value)
                {
                    value = 42;
                    await Task.CompletedTask;
                    return value;
                }
            }
            """;

        CompilationResult result = CompileSnippet(source);
        Console.WriteLine(result);

        await Assert.That(result.HasErrorWithId("CS1988")).IsTrue();
    }

    /// <summary>
    /// Proves that today's actual API surface does not offer <c>Check&lt;T&gt;(Func&lt;T&gt;, out T?)</c>
    /// on a <see cref="Task{ResultAggregator}"/> receiver — i.e. calling it directly after
    /// <c>CheckAsync</c>/<c>CheckAll</c> fails to compile because no such overload exists.
    /// </summary>
    /// <remarks>
    /// This intentionally asserts only that compilation fails, not a specific diagnostic ID.
    /// When no overload matches on both arity and receiver type, Roslyn does not consistently
    /// report the same code (CS1061, CS1929, CS1501, or a lambda-conversion error like
    /// CS0029/CS1662 depending on which partial-match candidate it falls back to for
    /// diagnostics) — the stable, meaningful guarantee is "this does not compile," not which
    /// specific code it fails with.
    /// </remarks>
    [Test]
    public async Task OutOverload_AfterAsyncStep_FailsToCompile()
    {
        const string source =
            """
            using System.Threading.Tasks;
            using LightningArc.Results;

            public static class Snippet
            {
                public static async Task UsesChain()
                {
                    // .CheckAsync(...) returns Task<ResultAggregator>. Calling the out-parameter
                    // Check<T> overload directly on that receiver must fail to compile: no such
                    // extension exists on Task<ResultAggregator>, by design.
                    await Result.Aggregate()
                        .CheckAsync(() => Task.FromResult(Result.Success()))
                        .Check(() => 5, out var value);
                }
            }
            """;

        CompilationResult result = CompileSnippet(source);
        Console.WriteLine(result);

        await Assert.That(result.HasErrors).IsTrue();
    }

    /// <summary>
    /// Sanity check that the harness itself is wired correctly: a normal, valid chain using only
    /// methods confirmed to have full sync/async mirror coverage compiles cleanly. This guards
    /// against the test above passing for the wrong reason (e.g. a broken reference set that
    /// makes everything fail to compile), while deliberately avoiding any call shape whose mirror
    /// completeness is itself in question — that is a library-correctness concern, not something
    /// this harness-sanity test should depend on.
    /// </summary>
    [Test]
    public async Task ValidChain_AfterAsyncStep_CompilesSuccessfully()
    {
        const string source =
            """
            using System.Threading.Tasks;
            using LightningArc.Results;

            public static class Snippet
            {
                public static async Task UsesChain()
                {
                    Result result = await Result.Aggregate()
                        .CheckAsync(() => Task.FromResult(Result.Success()))
                        .CheckAsync(() => Task.FromResult(Result.Success()))
                        .BuildAsync();
                }
            }
            """;

        CompilationResult result = CompileSnippet(source);
        Console.WriteLine(result);

        await Assert.That(result.HasErrors).IsFalse();
    }

    private static CompilationResult CompileSnippet(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        MetadataReference[] references = GetReferences();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: $"CompileFailTest_{Guid.NewGuid():N}",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

        return new CompilationResult(diagnostics);
    }

    /// <summary>
    /// Builds the reference set: every trusted platform assembly (covers BCL types like
    /// <see cref="Task"/>), MINUS any entry that is actually a LightningArc project output
    /// (the test project's own <c>ProjectReference</c>s mean those are already present in
    /// <c>TRUSTED_PLATFORM_ASSEMBLIES</c> — leaving them in and then adding our own copy below
    /// produces duplicate/ambiguous-type errors like CS0433, not the diagnostic under test).
    /// LightningArc assemblies are then added back exactly once, resolved directly from the
    /// loaded <see cref="ResultAggregator"/> type and its own referenced assemblies, so there is
    /// exactly one physical reference per assembly regardless of path formatting differences.
    /// </summary>
    private static MetadataReference[] GetReferences()
    {
        var trustedAssembliesPaths = (
            (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!
        ).Split(Path.PathSeparator);

        List<MetadataReference> references =
        [
            .. trustedAssembliesPaths
                .Where(path => !Path.GetFileName(path).StartsWith("LightningArc.", StringComparison.OrdinalIgnoreCase))
                .Select(path => MetadataReference.CreateFromFile(path)),
        ];

        foreach (ReflectionAssembly lightningArcAssembly in ResolveLightningArcAssemblies())
        {
            references.Add(MetadataReference.CreateFromFile(lightningArcAssembly.Location));
        }

        return [.. references];
    }

    /// <summary>
    /// Walks the reference graph starting from the assembly containing <see cref="ResultAggregator"/>,
    /// following only assemblies whose name starts with <c>LightningArc.</c>, so every LightningArc
    /// dependency actually used by the snippet (e.g. <c>LightningArc.Primitives</c> if referenced
    /// transitively) is included exactly once, regardless of what happens to be in the TPA list.
    /// </summary>
    private static IReadOnlyCollection<ReflectionAssembly> ResolveLightningArcAssemblies()
    {
        var visited = new Dictionary<string, ReflectionAssembly>(StringComparer.OrdinalIgnoreCase);
        var toVisit = new Queue<ReflectionAssembly>();
        toVisit.Enqueue(typeof(ResultAggregator).Assembly);

        while (toVisit.Count > 0)
        {
            ReflectionAssembly current = toVisit.Dequeue();
            string name = current.GetName().Name!;

            if (!visited.TryAdd(name, current))
            {
                continue;
            }

            foreach (ReflectionAssemblyName referenced in current.GetReferencedAssemblies())
            {
                if (!referenced.Name!.StartsWith("LightningArc.", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ReflectionAssembly loaded = ReflectionAssembly.Load(referenced);
                toVisit.Enqueue(loaded);
            }
        }

        return visited.Values;
    }

    private readonly struct CompilationResult(ImmutableArray<Diagnostic> diagnostics)
    {
        private readonly ImmutableArray<Diagnostic> _diagnostics = diagnostics;

        public bool HasErrors { get; } = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

        public bool HasErrorWithId(string diagnosticId) =>
            _diagnostics.Any(d =>
                d.Severity == DiagnosticSeverity.Error
                && d.Id.Equals(diagnosticId, StringComparison.Ordinal)
            );

        /// <summary>
        /// Human-readable dump of every diagnostic, used to make a failing assertion
        /// self-explanatory instead of a bare true/false mismatch.
        /// </summary>
        public override string ToString() =>
            _diagnostics.Length == 0
                ? "(no diagnostics)"
                : string.Join(Environment.NewLine, _diagnostics.Select(d => d.ToString()));
    }
}
