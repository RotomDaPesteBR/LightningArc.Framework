using System.Data.Common;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Repositories;
using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace LightningArc.Analyzers.Tests.Verifiers;

public static class AnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic() =>
        CSharpAnalyzerVerifier<TAnalyzer, TUnitVerifier>.Diagnostic();

    public static DiagnosticResult Diagnostic(string diagnosticId) =>
        CSharpAnalyzerVerifier<TAnalyzer, TUnitVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
        CSharpAnalyzerVerifier<TAnalyzer, TUnitVerifier>.Diagnostic(descriptor);

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        Test test = new() { TestCode = source };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    private class Test : CSharpAnalyzerTest<TAnalyzer, TUnitVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

            // Add references to the actual project assemblies
            TestState.AdditionalReferences.Add(typeof(Result).Assembly);
            TestState.AdditionalReferences.Add(typeof(Email).Assembly);
            TestState.AdditionalReferences.Add(typeof(RepositoryBase).Assembly);
            TestState.AdditionalReferences.Add(typeof(IMapper).Assembly);

            // Ignore compiler errors to focus on analyzer results
            CompilerDiagnostics = CompilerDiagnostics.Warnings;

            // Suppress XML documentation warnings which are irrelevant for tests
            DisabledDiagnostics.Add("CS1591");
            DisabledDiagnostics.Add("CS8604");
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            yield return new TAnalyzer();
        }
    }
}
