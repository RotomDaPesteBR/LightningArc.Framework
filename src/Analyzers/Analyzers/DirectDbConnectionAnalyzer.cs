using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC042 - Detects direct instantiation of DbConnection types inside repositories.
/// Repositories should use IConnectionFactory or GetConnection() from RepositoryBase.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DirectDbConnectionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC042";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Direct DbConnection instantiation in repository",
        messageFormat: "DbConnection type '{0}' is instantiated directly inside a repository. Use 'GetConnection()' or 'GetConnectionAsync()' to leverage centralized connection management.",
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
            RepositoryTypeRecognizer recognizer = RepositoryTypeRecognizer.Resolve(
                compilationContext.Compilation
            );
            if (!recognizer.IsAvailable)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeObjectCreation(nodeContext, recognizer),
                SyntaxKind.ObjectCreationExpression
            );
        });
    }

    private static void AnalyzeObjectCreation(
        SyntaxNodeAnalysisContext context,
        RepositoryTypeRecognizer recognizer
    )
    {
        ObjectCreationExpressionSyntax objectCreation = (ObjectCreationExpressionSyntax)
            context.Node;

        // 1. Are we inside a RepositoryBase?
        var classDecl = objectCreation
            .Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();
        if (classDecl == null)
        {
            return;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (classSymbol == null || !recognizer.InheritsFromRepositoryBase(classSymbol))
        {
            return;
        }

        // 2. Is this a DbConnection type?
        var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
        if (typeInfo.Type == null)
        {
            return;
        }

        if (IsDbConnectionType(typeInfo.Type))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, objectCreation.GetLocation(), typeInfo.Type.Name)
            );
        }
    }

    private static bool IsDbConnectionType(ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            // Full metadata name for System.Data.Common.DbConnection
            if (current.Name == "DbConnection" || current.Name == "IDbConnection")
            {
                return true;
            }

            current = current.BaseType;
        }
        return false;
    }
}
