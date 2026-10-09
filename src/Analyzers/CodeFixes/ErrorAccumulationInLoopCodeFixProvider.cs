using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

/// <summary>
/// Code fix for LARC008 - rewrites <c>errors += e;</c> accumulation inside a loop
/// into a single-pass batch form: a <c>List&lt;Error&gt;</c> declared before the loop,
/// <c>Add</c> calls inside it, and one <c>Error.Aggregate(list)</c> assignment after
/// the loop. Offered only when the accumulator is a nullable local declared outside
/// the loop and every other reference to it sits after the loop; otherwise no fix is
/// offered. Fix-All is deliberately disabled: each loop rewrite needs its own review.
/// </summary>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(ErrorAccumulationInLoopCodeFixProvider)
)]
[Shared]
public class ErrorAccumulationInLoopCodeFixProvider : CodeFixProvider
{
    private const string _title = "Collect into a list and aggregate once";

    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        [ErrorAccumulationInLoopAnalyzer.DiagnosticId];

    // Deliberately no Fix-All: a multi-statement loop rewrite is not safely
    // batchable without per-site review.
    public sealed override FixAllProvider? GetFixAllProvider() => null;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context
            .Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        // Diagnostic location is the `errors += e` assignment itself.
        TextSpan diagnosticSpan = context.Diagnostics[0].Location.SourceSpan;
        SyntaxNode? node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        AssignmentExpressionSyntax? accumulation =
            node as AssignmentExpressionSyntax
            ?? node?.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().FirstOrDefault();

        if (
            accumulation == null
            || !accumulation.IsKind(SyntaxKind.AddAssignmentExpression)
            || accumulation.Left is not IdentifierNameSyntax accumulator
        )
        {
            return;
        }

        SemanticModel? semanticModel = await context
            .Document.GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (semanticModel == null)
        {
            return;
        }

