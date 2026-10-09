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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LightningArc.Analyzers.CodeFixes;

[
    ExportCodeFixProvider(
        LanguageNames.CSharp,
        Name = nameof(ValueObjectRecordTypeCodeFixProvider)
    ),
    Shared
]
public class ValueObjectRecordTypeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [ValueObjectRecordTypeAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context
            .Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var declaration = root.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (declaration == null)
        {
            return;
        }

        // GAP-8: withhold the class->record swap when the type hand-writes
        // equality or construction semantics — converting to a record would
        // silently change runtime behavior. Syntax-only check (no
        // SemanticModel needed). Only true `object`-equality overrides
        // withhold: a typed `Equals(T)` overload (e.g. `IEquatable<T>`) is
        // record-compatible and still offers the fix.
        // Constructor-shape decision: both block-bodied (`{ ... }`, including
        // an empty block) and expression-bodied (`=> ...`) constructors count
        // as user-written bodies and withhold the fix. A declaration with
        // neither (e.g. an `extern`/partial stub) still offers the fix.
        // Initializers alone (`: this(...)` / `: base(...)`) do not exempt:
        // every legal in-source constructor with an initializer also carries
        // a body or arrow, so it is already guarded by the body check below.
        bool hasCustomEquality = declaration.Members.Any(m =>
            (m is MethodDeclarationSyntax md && IsObjectEqualsOverride(md))
            || (m is MethodDeclarationSyntax gh && IsParameterlessGetHashCodeOverride(gh))
            || (m is OperatorDeclarationSyntax od && od.OperatorToken.Text is "==" or "!=")
            || (m is ConstructorDeclarationSyntax cd && (cd.Body is not null || cd.ExpressionBody is not null)));
        if (hasCustomEquality)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to record",
                createChangedDocument: c => ConvertToRecordAsync(context.Document, declaration, c),
                equivalenceKey: nameof(ValueObjectRecordTypeCodeFixProvider)
            ),
            diagnostic
        );
    }

    /// <summary>
    /// True for an <c>Equals(object[?])</c> override: the signature a record
    /// would synthesize. Typed overloads (<c>Equals(T)</c>) are
    /// record-compatible and do not withhold the fix.
    /// </summary>
    private static bool IsObjectEqualsOverride(MethodDeclarationSyntax md) =>
        md.Identifier.Text == "Equals"
        && md.Modifiers.Any(SyntaxKind.OverrideKeyword)
        && md.ParameterList?.Parameters is { Count: 1 } parameters
        && parameters[0].Type?.ToString() is "object" or "object?";

    /// <summary>
    /// True for a parameterless <c>GetHashCode()</c> override: the signature a
    /// record would synthesize.
    /// </summary>
    private static bool IsParameterlessGetHashCodeOverride(MethodDeclarationSyntax md) =>
        md.Identifier.Text == "GetHashCode"
        && md.Modifiers.Any(SyntaxKind.OverrideKeyword)
        && md.ParameterList?.Parameters.Count == 0;

    private static async Task<Document> ConvertToRecordAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken
    )
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Create a record declaration from the class declaration
        var recordDeclaration = RecordDeclaration(
            classDeclaration.AttributeLists,
            classDeclaration.Modifiers,
            Token(SyntaxKind.RecordKeyword),
            classDeclaration.Identifier,
            classDeclaration.TypeParameterList,
            classDeclaration.ParameterList,
            classDeclaration.BaseList,
            classDeclaration.ConstraintClauses,
            classDeclaration.OpenBraceToken,
            classDeclaration.Members,
            classDeclaration.CloseBraceToken,
            classDeclaration.SemicolonToken
        );

        var newRoot = root.ReplaceNode(classDeclaration, recordDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
