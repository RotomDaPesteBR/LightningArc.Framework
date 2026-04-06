using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC011 - Detects when a nullable ValueObject variable is used in a context
/// that triggers implicit conversion to string.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NullValueObjectToStringConversionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC011";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    private static readonly string[] KnownValueObjectNames =
    [
        "Email", "Cpf", "Cnpj", "PhoneNumber", "Url"
    ];

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Potential null ValueObject conversion to string",
        messageFormat: "Potential null conversion to string may throw",
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

    /// <summary>
    /// Detects: string s = nullableValueObject;
    /// </summary>
    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
            return;

        // Check if the left side is typed as string
        var leftTypeInfo = context.SemanticModel.GetTypeInfo(assignment.Left);
        if (leftTypeInfo.Type?.SpecialType != SpecialType.System_String)
            return;

        // Check if the right side is a nullable ValueObject identifier
        if (assignment.Right is not IdentifierNameSyntax { Identifier.ValueText: { } varName })
            return;

        var symbol = context.SemanticModel.GetSymbolInfo(assignment.Right).Symbol as ILocalSymbol;
        if (symbol == null)
        {
            // Could be a field or parameter
            var fieldOrParamSymbol = context.SemanticModel.GetSymbolInfo(assignment.Right).Symbol;
            if (fieldOrParamSymbol is IFieldSymbol or IParameterSymbol or IPropertySymbol)
            {
                var fieldType = fieldOrParamSymbol switch
                {
                    IFieldSymbol f => f.Type,
                    IParameterSymbol p => p.Type,
                    IPropertySymbol pr => pr.Type,
                    _ => null
                };
                if (fieldType != null && IsNullableValueObject(fieldType))
                {
                    var diagnostic = Diagnostic.Create(Rule, assignment.Right.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
            return;
        }

        if (IsNullableValueObject(symbol.Type))
        {
            var diagnostic = Diagnostic.Create(Rule, assignment.Right.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Detects: SomeMethod(nullableValueObject)  where parameter is string
    /// </summary>
    private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ArgumentSyntax { Expression: IdentifierNameSyntax argIdent })
            return;

        // Get the expected parameter type from the invocation
        var typeInfo = context.SemanticModel.GetTypeInfo(argIdent);
        if (typeInfo.ConvertedType?.SpecialType != SpecialType.System_String)
            return;

        // Check if the argument symbol is a nullable ValueObject
        var argSymbol = context.SemanticModel.GetSymbolInfo(argIdent).Symbol;
        if (argSymbol == null)
            return;

        var argType = argSymbol switch
        {
            ILocalSymbol local => local.Type,
            IParameterSymbol param => param.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol prop => prop.Type,
            _ => null
        };

        if (argType != null && IsNullableValueObject(argType))
        {
            var diagnostic = Diagnostic.Create(Rule, argIdent.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullableValueObject(ITypeSymbol type)
    {
        // Check known value object names
        if (Array.IndexOf(KnownValueObjectNames, type.Name) < 0 &&
            !type.Name.EndsWith("ValueObject"))
            return false;

        // Check if nullable (reference type or nullable annotation)
        return type.NullableAnnotation != NullableAnnotation.NotAnnotated;
    }
}
