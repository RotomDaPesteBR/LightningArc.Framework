using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC020 - Detects calls to GetConnection() (non-async version) inside
/// an async method within classes that inherit RepositoryBase.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SyncConnectionInAsyncMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC020";
    private const string HelpLinkBase = "https://github.com/RotomDaPesteBR/Utils/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Synchronous GetConnection in async method",
        messageFormat: "Synchronous GetConnection() called in async method. Use GetConnectionAsync() instead.",
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

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        // Get the method being called
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Check if the method is named GetConnection (exactly, not GetConnectionAsync)
        if (methodSymbol.Name != "GetConnection")
            return;

        // Find the enclosing method declaration
        var method = invocation.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (method == null)
            return;

        // Check if the enclosing method is async
        if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
            return;

        // Check if the enclosing class derives from RepositoryBase
        var classDeclaration = method.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration == null)
            return;

        if (!DerivesFromRepositoryBase(classDeclaration, context.SemanticModel))
            return;

        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool DerivesFromRepositoryBase(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (symbol == null)
            return false;

        var current = symbol.BaseType;
        while (current != null)
        {
            var fullName = current.ToDisplayString();
            if (fullName.Contains("RepositoryBase"))
                return true;

            if (current.Name.Contains("RepositoryBase"))
                return true;

            current = current.BaseType;
        }

        return false;
    }
}
