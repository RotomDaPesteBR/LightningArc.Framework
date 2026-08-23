using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC023 - Detects when a ValueObject is defined as a class instead of a record.
/// Value Objects should use value-based equality, which records provide by default.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueObjectRecordTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC023";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ValueObject should be a record",
        messageFormat: "Value Object '{0}' should be defined as a 'record' to ensure value-based equality",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNamedType, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeNamedType(SyntaxNodeAnalysisContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (symbol == null)
        {
            return;
        }

        // Check if it implements IValueObject or inherits from a known ValueObject type
        if (IsValueObjectType(symbol))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                symbol.Name
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsValueObjectType(INamedTypeSymbol symbol) =>
        ValueObjectTypeRecognizer.IsValueObjectType(symbol);
}
