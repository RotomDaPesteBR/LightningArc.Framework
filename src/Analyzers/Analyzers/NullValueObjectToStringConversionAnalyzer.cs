using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC021 - Detects when a nullable ValueObject variable is used in a context
/// that triggers implicit conversion to string.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullValueObjectToStringConversionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC021";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Potential null ValueObject conversion to string",
        messageFormat: "Potential null conversion to string may throw",
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

    /// <summary>
    /// Detects: string s = nullableValueObject;
    /// </summary>
    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not VariableDeclaratorSyntax declarator)
        {
            return;
        }

        if (declarator.Initializer == null)
        {
            return;
        }

        // Check if the variable is typed as string
        ITypeSymbol? declaredType = null;
        if (context.SemanticModel.GetDeclaredSymbol(declarator) is ILocalSymbol symbol)
        {
            declaredType = symbol.Type;
        }
        else if (context.SemanticModel.GetDeclaredSymbol(declarator) is IFieldSymbol fieldSymbol)
        {
            declaredType = fieldSymbol.Type;
        }

        if (declaredType?.SpecialType != SpecialType.System_String)
        {
            return;
        }

        // Check if the assigned value is a nullable ValueObject
        TypeInfo valueTypeInfo = context.SemanticModel.GetTypeInfo(declarator.Initializer.Value);
        if (valueTypeInfo.Type != null && IsNullableValueObject(valueTypeInfo.Type))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                declarator.Initializer.Value.GetLocation()
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Detects: string s = nullableValueObject;
    /// </summary>
    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        // Check if the left side is typed as string
        TypeInfo leftTypeInfo = context.SemanticModel.GetTypeInfo(assignment.Left);
        if (leftTypeInfo.Type?.SpecialType != SpecialType.System_String)
        {
            return;
        }

        // Check if the right side is a nullable ValueObject
        TypeInfo rightTypeInfo = context.SemanticModel.GetTypeInfo(assignment.Right);
        if (rightTypeInfo.Type != null && IsNullableValueObject(rightTypeInfo.Type))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, assignment.Right.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Detects: SomeMethod(nullableValueObject)  where parameter is string
    /// </summary>
    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ArgumentSyntax argument)
        {
            return;
        }

        // Get the expected parameter type from the invocation
        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(argument.Expression);
        if (typeInfo.ConvertedType?.SpecialType != SpecialType.System_String)
        {
            return;
        }

        // Check if the expression type is a nullable ValueObject
        if (typeInfo.Type != null && IsNullableValueObject(typeInfo.Type))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, argument.Expression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullableValueObject(ITypeSymbol type)
    {
        if (!ValueObjectTypeRecognizer.IsValueObjectType(type))
        {
            return false;
        }

        // In #nullable enable context, NotAnnotated means non-nullable (e.g., Email)
        // Annotated means nullable (e.g., Email?)
        // None means we are not in a nullable context, or it's a value type (not our case)
        return type.NullableAnnotation != NullableAnnotation.NotAnnotated;
    }
}
