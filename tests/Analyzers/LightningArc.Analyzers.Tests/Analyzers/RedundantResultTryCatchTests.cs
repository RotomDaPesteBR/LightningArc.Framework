using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class RedundantResultTryCatchTests
{
    private const string Usings = """
        using System;
        using LightningArc.Results;
        """;

    [Test]
    public async Task Redundant_TryCatch_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    [|try|]
                    {
                        return Result.Success();
                    }
                    catch (Exception)
                    {
                        return Error.Application.Internal();
                    }
                }
            }
            """;

        await AnalyzerVerifier<RedundantResultTryCatchAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task TryCatch_With_Logging_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    try
                    {
                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return Error.Application.Internal();
                    }
                }
            }
            """;

        await AnalyzerVerifier<RedundantResultTryCatchAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task TryCatch_With_Specific_Exception_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    try
                    {
                        return Result.Success();
                    }
                    catch (InvalidOperationException)
                    {
                        return Error.Application.Internal();
                    }
                }
            }
            """;

        await AnalyzerVerifier<RedundantResultTryCatchAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task TryCatch_Returning_Multiple_Statements_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    try
                    {
                        return Result.Success();
                    }
                    catch (Exception)
                    {
                        var e = Error.Application.Internal();
                        return e;
                    }
                }
            }
            """;

        await AnalyzerVerifier<RedundantResultTryCatchAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
