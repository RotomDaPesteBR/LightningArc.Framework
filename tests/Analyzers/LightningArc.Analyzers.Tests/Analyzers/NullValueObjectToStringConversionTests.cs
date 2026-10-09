using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class NullValueObjectToStringConversionTests
{
    private const string Usings = """
        #nullable enable
        #pragma warning disable CS8604 // Every LARC021 repro passes a nullable ValueObject to a non-nullable string slot
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

    [Test]
    public async Task Nullable_ValueObject_Declarator_CodeFix_Should_Use_NullSafe_Value()
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

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main(Email? email)
                {
                    string s = email?.Value ?? string.Empty;
                }
            }
            """;

        await CodeFixVerifier<
            NullValueObjectToStringConversionAnalyzer,
            NullValueObjectToStringConversionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Nullable_ValueObject_Assignment_CodeFix_Should_Use_NullSafe_Value()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main(Email? email)
                {
                    string s = string.Empty;
                    s = [|email|];
                }
            }
            """;

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main(Email? email)
                {
                    string s = string.Empty;
                    s = email?.Value ?? string.Empty;
                }
            }
            """;

        await CodeFixVerifier<
            NullValueObjectToStringConversionAnalyzer,
            NullValueObjectToStringConversionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task Nullable_ValueObject_Argument_CodeFix_Should_Use_NullSafe_Value()
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

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Process(string s) { }

                void Main(Email? email)
                {
                    Process(email?.Value ?? string.Empty);
                }
            }
            """;

        await CodeFixVerifier<
            NullValueObjectToStringConversionAnalyzer,
            NullValueObjectToStringConversionCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }
}
