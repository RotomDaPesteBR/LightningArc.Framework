using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ErrorAccumulationInLoopTests
{
    private const string Usings = """
        #nullable enable
        using System.Collections.Generic;
        using LightningArc.Results;
        """;

    [Test]
    public async Task ErrorPlusEquals_InsideForeach_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    foreach (var e in es) { [|errors += e|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideFor_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    for (int i = 0; i < es.Count; i++) { [|errors += es[i]|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideWhile_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    while (es.Count > 0) { [|errors += es[0]|]; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideDo_Reports()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(List<Error> es)
                {
                    Error? errors = null;
                    do { [|errors += es[0]|]; } while (false);
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_OutsideAnyLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(Error e)
                {
                    Error? errors = null;
                    errors += e;
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task IntPlusEquals_InsideLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(int[] xs)
                {
                    int total = 0;
                    foreach (var i in xs) { total += i; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task StringPlusEquals_InsideLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                void M(string[] xs)
                {
                    string s = "";
                    foreach (var x in xs) { s += x; }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InMethodCalledFromLoop_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                Error? _errors;

                void M(List<Error> es)
                {
                    foreach (var e in es) { Add(e); }
                }

                void Add(Error e)
                {
                    _errors += e;
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task ErrorPlusEquals_InsideLocalFunction_NoDiagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class C
            {
                Error? _errors;

                void M(List<Error> es)
                {
                    foreach (var e in es)
                    {
                        void Local() { _errors += e; }
                        Local();
                    }
                }
            }
            """;

        await AnalyzerVerifier<ErrorAccumulationInLoopAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
