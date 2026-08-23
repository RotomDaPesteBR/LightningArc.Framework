using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC020 - Detects implicit conversion from string literal to ValueObject types
/// (Email, Cpf, Cnpj, PhoneNumber, Url) in LightningArc.Primitives namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ImplicitStringToValueObjectAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC020";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Implicit conversion from string to ValueObject",
        messageFormat: "Implicit conversion from string to '{0}' may throw. Use TryCreate or Create for explicit validation.",
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

        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.Argument);
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
    }

    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not VariableDeclaratorSyntax declarator)
        {
            return;
        }

        if (
            declarator.Initializer?.Value is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression)
        )
        {
            return;
        }

        // Get the type the variable is declared as
        ILocalSymbol? symbol = context.SemanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol;
        if (symbol?.Type is not INamedTypeSymbol destType)
        {
            // Could be a field
            IFieldSymbol? fieldSymbol =
                context.SemanticModel.GetDeclaredSymbol(declarator) as IFieldSymbol;
            if (fieldSymbol?.Type is not INamedTypeSymbol fieldType)
            {
                return;
            }

            destType = fieldType;
        }

        if (ValueObjectTypeRecognizer.IsValueObjectType(destType))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                declarator.Initializer.Value.GetLocation(),
                destType.Name
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (
            assignment.Right is not LiteralExpressionSyntax literal
            || !literal.IsKind(SyntaxKind.StringLiteralExpression)
        )
        {
            return;
        }

        if (!TryGetTypeName(assignment.Left, context.SemanticModel, out string? typeName))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(Rule, assignment.Right.GetLocation(), typeName);
        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ArgumentSyntax argument)
        {
            return;
        }

        // Skip ref/out arguments
        if (argument.RefOrOutKeyword != default)
        {
            return;
        }

        if (
            argument.Expression is not LiteralExpressionSyntax literal
            || literal.Kind() != SyntaxKind.StringLiteralExpression
        )
        {
            return;
        }

        // Get the type the argument is being converted to
        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(argument.Expression);
        if (typeInfo.ConvertedType is not INamedTypeSymbol destType)
        {
            return;
        }

        if (ValueObjectTypeRecognizer.IsValueObjectType(destType))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                argument.Expression.GetLocation(),
                destType.Name
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool TryGetTypeName(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        out string? typeName
    )
    {
        typeName = null;

        TypeInfo typeInfo = semanticModel.GetTypeInfo(expression);
        if (typeInfo.ConvertedType is not INamedTypeSymbol destType)
        {
            return false;
        }

        if (!ValueObjectTypeRecognizer.IsValueObjectType(destType))
        {
            return false;
        }

        typeName = destType.Name;
        return true;
    }
}
