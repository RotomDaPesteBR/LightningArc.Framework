using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC004 - Detects when Result.Success(null) is called for a non-nullable reference type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreventNullSuccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC004";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result.Success(null) for non-nullable type",
        messageFormat: "Passing 'null' to Result.Success for non-nullable type '{0}' may lead to unexpected nulls. Use a nullable Result or provide a value.",
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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            ResultTypeRecognizer recognizer = ResultTypeRecognizer.Resolve(
                compilationContext.Compilation
            );
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, recognizer),
                SyntaxKind.InvocationExpression
            );
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
        MemberAccessExpressionSyntax? memberAccess =
            invocation.Expression as MemberAccessExpressionSyntax;

        // If it's not a member access, it's not Result.Success (it's a static call)
        // Check for 'Success' or 'Ok'
        string methodName =
            memberAccess?.Name.Identifier.ValueText
            ?? (invocation.Expression as IdentifierNameSyntax)?.Identifier.ValueText
            ?? "";
        if (methodName != "Success" && methodName != "Ok")
        {
            return;
        }

        if (
            context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol
            is not IMethodSymbol symbol
        )
        {
            return;
        }

        // Check if the containing type is Result or Result<T>
        if (!recognizer.IsResultType(symbol.ContainingType))
        {
            return;
        }

        // Check if it's a generic method call or on a generic type
        ITypeSymbol? typeArgument = null;
        if (symbol.IsGenericMethod)
        {
            typeArgument = symbol.TypeArguments.FirstOrDefault();
        }
        else if (symbol.ContainingType.IsGenericType)
        {
            typeArgument = symbol.ContainingType.TypeArguments.FirstOrDefault();
        }

        if (typeArgument == null)
        {
            return;
        }

        // We only care about reference types
        if (!typeArgument.IsReferenceType)
        {
            return;
        }

        // If the type is explicitly nullable, it's fine
        if (typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return;
        }

        // In a #nullable enable context, if it's a reference type and NOT annotated, it's non-nullable.
        // If NRT is disabled, it might be 'None', but we usually want to warn anyway if it's a reference type
        // being passed a literal null in a framework that promotes NRT.

        // Check the argument passed
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var argument = invocation.ArgumentList.Arguments[0].Expression;

        // Detect literal null or default(T)
        if (IsNullLiteralOrDefault(argument, context.SemanticModel))
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Rule,
                argument.GetLocation(),
                typeArgument.Name
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullLiteralOrDefault(
        ExpressionSyntax expression,
        SemanticModel semanticModel
    )
    {
        // Remove casts and parentheses
        while (expression is CastExpressionSyntax cast)
        {
            expression = cast.Expression;
        }

        while (expression is ParenthesizedExpressionSyntax paren)
        {
            expression = paren.Expression;
        }

        // literal null
        if (expression.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return true;
        }

        // null! (null-forgiving)
        if (
            expression is PostfixUnaryExpressionSyntax postfix
            && postfix.IsKind(SyntaxKind.SuppressNullableWarningExpression)
        )
        {
            return IsNullLiteralOrDefault(postfix.Operand, semanticModel);
        }

        // default or default(T)
        if (expression.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            return true;
        }

        if (expression is DefaultExpressionSyntax)
        {
            return true;
        }

        return false;
    }
}
