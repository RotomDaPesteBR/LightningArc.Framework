using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC060 - Detects when a Minimal API endpoint returns a Result or Result of T
/// without calling .ToEndpointResult(), which leads to default JSON serialization.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MinimalApiResultMappingAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC060";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing .ToEndpointResult() in Minimal API",
        messageFormat: "Minimal API endpoints returning Result or Result<TValue> should use '.ToEndpointResult()' to ensure correct HTTP status code mapping",
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
                nodeContext => AnalyzeLambda(nodeContext, recognizer),
                SyntaxKind.SimpleLambdaExpression
            );
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeLambda(nodeContext, recognizer),
                SyntaxKind.ParenthesizedLambdaExpression
            );
        });
    }

    private static void AnalyzeLambda(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        LambdaExpressionSyntax lambda = (LambdaExpressionSyntax)context.Node;

        // Is this lambda an argument to a MapXxx method?
        var argument = lambda.Ancestors().OfType<ArgumentSyntax>().FirstOrDefault();
        if (argument == null)
        {
            return;
        }

        var invocation = argument.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation == null)
        {
            return;
        }

        if (
            context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol
                is not IMethodSymbol symbol
            || !IsMapMethod(symbol)
        )
        {
            return;
        }

        // 1. Try Semantic approach (Best)
        var typeInfo = context.SemanticModel.GetTypeInfo(lambda);
        ITypeSymbol? returnType = null;
        if (
            typeInfo.ConvertedType is INamedTypeSymbol delegateType
            && delegateType.DelegateInvokeMethod != null
        )
        {
            returnType = delegateType.DelegateInvokeMethod.ReturnType;
        }

        // 2. Try Syntax approach (Fallback for tests/complex inference)
        bool isResultReturn = false;
        if (returnType != null && recognizer.IsResultType(returnType))
        {
            isResultReturn = true;
        }
        else if (lambda.ExpressionBody != null)
        {
            // If it's => Result.Success(), check the expression directly
            var bodyType = context.SemanticModel.GetTypeInfo(lambda.ExpressionBody).Type;
            if (bodyType != null && recognizer.IsResultType(bodyType))
            {
                isResultReturn = true;
            }
        }

        if (isResultReturn)
        {
            // But NOT if it's already an EndpointResult or IResult
            if (returnType != null && IsHttpResultType(returnType))
            {
                return;
            }

            // Check syntax for .ToEndpointResult() call as a final guard
            if (lambda.ToString().Contains(".ToEndpointResult("))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation()));
        }
    }

    private static bool IsMapMethod(IMethodSymbol symbol)
    {
        string[] mapMethods =
        [
            "MapGet",
            "MapPost",
            "MapPut",
            "MapDelete",
            "MapPatch",
            "MapMethods",
        ];
        if (!mapMethods.Contains(symbol.Name))
        {
            return false;
        }

        // Either on IEndpointRouteBuilder or in EndpointRouteBuilderExtensions
        return symbol.ReceiverType?.Name == "IEndpointRouteBuilder"
            || symbol.ContainingType.Name.Contains("EndpointRouteBuilderExtensions")
            || (
                symbol.Parameters.Length > 0
                && symbol.Parameters[0].Type.Name == "IEndpointRouteBuilder"
            );
    }

    private static bool IsHttpResultType(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString();
        // Already an EndpointResult or IResult
        if (
            fullName.Contains("LightningArc.Results.AspNetCore.EndpointResult")
            || fullName.Contains("Microsoft.AspNetCore.Http.IResult")
        )
        {
            return true;
        }

        if (
            type.AllInterfaces.Any(i =>
                i.ToDisplayString().Contains("Microsoft.AspNetCore.Http.IResult")
            )
        )
        {
            return true;
        }

        return type.BaseType != null && IsHttpResultType(type.BaseType);
    }
}
