using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class PreventNullSuccessTests
{
    private const string Usings = """
        #nullable enable
        using LightningArc.Results;
        """;

    [Test]
    public async Task Success_With_Literal_Null_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var result = Result.Success<string>(value: [|(string)null!|]);
                }
            }
            """;

        await AnalyzerVerifier<PreventNullSuccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Success_With_Null_Forgiving_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var result = Result.Success<string>(value: [|(string)null!|]);
                }
            }
            """;

        await AnalyzerVerifier<PreventNullSuccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Success_With_Default_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var result = Result.Success<string>(value: [|default(string)!|]);
                }
            }
            """;

        await AnalyzerVerifier<PreventNullSuccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Success_With_Nullable_Type_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var result = Result.Success<string?>(value: null);
                }
            }
            """;

        await AnalyzerVerifier<PreventNullSuccessAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Success_With_Value_Type_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var result = Result.Success<int>(value: default);
                }
            }
            """;

        await AnalyzerVerifier<PreventNullSuccessAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
