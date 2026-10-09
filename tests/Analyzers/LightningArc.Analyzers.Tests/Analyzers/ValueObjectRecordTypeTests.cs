using System.Linq;
using System.Threading;
using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    [Test]
    public async Task CodeFix_With_Equals_Override_Offers_No_Fix()
    {
        const string source = """
            public class MyValueObject
            {
                public string Value => "test";
                public override bool Equals(object? obj) => obj is MyValueObject other && other.Value == Value;
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_GetHashCode_Override_Offers_No_Fix()
    {
        const string source = """
            public class MyValueObject
            {
                public string Value => "test";
                public override int GetHashCode() => Value.GetHashCode();
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Equality_Operators_Offers_No_Fix()
    {
        const string source = """
            public class MyValueObject
            {
                public string Value => "test";
                public static bool operator ==(MyValueObject a, MyValueObject b) => true;
                public static bool operator !=(MyValueObject a, MyValueObject b) => false;
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    [Test]
    public async Task CodeFix_With_Constructor_Body_Offers_No_Fix()
    {
        const string source = """
            public class MyValueObject
            {
                public MyValueObject() { Value = "test"; }
                public string Value { get; }
            }
            """;

        await Assert.That(await CountFixesOfferedAsync(source)).IsEqualTo(0);
    }

    private static async Task<int> CountFixesOfferedAsync(string source)
    {
        // GAP-8 guard pins: the diagnostic still fires on these types, but the
        // provider must decline to offer the class->record swap. Drive the
        // provider directly with a diagnostic on the class identifier, since
        // CodeFixVerifier cannot express "diagnostic present, no fix offered".
        using AdhocWorkspace workspace = new();
        Document document = workspace
            .AddProject("Guards", LanguageNames.CSharp)
            .AddDocument("Guard.cs", source);

        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        await Assert.That(tree).IsNotNull();

        var root = await tree!.GetRootAsync(CancellationToken.None);
        var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        Diagnostic diagnostic = Diagnostic.Create(
            ValueObjectRecordTypeAnalyzer.Rule,
            Location.Create(tree, classDeclaration.Identifier.Span)
        );

        int fixesOffered = 0;
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, diagnostics) => fixesOffered++,
            CancellationToken.None
        );

        await new ValueObjectRecordTypeCodeFixProvider().RegisterCodeFixesAsync(context);

        return fixesOffered;
    }
}
