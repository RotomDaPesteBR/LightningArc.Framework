using LightningArc.Analyzers;
using LightningArc.Analyzers.CodeFixes;
using LightningArc.Analyzers.Tests.Verifiers;
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
}
