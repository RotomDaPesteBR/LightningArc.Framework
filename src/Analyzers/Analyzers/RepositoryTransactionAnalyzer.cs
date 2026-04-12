using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC021 - Detects when a repository method calls a database operation
/// without passing the 'Transaction' property from RepositoryBase.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RepositoryTransactionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC021";
    public const string HelpLinkBase = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing transaction in repository call",
        messageFormat: "Database operation '{0}' is called without passing the available 'Transaction' property. This may lead to operations running outside the intended transaction.",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private static readonly string[] DapperMethods = 
    [
        "Query", "QueryAsync", "Execute", "ExecuteAsync", 
        "QuerySingle", "QuerySingleAsync", "QueryFirst", "QueryFirstAsync",
        "QueryFirstOrDefault", "QueryFirstOrDefaultAsync", "QuerySingleOrDefault", "QuerySingleOrDefaultAsync"
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        
        // 1. Are we inside a RepositoryBase?
        var classDecl = invocation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDecl == null) return;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol == null || !InheritsFromRepositoryBase(classSymbol)) return;

        // 2. Is this a database operation (Dapper or DbConnection)?
        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null) return;

        if (!IsDatabaseOperation(methodSymbol)) return;

        // 3. Does it have a transaction parameter?
        var transactionParam = methodSymbol.Parameters.FirstOrDefault(p => 
            p.Name.Equals("transaction", System.StringComparison.OrdinalIgnoreCase) || 
            p.Type.Name.Contains("DbTransaction"));
        
        if (transactionParam == null) return;

        // 4. Is the transaction argument provided?
        if (!IsTransactionArgumentProvided(invocation, methodSymbol, context.SemanticModel))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name));
        }
    }

    private static bool InheritsFromRepositoryBase(INamedTypeSymbol symbol)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.Name == "RepositoryBase") return true;
            current = current.BaseType;
        }
        return false;
    }

    private static bool IsDatabaseOperation(IMethodSymbol method)
    {
        // Check Dapper extension methods
        if (DapperMethods.Contains(method.Name)) return true;

        // Check DbConnection methods
        if (method.ContainingType != null && 
            (method.ContainingType.Name == "DbConnection" || method.ContainingType.Name == "IDbConnection"))
        {
            return true;
        }

        return false;
    }

    private static bool IsTransactionArgumentProvided(InvocationExpressionSyntax invocation, IMethodSymbol method, SemanticModel semanticModel)
    {
        // Check for named argument "transaction"
        var hasNamedTransaction = invocation.ArgumentList.Arguments.Any(a => 
            a.NameColon != null && a.NameColon.Name.Identifier.ValueText == "transaction");
        if (hasNamedTransaction) return true;

        // Check for positional argument
        var transactionParamIndex = method.Parameters.ToList().FindIndex(p => 
            p.Name.Equals("transaction", System.StringComparison.OrdinalIgnoreCase));
        
        if (transactionParamIndex >= 0 && invocation.ArgumentList.Arguments.Count > transactionParamIndex)
        {
            // Simplified check: if there is an argument at that position, assume it's the transaction
            // (Unless it's explicitly null, but even then the user "provided" it)
            return true;
        }

        return false;
    }
}
