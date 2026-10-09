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
/// The directly-returned case (see <see cref="IsDirectlyReturned"/>) and its
/// local-mediated variant <c>var c = GetConnection(...); return c;</c> (see
/// <see cref="IsLocalForward"/>) are instances of this same limitation handled
/// explicitly, since they show up inside RepositoryBase's own forwarding
/// overloads.
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
        category: DiagnosticCategory.Reliability,
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

        // A call whose result is immediately returned (e.g. the zero-arg GetConnection()
        // overload forwarding to GetConnection(IConnectionFactory?)) hands the connection to
        // its own caller rather than consuming it here — there is nothing for *this* method to
        // release. Flagging it would mean every forwarding/delegating overload of
        // GetConnection/GetConnectionAsync permanently false-positives, including the ones
        // declared on RepositoryBase itself. This is the same "can't trace across method
        // boundaries" limitation already documented on this analyzer, applied to the specific
        // case where the boundary is crossed via direct return rather than a stored variable.
        if (IsDirectlyReturned(invocation))
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

        // Local-mediated forwarding: `var c = GetConnection(...); return c;` where the local
        // is declared in this method, returned once, and never otherwise used. Same rationale
        // as IsDirectlyReturned — the method hands the connection to its caller rather than
        // consuming it here. Any additional use (passed to another call, read twice, stored in
        // a field, ...) means the method does consume/observe the connection, so the
        // exclusion does not apply. Cross-method acquire/release (e.g. stored in a field and
        // released from another method) is deliberately NOT excluded.
        if (IsLocalForward(invocation, enclosingMethod, context.SemanticModel))
        {
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

    /// <summary>
    /// True when <paramref name="invocation"/>'s result flows directly into a <c>return</c>
    /// (either <c>return GetConnection(...);</c> or an arrow-expression body
    /// <c>=&gt; GetConnection(...);</c>), optionally through an <c>await</c>. In this shape the
    /// enclosing method is forwarding the connection to its own caller, not acquiring one it
    /// intends to use and release itself.
    /// </summary>
    private static bool IsDirectlyReturned(InvocationExpressionSyntax invocation)
    {
        SyntaxNode? parent = invocation.Parent;

        // Unwrap `return await GetConnectionAsync(...);` / `=> await GetConnectionAsync(...);`
        if (parent is AwaitExpressionSyntax awaitExpression)
        {
            parent = awaitExpression.Parent;
        }

        return parent is ReturnStatementSyntax or ArrowExpressionClauseSyntax;
    }

    /// <summary>
    /// True when <paramref name="invocation"/>'s result is stored in a local that is declared in
    /// <paramref name="method"/>, returned exactly once (<c>return c;</c>), and never otherwise
    /// used — i.e. the local-mediated shape <c>var c = GetConnection(...); return c;</c>. The
    /// initializer may be awaited (<c>var c = await GetConnectionAsync(...);</c>).
    /// </summary>
    private static bool IsLocalForward(
        InvocationExpressionSyntax invocation,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel
    )
    {
        SyntaxNode? initializer = invocation;

        // Unwrap `var c = await GetConnectionAsync(...);`
        if (initializer.Parent is AwaitExpressionSyntax awaitExpression)
        {
            initializer = awaitExpression;
        }

        if (initializer.Parent is not EqualsValueClauseSyntax equalsClause)
        {
            return false;
        }

        if (
            equalsClause.Parent is not VariableDeclaratorSyntax declarator
            || declarator.Parent is not VariableDeclarationSyntax declaration
            || declaration.Parent is not LocalDeclarationStatementSyntax
        )
        {
            return false;
        }

        ISymbol? localSymbol = semanticModel.GetDeclaredSymbol(declarator);
        if (localSymbol == null)
        {
            return false;
        }

        List<IdentifierNameSyntax> references = method
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(candidate =>
                candidate.Identifier.ValueText == declarator.Identifier.ValueText
                && SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetSymbolInfo(candidate).Symbol,
                    localSymbol
                )
            )
            .ToList();

        // Exactly one use, and that use is the operand of `return c;`.
        return references.Count == 1
            && references[0].Parent is ReturnStatementSyntax returnStatement
            && returnStatement.Expression == references[0];
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
