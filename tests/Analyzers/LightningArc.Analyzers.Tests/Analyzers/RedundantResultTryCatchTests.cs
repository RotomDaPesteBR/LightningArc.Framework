using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
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

    [Test]
    public async Task Redundant_TryCatch_With_Handler_Reference_Reports()
    {
        // Regression pin (renumbering fix): LARC006 fires only when the compilation
        // can see ResultExceptionHandler. The shared verifier references
        // LightningArc.Results.AspNetCore, so the gate is open here and the
        // redundant shape must report.
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
    public async Task Redundant_TryCatch_Without_Handler_Reference_NoDiagnostic()
    {
        // Regression pin (renumbering fix): same try/catch shape as above, but the
        // compilation does not reference LightningArc.Results.AspNetCore, so the
        // middleware cannot cover this code and the rule must stay silent.
        // Reverting the gate (unconditional registration) reports here and fails.
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
                        return Error.Application.Internal();
                    }
                }
            }
            """;

        await new WithoutAspNetCoreTest(code).RunAsync(CancellationToken.None);
    }

    [Test]
    public async Task Redundant_TryCatch_CodeFix_Should_Splice_Try_Body()
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

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    return Result.Success();
                }
            }
            """;

        await CodeFixVerifier<
            RedundantResultTryCatchAnalyzer,
            RedundantResultTryCatchCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Redundant_TryCatch_CodeFix_Should_Preserve_Leading_Comment()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    // The middleware already maps exceptions.
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

        const string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                Result Main()
                {
                    // The middleware already maps exceptions.
                    return Result.Success();
                }
            }
            """;

        await CodeFixVerifier<
            RedundantResultTryCatchAnalyzer,
            RedundantResultTryCatchCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    // Minimal harness mirroring AnalyzerVerifier.Test but WITHOUT the
    // LightningArc.Results.AspNetCore reference, so ResultExceptionHandler is
    // invisible and the LARC006 compilation gate stays closed. Test-file-only;
    // no production or shared-verifier changes.
    private sealed class WithoutAspNetCoreTest
        : CSharpAnalyzerTest<RedundantResultTryCatchAnalyzer, TUnitVerifier>
    {
        public WithoutAspNetCoreTest(string source)
        {
            TestCode = source;
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
            TestState.AdditionalReferences.Add(typeof(Result).Assembly);
            CompilerDiagnostics = CompilerDiagnostics.Warnings;
            DisabledDiagnostics.Add("CS1591");
            DisabledDiagnostics.Add("CS8604");
            DisabledDiagnostics.Add("CS1701");
            DisabledDiagnostics.Add("CS8019");
        }
    }
}
