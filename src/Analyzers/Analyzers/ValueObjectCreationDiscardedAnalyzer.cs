using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC022 - Detects Create() or TryCreate() method calls on ValueObject types
/// where the return value is discarded.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ValueObjectCreationDiscardedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC022";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "ValueObject creation result is discarded",
        messageFormat: "ValueObject creation result is discarded",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeExpressionStatement,
            SyntaxKind.ExpressionStatement
        );
    }

    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax expressionStatement)
        {
            return;
        }

        if (expressionStatement.Expression is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.ValueText;
        if (methodName != "Create" && methodName != "TryCreate")
        {
            return;
        }

        // Get the type that defines the Create/TryCreate method
        ITypeSymbol? containingType = null;
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(
            invocation,
            context.CancellationToken
        );
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            containingType = methodSymbol.ContainingType;
        }
        else if (memberAccess.Expression is IdentifierNameSyntax typeName)
        {
            TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(
                typeName,
                context.CancellationToken
            );
            containingType = typeInfo.Type;
        }

        if (containingType is INamedTypeSymbol namedType && IsValueObjectType(namedType))
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsValueObjectType(INamedTypeSymbol type) =>
        ValueObjectTypeRecognizer.IsValueObjectType(type);
}
