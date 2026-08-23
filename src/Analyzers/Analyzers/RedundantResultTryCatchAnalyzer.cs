using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC006 - Detects redundant try-catch blocks that only map exceptions to generic Result errors.
/// These should be handled by the global ResultExceptionHandler instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RedundantResultTryCatchAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC006";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Redundant try-catch for Result mapping",
        messageFormat: "This try-catch block is redundant. The global ResultExceptionHandler already maps unhandled exceptions to standardized errors.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Fully-qualified metadata name of the ASP.NET Core middleware this rule assumes is
    /// active. Adjust if the actual namespace/type differs — this must match exactly for the
    /// compilation-reference gate below to work.
    /// </summary>
    private const string ResultExceptionHandlerMetadataName =
        "LightningArc.Results.AspNetCore.ResultExceptionHandler";

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
            INamedTypeSymbol? handlerType = compilation.GetTypeByMetadataName(
                ResultExceptionHandlerMetadataName
            );

            if (handlerType is null)
            {
                return;
            }

            ResultTypeRecognizer recognizer = ResultTypeRecognizer.Resolve(compilation);
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeTryStatement(nodeContext, recognizer),
                SyntaxKind.TryStatement
            );
        });
    }

    private static void AnalyzeTryStatement(
        SyntaxNodeAnalysisContext context,
        ResultTypeRecognizer recognizer
    )
    {
        TryStatementSyntax tryStatement = (TryStatementSyntax)context.Node;

        // We are looking for try-catch blocks with a single catch clause
        if (tryStatement.Catches.Count != 1)
        {
            return;
        }

        var catchClause = tryStatement.Catches[0];

        // 1. Is it catching a broad exception? (Exception or nothing)
        if (!IsBroadCatch(catchClause, context.SemanticModel))
        {
            return;
        }

        // 2. Does the catch block only return an Error/Result?
        if (!IsSimpleErrorReturn(catchClause.Block, context.SemanticModel, recognizer))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, tryStatement.TryKeyword.GetLocation()));
    }

    private static bool IsBroadCatch(CatchClauseSyntax catchClause, SemanticModel semanticModel)
    {
        if (catchClause.Declaration == null)
        {
            return true; // catch { ... }
        }

        var typeInfo = semanticModel.GetTypeInfo(catchClause.Declaration.Type);
        if (typeInfo.Type == null)
        {
            return false;
        }

        return typeInfo.Type.Name == "Exception"
            && (
                typeInfo.Type.ContainingNamespace?.Name == "System"
                || typeInfo.Type.ContainingNamespace == null
            );
    }

    private static bool IsSimpleErrorReturn(
        BlockSyntax block,
        SemanticModel semanticModel,
        ResultTypeRecognizer recognizer
    )
    {
        // Must have exactly one statement
        if (block.Statements.Count != 1)
        {
            return false;
        }

        var statement = block.Statements[0];

        // That statement must be a return
        if (statement is not ReturnStatementSyntax returnStmt || returnStmt.Expression == null)
        {
            return false;
        }

        // The expression must be an Error creation or Result.Failure — compared against the
        // actual LightningArc.Results types by symbol, not by bare name, so an unrelated
        // "Error" or "Result" type elsewhere in the compilation can't trigger a false match.
        var typeInfo = semanticModel.GetTypeInfo(returnStmt.Expression);
        if (typeInfo.Type == null)
        {
            return false;
        }

        bool isResult =
            recognizer.IsResultType(typeInfo.Type) || recognizer.IsErrorType(typeInfo.Type);

        if (!isResult)
        {
            return false;
        }

        // Check if there are any other operations like logging inside the return (e.g. return LogAndReturn(ex))
        // For simplicity, if it's an invocation of Error.Application.Internal or similar, we consider it simple.
        if (returnStmt.Expression is InvocationExpressionSyntax invocation)
        {
            if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol)
            {
                return false;
            }

            // Anchor to the actual LightningArc.Results namespace so an unrelated type named
            // "Application"/"Validation" (or a method named "Failure") elsewhere in the
            // compilation can't be mistaken for one of our Error factory methods.
            string? containingNamespace =
                symbol.ContainingType?.ContainingNamespace?.ToDisplayString();
            if (
                containingNamespace == null
                || !containingNamespace.StartsWith(ResultsNamespacePrefix, StringComparison.Ordinal)
            )
            {
                return false;
            }

            return symbol.ContainingType!.Name == "Application"
                || symbol.ContainingType.Name == "Validation"
                || symbol.Name == "Internal"
                || symbol.Name == "Failure";
        }

        return true;
    }
}
