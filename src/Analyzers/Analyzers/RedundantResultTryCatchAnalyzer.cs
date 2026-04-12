using System.Collections.Immutable;
using System.Linq;
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

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTryStatement, SyntaxKind.TryStatement);
    }

    private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
    {
        var tryStatement = (TryStatementSyntax)context.Node;

        // We are looking for try-catch blocks with a single catch clause
        if (tryStatement.Catches.Count != 1) return;

        var catchClause = tryStatement.Catches[0];

        // 1. Is it catching a broad exception? (Exception or nothing)
        if (!IsBroadCatch(catchClause, context.SemanticModel)) return;

        // 2. Does the catch block only return an Error/Result?
        if (!IsSimpleErrorReturn(catchClause.Block, context.SemanticModel)) return;

        // 3. Optional: Check if we are inside a context that supports global handling (Web/Controller)
        // For now, let's keep it generic as it's a good practice everywhere in this framework.

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

    private static bool IsSimpleErrorReturn(BlockSyntax block, SemanticModel semanticModel)
    {
        // Must have exactly one statement
        if (block.Statements.Count != 1) return false;

        var statement = block.Statements[0];
        
        // That statement must be a return
        if (statement is not ReturnStatementSyntax returnStmt || returnStmt.Expression == null) return false;

        // The expression must be an Error creation or Result.Failure
        var typeInfo = semanticModel.GetTypeInfo(returnStmt.Expression);
        if (typeInfo.Type == null) return false;

        bool isError = typeInfo.Type.Name == "Error" || (typeInfo.Type.BaseType != null && typeInfo.Type.BaseType.Name == "Error");
        bool isResult = typeInfo.Type.Name == "Result" || typeInfo.Type.AllInterfaces.Any(i => i.Name.StartsWith("IResult"));

        if (!isError && !isResult) return false;

        // Check if there are any other operations like logging inside the return (e.g. return LogAndReturn(ex))
        // For simplicity, if it's an invocation of Error.Application.Internal or similar, we consider it simple.
        if (returnStmt.Expression is InvocationExpressionSyntax invocation)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null) return false;

            // If it's a factory method from our Error modules
            return symbol.ContainingType.Name == "Application" || 
                   symbol.ContainingType.Name == "Validation" ||
                   symbol.Name == "Internal" ||
                   symbol.Name == "Failure";
        }

        return true;
    }
}
