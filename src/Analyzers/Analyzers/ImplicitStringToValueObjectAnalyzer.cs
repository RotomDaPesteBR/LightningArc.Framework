using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC010 - Detects implicit conversion from string literal to ValueObject types
/// (Email, Cpf, Cnpj, PhoneNumber, Url) in LightningArc.Primitives namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ImplicitStringToValueObjectAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC010";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    private static readonly string[] KnownValueObjectNames =
    [
        "Email", "Cpf", "Cnpj", "PhoneNumber", "Url"
    ];

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Implicit conversion from string to ValueObject",
        messageFormat: "Implicit conversion from string to '{0}' may throw. Use TryCreate or Create for explicit validation.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.Argument);
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
            return;

        if (assignment.Right is not LiteralExpressionSyntax literal ||
            !literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;

        if (!TryGetTypeName(assignment.Left, context.SemanticModel, out var typeName))
            return;

        if (IsKnownValueObjectTypeName(typeName!))
        {
            var diagnostic = Diagnostic.Create(Rule, assignment.Right.GetLocation(), typeName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ArgumentSyntax argument)
            return;

        // Skip ref/out arguments
        if (argument.RefOrOutKeyword != default)
            return;

        if (argument.Expression is not LiteralExpressionSyntax literal ||
            literal.Kind() != SyntaxKind.StringLiteralExpression)
            return;

        // Get the type the argument is being converted to
        var typeInfo = context.SemanticModel.GetTypeInfo(argument.Expression);
        if (typeInfo.ConvertedType is not INamedTypeSymbol destType)
            return;

        var typeName = destType.Name;
        if (IsKnownValueObjectTypeName(typeName))
        {
            var diagnostic = Diagnostic.Create(Rule, argument.Expression.GetLocation(), typeName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool TryGetTypeName(ExpressionSyntax expression, SemanticModel semanticModel, out string? typeName)
    {
        typeName = null;

        var typeInfo = semanticModel.GetTypeInfo(expression);
        if (typeInfo.ConvertedType is not INamedTypeSymbol destType)
            return false;

        // Check known value object names
        if (IsKnownValueObjectTypeName(destType.Name))
        {
            typeName = destType.Name;
            return true;
        }

        // Check if type is in LightningArc.Primitives namespace
        if (destType.ContainingNamespace.ToDisplayString().Contains("LightningArc.Primitives"))
        {
            typeName = destType.Name;
            return true;
        }

        return false;
    }

    private static bool IsKnownValueObjectTypeName(string typeName)
    {
        return Array.IndexOf(KnownValueObjectNames, typeName) >= 0 ||
               typeName.EndsWith("ValueObject");
    }
}
