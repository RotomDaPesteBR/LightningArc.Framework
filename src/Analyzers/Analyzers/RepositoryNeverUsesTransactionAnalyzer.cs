using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC045 - Detects a non-abstract <c>RepositoryBase</c>-derived class that makes
/// at least one database-operation call but never references
/// <c>RepositoryBase.Transaction</c> anywhere in the type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RepositoryNeverUsesTransactionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC045";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Repository never uses Transaction",
        messageFormat: "Repository '{0}' never passes Transaction, so it cannot participate in unit-of-work mode",
        category: DiagnosticCategory.Reliability,
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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            RepositoryTypeRecognizer recognizer = RepositoryTypeRecognizer.Resolve(
                compilationContext.Compilation
            );
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeClass(nodeContext, recognizer),
                SyntaxKind.ClassDeclaration
            );
        });
    }

    private static void AnalyzeClass(
        SyntaxNodeAnalysisContext context,
        RepositoryTypeRecognizer recognizer
    )
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
        {
            return;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol == null || !recognizer.InheritsFromRepositoryBase(classSymbol))
        {
            return;
        }

        bool hasDbOperation = false;
        foreach (InvocationExpressionSyntax invocation in classDecl
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>())
        {
            if (
                context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
                && DatabaseOperationRecognizer.IsDatabaseOperation(methodSymbol)
            )
            {
                hasDbOperation = true;
                break;
            }
        }

        if (!hasDbOperation)
        {
            return;
        }

        if (ReferencesTransaction(classDecl, context.SemanticModel, recognizer))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(Rule, classDecl.Identifier.GetLocation(), classSymbol.Name)
        );
    }

    private static bool ReferencesTransaction(
        ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel,
        RepositoryTypeRecognizer recognizer
    )
    {
        ISymbol? transactionField = recognizer.RepositoryBaseType
            ?.GetMembers("Transaction")
            .FirstOrDefault();
        if (transactionField == null)
        {
            // Fail open: without a resolvable Transaction member there is
            // nothing to compare references against, so stay silent rather
            // than flagging every repository in the compilation.
            return true;
        }

        foreach (IdentifierNameSyntax identifier in classDecl
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>())
        {
            if (identifier.Identifier.ValueText != "Transaction")
            {
                continue;
            }

            ISymbol? symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol != null && SymbolEqualityComparer.Default.Equals(symbol, transactionField))
            {
                return true;
            }
        }

        return false;
    }
}
