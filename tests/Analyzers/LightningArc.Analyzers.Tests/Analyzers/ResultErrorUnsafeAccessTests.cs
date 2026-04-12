using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ResultErrorUnsafeAccessTests
{
    private const string Usings = """
        using LightningArc.Results;
        """;

    [Test]
    public async Task Error_Access_Without_Guard_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    var x = result.[|Error|];
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Error_Access_With_IsFailure_Guard_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        var x = result.Error;
                    }
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Error_Access_With_NullForgiving_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    var x = result.Error!;
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorUnsafeAccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Error_Access_CodeFix_Should_Wrap_In_TryGetError()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    var x = result.[|Error|];
                }
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main(Result result)
                {
                    if (result.TryGetError(out var err))
                    {
                        var x = err;
                    }
                }
            }
            """;

        await CodeFixVerifier<
            ResultErrorUnsafeAccessAnalyzer,
            ResultErrorUnsafeAccessCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }
}
