using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LightningArc.Analyzers;

/// <summary>
/// LARC044 - Detects a <c>GetConnection()</c>/<c>GetConnectionAsync()</c> acquisition with no
/// matching <c>ReleaseConnection(...)</c> call in a <c>finally</c> block within the same method.
/// </summary>
/// <remarks>
/// <c>RepositoryBase</c>'s acquire/release contract is <c>try { ... GetConnection[Async] ... }
/// finally { ReleaseConnection(...) }</c> — this is the pattern used throughout the library's
/// own repositories. Disposing the acquired connection directly (e.g. wrapping it in a
/// <c>using</c>) is not a safe alternative: <c>ReleaseConnection</c> deliberately no-ops when
/// the connection came from an externally-provided <see cref="System.Data.Common.DbConnection"/>
/// or an active <see cref="System.Data.Common.DbTransaction"/> (unit-of-work mode), so that the
/// repository never closes a connection it doesn't own. A bare <c>using</c> on the acquired
/// connection would close it unconditionally, breaking unit-of-work mode. <c>ReleaseConnection</c>
/// is therefore the one correct disposal path in both modes, which is why its absence is flagged
/// with high confidence rather than treated as one of several acceptable patterns.
///
/// Scope limitation: this only looks within the same method body as the acquisition. A
/// repository that splits "acquire" and "use + release" across two methods (passing the
/// connection between them) will false-positive here — this is a deliberate, documented
/// trade-off for staying purely syntactic rather than attempting call-graph analysis, the same
/// scoping choice made for <see cref="ResultAccessSafetyRecognizer.IsGuardedByPrecedingExit"/>.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingReleaseConnectionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "LARC044";
    public const string HelpLinkBase =
        "https://github.com/RotomDaPesteBR/LightningArc.Framework/blob/main/docs/analyzers/";

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing ReleaseConnection after GetConnection",
        messageFormat: "'{0}' is called without a matching ReleaseConnection(...) call in a finally block within this method. This may leak the connection when it was acquired via the connection factory.",
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
                nodeContext => AnalyzeInvocation(nodeContext, recognizer),
                SyntaxKind.InvocationExpression
            );
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        RepositoryTypeRecognizer recognizer
    )
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        var methodSymbol =
            context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol
            as IMethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        if (methodSymbol.Name != "GetConnection" && methodSymbol.Name != "GetConnectionAsync")
        {
            return;
        }

        // Same equality-or-inherits check as LARC043's ReleaseConnection verification:
        // GetConnection/GetConnectionAsync are declared directly on RepositoryBase, so
        // methodSymbol.ContainingType resolves to RepositoryBase itself, not a derived class.
        if (!IsRepositoryBaseMethod(methodSymbol.ContainingType, recognizer))
        {
            return;
        }

        MethodDeclarationSyntax? enclosingMethod = invocation
            .Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();
        if (enclosingMethod == null)
        {
            // Acquired outside a regular method body (e.g. a constructor, field initializer, or
            // local function) — outside this rule's intentionally narrow scope.
            return;
        }

        if (HasReleaseConnectionInFinally(enclosingMethod, context.SemanticModel, recognizer))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name)
        );
    }

    private static bool IsRepositoryBaseMethod(
        INamedTypeSymbol? containingType,
        RepositoryTypeRecognizer recognizer
    )
    {
        return recognizer.RepositoryBaseType != null
            && containingType != null
            && (
                SymbolEqualityComparer.Default.Equals(containingType, recognizer.RepositoryBaseType)
                || recognizer.InheritsFromRepositoryBase(containingType)
            );
    }

    private static bool HasReleaseConnectionInFinally(
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        RepositoryTypeRecognizer recognizer
    )
    {
        foreach (TryStatementSyntax tryStatement in method.DescendantNodes().OfType<TryStatementSyntax>())
        {
            if (tryStatement.Finally == null)
            {
                continue;
            }

            bool callsRelease = tryStatement
                .Finally.Block.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(candidate => IsReleaseConnectionCall(candidate, semanticModel, recognizer));

            if (callsRelease)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsReleaseConnectionCall(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        RepositoryTypeRecognizer recognizer
    )
    {
        var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol == null || methodSymbol.Name != "ReleaseConnection")
        {
            return false;
        }

        return IsRepositoryBaseMethod(methodSymbol.ContainingType, recognizer);
    }
}
