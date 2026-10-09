using System.Collections.Immutable;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using LightningArc.Primitives.ValueObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

    // Currency has no implicit conversion to string (only to decimal), so any
    // string-slot usage is already broken code (CS0029) before the fix runs and the
    // shared CodeFixVerifier harness (which asserts compiler diagnostics) cannot
    // express it. These pins therefore drive the analyzer + fixer directly against a
    // real compilation: LARC021 must still fire, the fix must emit the compilable
    // currency?.ToString() ?? string.Empty form (never the decimal-typed
    // currency?.Value ?? string.Empty), and the fixed output must compile cleanly.

    [Test]
    public async Task Currency_Declarator_Fix_Should_Use_ToString_Form_And_Compile()
    {
        string fixedSource = await ApplyLarc021FixAsync("string s = currency;");

        await Assert.That(fixedSource).Contains("currency?.ToString() ?? string.Empty");
        await Assert.That(GetCompilationErrors(fixedSource)).IsEmpty();
    }

    [Test]
    public async Task Currency_Assignment_Fix_Should_Use_ToString_Form_And_Compile()
    {
        // The argument shape (Process(currency)) cannot trigger LARC021 for Currency:
        // AnalyzeArgument requires an implicit conversion to string, which Currency
        // lacks. The assignment shape fires via AnalyzeAssignment and exercises the
        // same fixer path.
        string fixedSource = await ApplyLarc021FixAsync(
            "s = currency;",
            "string s = string.Empty;"
        );

        await Assert.That(fixedSource).Contains("s = currency?.ToString() ?? string.Empty");
        await Assert.That(GetCompilationErrors(fixedSource)).IsEmpty();
    }

    private static async Task<string> ApplyLarc021FixAsync(
        string statement,
        string? members = null
    )
    {
        string source = $$"""
            #nullable enable
            using LightningArc.Primitives.ValueObjects;

            class Program
            {
                {{members}}
                void Main(Currency? currency)
                {
                    {{statement}}
                }
            }
            """;

        using AdhocWorkspace workspace = new();
        Project project = workspace
            .AddProject("Pins", LanguageNames.CSharp)
            .AddMetadataReferences(GetPinMetadataReferences());
        Document document = project.AddDocument("Pin.cs", source);

        SemanticModel? model = await document.GetSemanticModelAsync(CancellationToken.None);
        Compilation? compilation = model?.Compilation;
        await Assert.That(compilation).IsNotNull();

        ImmutableArray<Diagnostic> analyzerDiagnostics = await compilation!
            .WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(
                    new NullValueObjectToStringConversionAnalyzer()
                )
            )
            .GetAnalyzerDiagnosticsAsync();
        Diagnostic larc021 = analyzerDiagnostics.Single(d =>
            d.Id == NullValueObjectToStringConversionAnalyzer.DiagnosticId
        );

        List<CodeAction> actions = [];
        var fixContext = new CodeFixContext(
            document,
            larc021,
            (action, diagnostics) => actions.Add(action),
            CancellationToken.None
        );
        await new NullValueObjectToStringConversionCodeFixProvider().RegisterCodeFixesAsync(
            fixContext
        );
        await Assert.That(actions.Count).IsEqualTo(1);

        foreach (
            ApplyChangesOperation operation in await actions[0].GetOperationsAsync(
                CancellationToken.None
            )
        )
        {
            operation.Apply(workspace, CancellationToken.None);
        }

        Document? fixedDocument = workspace.CurrentSolution.GetDocument(document.Id);
        await Assert.That(fixedDocument).IsNotNull();
        return (await fixedDocument!.GetTextAsync()).ToString();
    }

    private static string[] GetCompilationErrors(string source)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "CurrencyFixOutput",
            [CSharpSyntaxTree.ParseText(source)],
            GetPinMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToArray();
    }

    private static IEnumerable<MetadataReference> GetPinMetadataReferences()
    {
        string? trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        foreach (string path in trusted!.Split(Path.PathSeparator))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (name is "System.Private.CoreLib" or "System.Runtime")
            {
                yield return MetadataReference.CreateFromFile(path);
            }
        }

        yield return MetadataReference.CreateFromFile(typeof(Currency).Assembly.Location);
    }
}
