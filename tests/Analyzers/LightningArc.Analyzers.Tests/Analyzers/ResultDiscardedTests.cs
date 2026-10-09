using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ResultDiscardedTests
{
    private const string Usings = """
        using LightningArc.Results;
        """;

    [Test]
    public async Task Discarded_Result_Invocation_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result DoWork() => Result.Success();

                void Main()
                {
                    [|DoWork()|];
                }
            }
            """;

        await AnalyzerVerifier<ResultDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Assigned_Result_Invocation_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result DoWork() => Result.Success();

                void Main()
                {
                    var result = DoWork();
                }
            }
            """;

        await AnalyzerVerifier<ResultDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Result_In_If_Condition_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result DoWork() => Result.Success();

                void Main()
                {
                    if (DoWork().IsSuccess) { }
                }
            }
            """;

        await AnalyzerVerifier<ResultDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Discarded_SearchResult_Invocation_NoDiagnostic()
    {
        // Regression pin (renumbering fix): LARC003 used a .Contains("Result")
        // substring match and false-matched any consumer type with "Result" in
        // its name. Symbol equality must reject this; reverting reports and fails.
        const string code = $$"""
            {{Usings}}

            class SearchResult
            {
            }

            class Program
            {
                SearchResult DoWork() => new SearchResult();

                void Main()
                {
                    DoWork();
                }
            }
            """;

        await AnalyzerVerifier<ResultDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Discarded_ApiResult_Invocation_NoDiagnostic()
    {
        // Regression pin (renumbering fix): same substring trap as SearchResult
        // above, with a second representative consumer-type name.
        const string code = $$"""
            {{Usings}}

            class ApiResult
            {
            }

            class Program
            {
                ApiResult DoWork() => new ApiResult();

                void Main()
                {
                    DoWork();
                }
            }
            """;

        await AnalyzerVerifier<ResultDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
