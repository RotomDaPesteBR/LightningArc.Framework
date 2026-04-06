using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC012 - Detects Create() or TryCreate() method calls on ValueObject types
/// where the return value is discarded.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueObjectCreationDiscardedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC012";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    private static readonly string[] KnownValueObjectNames =
    [
        "Email", "Cpf", "Cnpj", "PhoneNumber", "Url"
    ];

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ValueObject creation result is discarded",
        messageFormat: "ValueObject creation result is discarded",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
    }

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax expressionStatement)
            return;

        if (expressionStatement.Expression is not InvocationExpressionSyntax invocation)
            return;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        var methodName = memberAccess.Name.Identifier.ValueText;
        if (methodName != "Create" && methodName != "TryCreate")
            return;

        // Get the type that defines the Create/TryCreate method
        ITypeSymbol? containingType = null;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            containingType = methodSymbol.ContainingType;
        }
        else if (memberAccess.Expression is IdentifierNameSyntax typeName)
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(typeName, context.CancellationToken);
            containingType = typeInfo.Type;
        }

        if (containingType is INamedTypeSymbol namedType && IsValueObjectType(namedType))
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsValueObjectType(INamedTypeSymbol type)
    {
        // Check known value object names
        if (Array.IndexOf(KnownValueObjectNames, type.Name) >= 0 ||
            type.Name.EndsWith("ValueObject"))
            return true;

        // Check if in LightningArc.Primitives namespace
        if (type.ContainingNamespace.ToDisplayString() == "LightningArc.Primitives")
            return true;

        // Check if implements IValueObject
        foreach (var iface in type.AllInterfaces)
        {
            if (iface.Name.StartsWith("IValueObject"))
                return true;
        }

        // Check base type
        if (type.BaseType?.Name.Contains("ValueObject") == true)
            return true;

        return false;
    }
}
