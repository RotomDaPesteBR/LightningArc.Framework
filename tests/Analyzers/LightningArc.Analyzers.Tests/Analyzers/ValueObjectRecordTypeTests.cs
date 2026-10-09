using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Analyzers;

public class ValueObjectRecordTypeTests
{
    private const string Usings = """
        using LightningArc.Primitives;
        """;

    [Test]
    public async Task Class_Implementing_IValueObject_Should_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public class [|MyValueObject|] : IValueObject<string>
            {
                public string Value => "test";
            }
            """;

        await AnalyzerVerifier<ValueObjectRecordTypeAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Record_Implementing_IValueObject_Should_Not_Report_Diagnostic()
    {
        const string code = $$"""
            {{Usings}}

            public record MyValueObject(string Value) : IValueObject<string>;
            """;

        await AnalyzerVerifier<ValueObjectRecordTypeAnalyzer>.VerifyAnalyzerAsync(code);
    }

    [Test]
    public async Task Class_Implementing_IValueObject_CodeFix_Should_Convert_To_Record()
    {
        const string code = $$"""
            {{Usings}}

            public class [|MyValueObject|] : IValueObject<string>
            {
                public string Value => "test";
            }
            """;

        const string fixedCode = $$"""
            {{Usings}}

            public record MyValueObject : IValueObject<string>
            {
                public string Value => "test";
            }
            """;

        await CodeFixVerifier<
            ValueObjectRecordTypeAnalyzer,
            ValueObjectRecordTypeCodeFixProvider
        >.VerifyCodeFixAsync(code, fixedCode);
    }

    [Test]
    public async Task CodeFix_With_Diagnostic_Outside_Any_Class_Offers_No_Fix_Without_Throwing()
    {
        // Regression pin (renumbering fix): the provider looked up the enclosing
        // class with .First(), so the subsequent null check was dead code and an
        // unmatched span threw InvalidOperationException instead of declining.
        // Note: CodeFixVerifier cannot express "diagnostic present, no fix
        // offered" (it fails when no fix is registered), so the provider is
        // driven directly with a diagnostic whose span sits outside any class.
        // Reverting to .First() throws here and fails.
        const string source = "// no types declared here\n";

        using AdhocWorkspace workspace = new();
        Document document = workspace
            .AddProject("Pins", LanguageNames.CSharp)
            .AddDocument("Pin.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        Diagnostic diagnostic = Diagnostic.Create(
            ValueObjectRecordTypeAnalyzer.Rule,
            Location.Create(tree!, new TextSpan(0, 0))
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new ValueObjectRecordTypeCodeFixProvider().RegisterCodeFixesAsync(context);

        await Assert.That(fixesOffered).IsEqualTo(0);
    }
}
