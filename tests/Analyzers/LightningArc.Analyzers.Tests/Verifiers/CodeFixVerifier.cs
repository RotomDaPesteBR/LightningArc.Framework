using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Repositories;
using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace LightningArc.Analyzers.Tests.Verifiers;

public static class CodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic() =>
        CSharpCodeFixVerifier<TAnalyzer, TCodeFix, TUnitVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(string diagnosticId) =>
        CSharpCodeFixVerifier<TAnalyzer, TCodeFix, TUnitVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
        CSharpCodeFixVerifier<TAnalyzer, TCodeFix, TUnitVerifier>.Diagnostic(descriptor);

    public static async Task VerifyCodeFixAsync(string source, string fixedSource) =>
        await VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    public static async Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult expected,
        string fixedSource
    ) => await VerifyCodeFixAsync(source, [expected], fixedSource);

    public static async Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult[] expected,
        string fixedSource
    )
    {
        Test test = new() { TestCode = source, FixedCode = fixedSource };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, TUnitVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

            // Add references to the actual project assemblies
            TestState.AdditionalReferences.Add(typeof(Result).Assembly);
            TestState.AdditionalReferences.Add(typeof(Email).Assembly);
            TestState.AdditionalReferences.Add(typeof(RepositoryBase).Assembly);
            TestState.AdditionalReferences.Add(typeof(IMapper).Assembly);

            // Show compiler warnings/errors so we know if our stubs/test code is broken
            CompilerDiagnostics = CompilerDiagnostics.Warnings;

            // Suppress XML documentation warnings which are irrelevant for tests
            DisabledDiagnostics.Add("CS1591");
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            yield return new TAnalyzer();
        }

        protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
        {
            yield return new TCodeFix();
        }
    }
}