        RewritePlan? plan = TryBuildPlan(accumulation, accumulator, semanticModel);
        if (plan == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: c => ApplyAsync(context.Document, plan, c),
                equivalenceKey: nameof(ErrorAccumulationInLoopCodeFixProvider)
            ),
            context.Diagnostics[0]
        );
    }

    private sealed record RewritePlan(
        StatementSyntax Loop,
        List<ExpressionStatementSyntax> Accumulations,
        VariableDeclaratorSyntax AccumulatorDeclaration,
        TypeSyntax ErrorType,
        bool AccumulatorIsNullable,
        string ListName
    );

    private static RewritePlan? TryBuildPlan(
        AssignmentExpressionSyntax accumulation,
        IdentifierNameSyntax accumulator,
        SemanticModel semanticModel
    )
    {
        // The accumulator must be a plain local — fields/properties/parameters
        // cannot be reasoned about statement-locally.
        if (
            semanticModel.GetSymbolInfo(accumulator).Symbol is not ILocalSymbol local
            || local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
                is not VariableDeclaratorSyntax declarator
        )
        {
            return null;
        }

        // The loop being rewritten: nearest loop ancestor of the flagged `+=`.
        StatementSyntax? loop = accumulation
            .Ancestors()
            .OfType<StatementSyntax>()
            .FirstOrDefault(s =>
                s
                    is ForStatementSyntax
                        or ForEachStatementSyntax
                        or ForEachVariableStatementSyntax
                        or WhileStatementSyntax
                        or DoStatementSyntax
            );

        if (loop == null)
        {
            return null;
        }

        // The rewrite inserts statements immediately before/after the loop,
        // so the loop must sit directly in a block's statement list.
        if (loop.Parent is not BlockSyntax)
        {
            return null;
        }

        // The declaration must sit outside (and textually before) the loop.
        if (
            loop.DescendantNodesAndSelf().Contains(declarator)
            || declarator.SpanStart >= loop.SpanStart
        )
        {
            return null;
        }

        // Scope for reference analysis: the enclosing method/ctor/local function.
        SyntaxNode? scope = loop
            .Ancestors()
            .FirstOrDefault(a =>
                a
                    is MethodDeclarationSyntax
                        or ConstructorDeclarationSyntax
                        or LocalFunctionStatementSyntax
            );

        if (scope == null)
        {
            return null;
        }

        List<ExpressionStatementSyntax> inLoopAccumulations = [];
        foreach (IdentifierNameSyntax reference in scope
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
        )
        {
            if (
                !SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetSymbolInfo(reference).Symbol,
                    local
                )
            )
            {
                continue;
            }

            // The declaration itself is always fine.
            if (reference.Parent == declarator)
            {
                continue;
            }

            // A `acc += x;` inside the loop being rewritten becomes `list.Add(x);`.
            if (
                reference.Parent is AssignmentExpressionSyntax assignment
                && assignment.Left == reference
                && assignment.IsKind(SyntaxKind.AddAssignmentExpression)
                && assignment.Parent is ExpressionStatementSyntax statement
                && loop.DescendantNodesAndSelf().Contains(statement)
            )
            {
                // The flagged `+=` must be a standalone statement in the loop's own
                // executable flow — not deferred inside a nested lambda/local function.
                ExpressionStatementSyntax? stmt =
                    reference.Parent?.Parent as ExpressionStatementSyntax;
                if (stmt == null || IsDeferred(stmt, loop))
                {
                    return null;
                }

                if (!inLoopAccumulations.Contains(stmt))
                {
                    inLoopAccumulations.Add(stmt);
                }

                continue;
            }

            // Any write to the accumulator outside this loop (a second loop's
            // `+=`, a reset before/after, ...) makes the rewrite unsound.
            if (
                reference.Parent is AssignmentExpressionSyntax otherAssignment
                && otherAssignment.Left == reference
            )
            {
                return null;
            }

            // Reads are only safe after the loop; anything before or inside it
            // observes the intermediate value the rewrite removes.
            if (reference.SpanStart < loop.Span.End)
            {
                return null;
            }
        }

        if (inLoopAccumulations.Count == 0)
        {
            return null;
        }

        // Derive the Error type spelling from the declaration so aliases and
        // qualifications keep working; remember nullability for the assignment.
        if (declarator.Parent is not VariableDeclarationSyntax declaration)
        {
            return null;
        }

        TypeSyntax errorType = declaration.Type;
        bool isNullable = false;
        if (errorType is NullableTypeSyntax nullable)
        {
            errorType = nullable.ElementType;
            isNullable = true;
        }

        string listName = UniqueName(
            scope,
            accumulator.Identifier.ValueText + "List"
        );

        return new RewritePlan(
            loop,
            inLoopAccumulations,
            declarator,
            errorType.WithoutTrivia(),
            isNullable,
            listName
        );
    }

    private static bool IsDeferred(StatementSyntax statement, StatementSyntax loop)
    {
        return statement
            .Ancestors()
            .TakeWhile(a => a != loop)
            .Any(a =>
                a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax
            );
    }

    private static string UniqueName(SyntaxNode scope, string baseName)
    {
        HashSet<string> taken = new(
            scope
                .DescendantNodesAndSelf()
                .OfType<IdentifierNameSyntax>()
                .Select(id => id.Identifier.ValueText)
                .Concat(
                    scope
                        .DescendantNodesAndSelf()
                        .OfType<VariableDeclaratorSyntax>()
                        .Select(d => d.Identifier.ValueText)
                )
        );

        string candidate = baseName;
        int suffix = 2;
        while (taken.Contains(candidate))
        {
            candidate = baseName + suffix;
            suffix++;
        }

        return candidate;
    }

    /// <summary>
    /// The indentation (whitespace only) preceding <paramref name="node"/>.
    /// Roslyn keeps end-of-line trivia in the <i>preceding</i> token's trailing
    /// trivia, so a statement's leading trivia is just its indent — copying it
    /// verbatim puts an inserted sibling on its own correctly-indented line.
    /// </summary>
    private static SyntaxTriviaList LeadingIndent(SyntaxNode node) =>
        TriviaList(
            node
                .GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
        );

    /// <summary>
    /// The document's newline flavor, sniffed from its first end-of-line
    /// trivia (LF in test documents, CRLF in real files). Constructed line
    /// breaks use it so no later formatter pass has a gap to "fix" with a
    /// platform-default newline.
    /// </summary>
    private static SyntaxTrivia DocumentEndOfLine(SyntaxNode root)
    {
        string text = root
            .DescendantTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia))
            .ToFullString();

        return text == "\r\n" ? CarriageReturnLineFeed : LineFeed;
    }

    private static async Task<Document> ApplyAsync(
        Document document,
        RewritePlan plan,
        CancellationToken cancellationToken
    )
    {
        SyntaxNode? root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        IdentifierNameSyntax listReference = IdentifierName(plan.ListName);

        // The loop must sit directly in a block so the declaration and the
        // aggregation assignment have a statement list to join.
        if (plan.Loop.Parent is not BlockSyntax)
        {
            return document;
        }

        int loopStart = plan.Loop.SpanStart;

        // 1. Each `acc += x;` inside the loop becomes `<list>.Add(x);`.
        SyntaxNode newRoot = root;
        foreach (ExpressionStatementSyntax accumulation in plan.Accumulations)
        {
            if (
                accumulation.Expression
                is not AssignmentExpressionSyntax { Right: ExpressionSyntax added }
            )
            {
                return document;
            }

            ExpressionStatementSyntax addCall = accumulation
                .WithExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            listReference,
                            IdentifierName("Add")
                        ),
                        ArgumentList(SingletonSeparatedList(Argument(added.WithoutTrivia())))
                    )
                );

            newRoot = newRoot.ReplaceNode(accumulation, addCall);
        }

        // Re-anchor the loop and its block in the updated tree by span. The
        // edits above sit strictly inside the loop body, so neither the
        // block's nor the loop's start moved.
        BlockSyntax? block = newRoot
            .DescendantNodes()
            .OfType<BlockSyntax>()
            .FirstOrDefault(b =>
                b.SpanStart == plan.Loop.Parent!.SpanStart
                && b.DescendantNodes().OfType<StatementSyntax>().Any(s => s.SpanStart == loopStart)
            );

        StatementSyntax? loop = block
            ?.Statements.FirstOrDefault(s => s.SpanStart == loopStart);

        if (block == null || loop == null)
        {
            return document;
        }

        // 2. `var <list> = new List<Error>();` before the loop.
        // Trivia contract (Roslyn keeps the line break in the *preceding*
        // token's trailing trivia): leading indent only — the break before us
        // already lives in the previous statement's trailing trivia — plus our
        // own trailing break, since the loop's leading trivia has none.
        SyntaxTrivia newLine = DocumentEndOfLine(root);
        SyntaxTriviaList indent = LeadingIndent(loop);

        LocalDeclarationStatementSyntax listDeclaration = LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName("var"),
                    SingletonSeparatedList(
                        VariableDeclarator(Identifier(plan.ListName)).WithInitializer(
                            EqualsValueClause(
                                ObjectCreationExpression(
                                        GenericName(
                                            Identifier("List")
                                        )
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList(plan.ErrorType)
                                            )
                                        ),
                                        ArgumentList(),
                                        null
                                    )
                            )
                        )
                    )
                )
            )
            .WithLeadingTrivia(indent)
            .WithTrailingTrivia(newLine);

        // 3. `acc = Error.Aggregate(<list>);` after the loop — same contract:
        // leading indent only (break before us lives in the loop's trailing
        // trivia), own trailing break (the next statement's leading has none).
        ExpressionSyntax aggregateCall = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                plan.ErrorType,
                IdentifierName("Aggregate")
            ),
            ArgumentList(SingletonSeparatedList(Argument(listReference)))
        );

        if (!plan.AccumulatorIsNullable)
        {
            aggregateCall = PostfixUnaryExpression(
                SyntaxKind.SuppressNullableWarningExpression,
                aggregateCall
            );
        }

        ExpressionStatementSyntax aggregateAssignment = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(
                        plan.AccumulatorDeclaration.Identifier.ValueText
                    ),
                    aggregateCall
                )
            )
            .WithLeadingTrivia(indent)
            .WithTrailingTrivia(newLine);

        SyntaxList<StatementSyntax> updated = block
            .Statements.Insert(block.Statements.IndexOf(loop), listDeclaration)
            .Insert(block.Statements.IndexOf(loop) + 2, aggregateAssignment);

        newRoot = newRoot.ReplaceNode(block, block.WithStatements(updated));

        // 4. `using System.Collections.Generic;` when the file lacks it.
        // Same contract: the break before the new directive already lives in
        // the previous using's trailing trivia, so only a trailing break in
        // the document's own newline flavor is added.
        if (
            newRoot is CompilationUnitSyntax compilationUnit
            && !compilationUnit.Usings.Any(u =>
                u.Name?.ToString() == "System.Collections.Generic"
            )
        )
        {
            UsingDirectiveSyntax listUsing = UsingDirective(
                ParseName("System.Collections.Generic")
            ).WithTrailingTrivia(newLine);

            newRoot = compilationUnit.Usings.Count > 0
                ? compilationUnit.WithUsings(compilationUnit.Usings.Add(listUsing))
                : compilationUnit.WithUsings(SingletonList(listUsing));
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
