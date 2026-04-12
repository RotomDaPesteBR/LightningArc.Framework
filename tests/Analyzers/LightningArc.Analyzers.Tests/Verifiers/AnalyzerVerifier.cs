using System.Data.Common;
using Dapper;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.ADO.Repositories;
using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;
using LightningArc.Results.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
            var assemblies = new HashSet<System.Reflection.Assembly>
            {
                typeof(Result).Assembly,
                typeof(Email).Assembly,
                typeof(RepositoryBase).Assembly,
                typeof(IMapper).Assembly,
                typeof(EndpointResult).Assembly,
                typeof(HttpContext).Assembly,
                typeof(IApplicationBuilder).Assembly,
                typeof(IEndpointRouteBuilder).Assembly,
                typeof(EndpointRouteBuilderExtensions).Assembly,
                typeof(RouteData).Assembly,
                typeof(ControllerBase).Assembly,
                typeof(SqlMapper).Assembly,
            };

            foreach (var assembly in assemblies)
            {
                TestState.AdditionalReferences.Add(assembly);
            }

            // Ignore compiler errors to focus on analyzer results
            CompilerDiagnostics = CompilerDiagnostics.Warnings;

            // Suppress XML documentation warnings which are irrelevant for tests
            DisabledDiagnostics.Add("CS1591");
            DisabledDiagnostics.Add("CS8604");
            DisabledDiagnostics.Add("CS1701");
            DisabledDiagnostics.Add("CS8019");
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            yield return new TAnalyzer();
        }
    }
}
