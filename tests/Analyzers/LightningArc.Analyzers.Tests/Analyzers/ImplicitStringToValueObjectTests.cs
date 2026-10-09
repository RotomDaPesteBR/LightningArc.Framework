using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
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
    public async Task String_Literal_Declaration_CodeFix_Should_Wrap_With_Create()
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

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    Email email = Email.Create("test@example.com");
                }
            }
            """;

        await CodeFixVerifier<
            ImplicitStringToValueObjectAnalyzer,
            ImplicitStringToValueObjectCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task String_Literal_Argument_CodeFix_Should_Wrap_With_Create()
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

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Process(Email email) { }

                void Main()
                {
                    Process(Email.Create("test@example.com"));
                }
            }
            """;

        await CodeFixVerifier<
            ImplicitStringToValueObjectAnalyzer,
            ImplicitStringToValueObjectCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task String_Literal_Assignment_CodeFix_Should_Wrap_With_Create()
    {
        string code = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    Email email = Email.Create("a@example.com");
                    email = [|"b@example.com"|];
                }
            }
            """;

        string fixedCode = $$"""
            {{Usings}}

            class Program
            {
                void Main()
                {
                    Email email = Email.Create("a@example.com");
                    email = Email.Create("b@example.com");
                }
            }
            """;

        await CodeFixVerifier<
            ImplicitStringToValueObjectAnalyzer,
            ImplicitStringToValueObjectCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task String_Literal_Aliased_Declaration_CodeFix_Should_Use_Alias_Spelling()
    {
        // Pin: the fix must reuse the declaration's own type spelling (alias
        // `E`), not the underlying type name (`Email`).
        string code = """
            using E = LightningArc.Primitives.ValueObjects.Email;

            class Program
            {
                void Main()
                {
                    E email = [|"test@example.com"|];
                }
            }
            """;

        string fixedCode = """
            using E = LightningArc.Primitives.ValueObjects.Email;

            class Program
            {
                void Main()
                {
                    E email = E.Create("test@example.com");
                }
            }
            """;

        await CodeFixVerifier<
            ImplicitStringToValueObjectAnalyzer,
            ImplicitStringToValueObjectCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
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
