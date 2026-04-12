using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ResultErrorShadowingTests
{
    private const string Usings = """
        using LightningArc.Results;
        """;

    [Test]
    public async Task Shadowing_In_IfFailure_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return [|Error.Validation.InvalidParameter("Shadowed")|];
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Propagating_Original_Error_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        return result.Error;
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Error_Then_Returning_New_One_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        var msg = result.Error.Message;
                        return Error.Validation.InvalidParameter("New Error");
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Via_MapError_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    return result.MapError(e => Error.Validation.InvalidParameter("Mapped"));
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Consuming_Via_TryGetError_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.TryGetError(out var err))
                    {
                        return Error.Validation.InvalidParameter("From TryGet");
                    }
                    return Result.Success();
                }
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Passing_To_Method_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main(Result result)
                {
                    if (result.IsFailure)
                    {
                        Log(result.Error);
                        return Error.Validation.InvalidParameter("Logged");
                    }
                    return Result.Success();
                }

                void Log(Error e) {}
            }
            """;

        await AnalyzerVerifier<ResultErrorShadowingAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
