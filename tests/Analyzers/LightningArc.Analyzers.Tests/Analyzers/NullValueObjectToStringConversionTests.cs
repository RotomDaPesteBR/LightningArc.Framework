using LightningArc.Analyzers;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class NullValueObjectToStringConversionTests
{
    private const string Usings = """
        #nullable enable
        using LightningArc.Primitives.ValueObjects;
        """;

    [Test]
    public async Task Nullable_ValueObject_Assignment_To_String_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Email? email)
                {
                    string s = [|email|];
                }
            }
            """;

        await AnalyzerVerifier<NullValueObjectToStringConversionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Nullable_ValueObject_As_String_Argument_Should_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Process(string s) { }

                void Main(Email? email)
                {
                    Process([|email|]);
                }
            }
            """;

        await AnalyzerVerifier<NullValueObjectToStringConversionAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task NonNullable_ValueObject_Should_Not_Report_Diagnostic()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Email email)
                {
                    string s = email;
                }
            }
            """;

        await AnalyzerVerifier<NullValueObjectToStringConversionAnalyzer>.VerifyAnalyzerAsync(code);
    }
}
