using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC022 - Detects classes implementing IHostedService where StartAsync
/// body only consists of return Task.CompletedTask.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NoOpHostedServiceAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC022";
    public const string HelpLinkBase = "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "HostedService StartAsync performs no work",
        messageFormat: "HostedService StartAsync performs no work",
        category: DiagnosticCategory.Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: DiagnosticCategory.EditAndContinueTags,
        helpLinkUri: HelpLinkBase + DiagnosticId + ".md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        // Check if method is named StartAsync
        string methodName = methodDeclaration.Identifier.ValueText;
        if (methodName != "StartAsync")
        {
            return;
        }

        // Check if we are inside a class that implements IHostedService
        ClassDeclarationSyntax? classDeclaration = methodDeclaration.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration == null)
        {
            return;
        }

        if (!ImplementsIHostedService(classDeclaration, context.SemanticModel))
        {
            return;
        }

        // Check if the method body only returns Task.CompletedTask with no other work
        if (!IsNoOpStartAsync(methodDeclaration))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ImplementsIHostedService(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (symbol == null)
        {
            return false;
        }

        foreach (INamedTypeSymbol? iface in symbol.AllInterfaces)
        {
            if (iface.Name == "IHostedService")
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNoOpStartAsync(MethodDeclarationSyntax method)
    {
        // Check for arrow-bodied: => Task.CompletedTask;
        if (method.ExpressionBody != null)
        {
            return IsTaskCompletedTask(method.ExpressionBody.Expression);
        }

        // Check block-bodied method
        if (method.Body == null)
        {
            return false;
        }

        var statements = method.Body.Statements;

        // A single return statement returning Task.CompletedTask
        if (statements.Count == 1 && statements[0] is ReturnStatementSyntax returnStatement)
        {
            return IsTaskCompletedTask(returnStatement.Expression);
        }

        return false;
    }

    private static bool IsTaskCompletedTask(ExpressionSyntax? expression)
    {
        if (expression == null)
        {
            return false;
        }

        if (expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.ValueText == "CompletedTask")
        {
            string container = memberAccess.Expression.ToString();
            return container == "Task" ||
                   container == "System.Threading.Tasks.Task" ||
                   container == "ValueTask" ||
                   container == "System.Threading.Tasks.ValueTask";
        }

        return false;
    }
}
