using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC032 - Detects redundant try-catch blocks that only map exceptions to generic Result errors.
/// These should be handled by the global ResultExceptionHandler instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RedundantResultTryCatchAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC032";
    public const string HelpLinkBase = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Redundant try-catch for Result mapping",
        messageFormat: "This try-catch block is redundant. The global ResultExceptionHandler already maps unhandled exceptions to standardized errors.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Fully-qualified metadata name of the ASP.NET Core middleware this rule assumes is
    /// active. Adjust if the actual namespace/type differs — this must match exactly for the
    /// compilation-reference gate below to work.
    /// </summary>
    private const string ResultExceptionHandlerMetadataName = "LightningArc.Results.AspNetCore.ResultExceptionHandler";

    private const string ErrorTypeMetadataName = "LightningArc.Results.Error";
    private const string ResultTypeMetadataName = "LightningArc.Results.Result";
    private const string ResultsNamespacePrefix = "LightningArc.Results";

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            Compilation compilation = compilationContext.Compilation;

            // If this compilation cannot even see the middleware type, the middleware cannot
            // possibly be covering any code in it — the rule's entire premise doesn't apply,
            // regardless of what shape a given try/catch has. This is the common case for
            // LightningArc.Results itself and any consumer that doesn't reference
            // LightningArc.Results.AspNetCore (e.g. non-web apps, background services,
            // library code below the web layer by design).
            INamedTypeSymbol? handlerType = compilation.GetTypeByMetadataName(ResultExceptionHandlerMetadataName);

            if (handlerType is null)
            {
                return;
            }

            INamedTypeSymbol? errorType = compilation.GetTypeByMetadataName(ErrorTypeMetadataName);
            INamedTypeSymbol? resultType = compilation.GetTypeByMetadataName(ResultTypeMetadataName);

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeTryStatement(nodeContext, errorType, resultType),
                SyntaxKind.TryStatement
            );
        });
    }

    private static void AnalyzeTryStatement(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? errorType,
        INamedTypeSymbol? resultType
    )
    {
        var tryStatement = (TryStatementSyntax)context.Node;

        // We are looking for try-catch blocks with a single catch clause
        if (tryStatement.Catches.Count != 1) return;

        var catchClause = tryStatement.Catches[0];

        // 1. Is it catching a broad exception? (Exception or nothing)
        if (!IsBroadCatch(catchClause, context.SemanticModel)) return;

        // 2. Does the catch block only return an Error/Result?
        if (!IsSimpleErrorReturn(catchClause.Block, context.SemanticModel, errorType, resultType)) return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, tryStatement.TryKeyword.GetLocation()));
    }

    private static bool IsBroadCatch(CatchClauseSyntax catchClause, SemanticModel semanticModel)
    {
        if (catchClause.Declaration == null) return true; // catch { ... }

        var typeInfo = semanticModel.GetTypeInfo(catchClause.Declaration.Type);
        if (typeInfo.Type == null) return false;

        return typeInfo.Type.Name == "Exception" && 
               (typeInfo.Type.ContainingNamespace?.Name == "System" || typeInfo.Type.ContainingNamespace == null);
    }

    private static bool IsSimpleErrorReturn(
        BlockSyntax block,
        SemanticModel semanticModel,
        INamedTypeSymbol? errorType,
        INamedTypeSymbol? resultType
    )
    {
        // Must have exactly one statement
        if (block.Statements.Count != 1) return false;

        var statement = block.Statements[0];

        // That statement must be a return
        if (statement is not ReturnStatementSyntax returnStmt || returnStmt.Expression == null) return false;

        // The expression must be an Error creation or Result.Failure — compared against the
        // actual LightningArc.Results types by symbol, not by bare name, so an unrelated
        // "Error" or "Result" type elsewhere in the compilation can't trigger a false match.
        var typeInfo = semanticModel.GetTypeInfo(returnStmt.Expression);
        if (typeInfo.Type == null) return false;

        bool isError = IsErrorType(typeInfo.Type, errorType);
        bool isResult = IsResultType(typeInfo.Type, resultType);

        if (!isError && !isResult) return false;

        // Check if there are any other operations like logging inside the return (e.g. return LogAndReturn(ex))
        // For simplicity, if it's an invocation of Error.Application.Internal or similar, we consider it simple.
        if (returnStmt.Expression is InvocationExpressionSyntax invocation)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null) return false;

            // Anchor to the actual LightningArc.Results namespace so an unrelated type named
            // "Application"/"Validation" (or a method named "Failure") elsewhere in the
            // compilation can't be mistaken for one of our Error factory methods.
            string? containingNamespace = symbol.ContainingType?.ContainingNamespace?.ToDisplayString();
            if (containingNamespace == null || !containingNamespace.StartsWith(ResultsNamespacePrefix, StringComparison.Ordinal))
            {
                return false;
            }

            return symbol.ContainingType!.Name == "Application" ||
                   symbol.ContainingType.Name == "Validation" ||
                   symbol.Name == "Internal" ||
                   symbol.Name == "Failure";
        }

        return true;
    }

    private static bool IsErrorType(ITypeSymbol candidate, INamedTypeSymbol? errorType)
    {
        if (errorType == null) return false;

        for (ITypeSymbol? current = candidate; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, errorType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsResultType(ITypeSymbol candidate, INamedTypeSymbol? resultType)
    {
        if (resultType == null) return false;

        // Result<TValue> : Result, so walking the base-type chain (same pattern as
        // IsErrorType/AggregateError) covers both the non-generic Result and any Result<T>
        // instantiation with a single check — no separate generic-definition lookup needed.
        for (ITypeSymbol? current = candidate; current != null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, resultType))
            {
                return true;
            }
        }

        return false;
    }
}
