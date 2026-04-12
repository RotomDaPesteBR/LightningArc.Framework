using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
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
}
