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

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to record",
                createChangedDocument: c => ConvertToRecordAsync(context.Document, declaration, c),
                equivalenceKey: nameof(ValueObjectRecordTypeCodeFixProvider)
            ),
            diagnostic
        );
    }

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
