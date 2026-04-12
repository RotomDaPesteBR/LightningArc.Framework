using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ImplicitStringToValueObjectTests
{
    private const string Usings = """
        using LightningArc.Primitives.ValueObjects; 
        """;

    [Test]
    public async Task String_Literal_Assignment_To_ValueObject_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    Email email = [|"test@example.com"|];
                }
            }
            """;

        await AnalyzerVerifier<ImplicitStringToValueObjectAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task String_Literal_As_ValueObject_Argument_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Process(Email email) { }

                void Main()
                {
                    Process([|"test@example.com"|]);
                }
            }
            """;

        await AnalyzerVerifier<ImplicitStringToValueObjectAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Explicit_Creation_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    var email = Email.Create("test@example.com");
                }
            }
            """;

        await AnalyzerVerifier<ImplicitStringToValueObjectAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
