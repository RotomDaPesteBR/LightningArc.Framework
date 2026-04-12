using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ValueObjectCreationDiscardedTests
{
    private const string Usings = """
        using LightningArc.Primitives.ValueObjects; 
        """;

    [Test]
    public async Task Discarded_ValueObject_Create_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    [|Email.Create("test@example.com")|];
                }
            }
            """;

        await AnalyzerVerifier<ValueObjectCreationDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Discarded_ValueObject_TryCreate_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    [|Email.TryCreate("test@example.com", out _)|];
                }
            }
            """;

        await AnalyzerVerifier<ValueObjectCreationDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Assigned_ValueObject_Create_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var email = Email.Create("test@example.com");
                }
            }
            """;

        await AnalyzerVerifier<ValueObjectCreationDiscardedAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
