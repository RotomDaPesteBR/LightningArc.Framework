using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Results;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ResultValueUnsafeAccessTests
{
    private const string Usings = """
        using LightningArc.Results;
        """;

    [Test]
    public async Task Value_Access_Without_Guard_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    var x = result.[|Value|];
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Value_Access_With_IsSuccess_Guard_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    if (result.IsSuccess)
                    {
                        var x = result.Value;
                    }
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Value_Access_With_NullForgiving_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    var x = result.Value!;
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Value_Access_In_Ternary_With_Guard_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    var x = result.IsSuccess ? result.Value : 0;
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Value_After_IsFailure_Early_Return_NoDiagnostic()
    {
        // Regression pin (renumbering fix): ResultAccessSafetyRecognizer.IsGuardedByPrecedingExit
        // recognizes the early-return guard-clause idiom. A wrapping-guard-only check
        // false-positives here, so reverting the fix makes this report and fail.
        string code = $$"""
            {{Usings}}

            class Program
            {
                int Main(Result<int> result)
                {
                    if (result.IsFailure)
                    {
                        return -1;
                    }
                    return result.Value;
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task NonGenericResult_HasNoValueMember_WideningIsSafe()
    {
        // Pin for GAP-6: `Value` exists only on generic `Result<TValue>`
        // (Result.cs:410 inside `Result<TValue>`); non-generic `Result` has
        // no `Value` member. The analyzer deliberately widens beyond
        // generic-only (see comment in ResultValueUnsafeAccessAnalyzer), so
        // ordinary non-generic use must stay silent.
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    if (result.IsSuccess)
                    {
                    }
                }
            }
            """;

        await AnalyzerVerifier<ResultValueUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task NonGenericResult_ValueAccess_StillReports_WideningPin()
    {
        // Pin for GAP-6 widening: the check deliberately matches any Result
        // (generic or not). A `.Value` access on non-generic `Result` only
        // occurs in code that already fails to compile (no such member), so
        // firing here is a redundant diagnostic on broken code, not a false
        // positive on valid code. Narrowing the check back to generic-only
        // makes this test fail.
        // Uses a test-file-only harness with CompilerDiagnostics.None so the
        // inevitable CS1061 (no `Value` on non-generic `Result`) does not
        // pollute the analyzer assertion.
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    var x = result.[|Value|];
                }
            }
            """;

        var test = new BrokenCodeTest(code);
        await test.RunAsync(CancellationToken.None);
    }

    [Test]
    public async Task Value_Access_CodeFix_Should_Wrap_In_TryGetValue()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    var x = result.[|Value|];
                }
            }
            """;

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result<int> result)
                {
                    if (result.TryGetValue(out var value))
                    {
                        var x = value;
                    }
                }
            }
            """;

        await CodeFixVerifier<
            ResultValueUnsafeAccessAnalyzer,
            ResultValueUnsafeAccessCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    // Minimal harness mirroring AnalyzerVerifier<T>.Test but with
    // CompilerDiagnostics.None, so snippets that are intentionally
    // not-compiling (`.Value` on non-generic `Result`, which has no such
    // member) assert only the analyzer diagnostic. Test-file-only; no
    // production or shared-verifier changes.
    private sealed class BrokenCodeTest
        : CSharpAnalyzerTest<ResultValueUnsafeAccessAnalyzer, TUnitVerifier>
    {
        public BrokenCodeTest(string source)
        {
            TestCode = source;
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
            TestState.AdditionalReferences.Add(typeof(Result).Assembly);
            CompilerDiagnostics = CompilerDiagnostics.None;
        }
    }
}
