using System.Reflection;
using LightningArc.Data.ADO.Repositories;
using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Recognizers;

/// <summary>
/// Pins the true/false boundaries of <c>LightningArc.Analyzers.ResultTypeRecognizer</c>.
/// </summary>
/// <remarks>
/// The recognizer type is internal to the analyzers assembly, so these tests reach it via
/// reflection (no <c>InternalsVisibleTo</c> production change). Compilations are built
/// directly with Roslyn over the real LightningArc assemblies, mirroring the references
/// the analyzer test verifier wires up in <c>Verifiers/AnalyzerVerifier.cs</c>.
/// True API shapes verified against <c>src/Analyzers/Recognizers/ResultTypeRecognizer.cs</c>:
/// <c>Resolve(Compilation)</c>, <c>IsAvailable</c>, <c>IsResultType</c>, <c>IsErrorType</c>,
/// <c>IsAggregateErrorType</c>.
/// </remarks>
[Category("Recognizers")]
public class ResultTypeRecognizerTests
{
    private static readonly Type RecognizerType = RecognizerTestCompilations.GetRecognizerType(
        "ResultTypeRecognizer"
    );

    private const string ProbeSource = """
        using LightningArc.Results;

        namespace Other
        {
            public class Result { }
            public class Error { }
        }

        public class Probe
        {
            public Result ResultField = null!;
            public Result<int> GenericResultField = null!;
            public Error ErrorField = null!;
            public AggregateError AggregateField = null!;
            public Other.Result ForeignResultField = null!;
            public Other.Error ForeignErrorField = null!;
            public string PlainField = null!;
        }
        """;

    private static readonly CSharpCompilation Compilation = RecognizerTestCompilations.Create(
        ProbeSource
    );

    private static readonly object Recognizer = RecognizerTestCompilations.Resolve(
        RecognizerType,
        Compilation
    );

    private static ITypeSymbol? FieldType(string name) =>
        RecognizerTestCompilations.GetFieldType(Compilation, "Probe", name);

    private static bool IsResultType(ITypeSymbol? candidate) =>
        RecognizerTestCompilations.InvokeInstance(
            RecognizerType,
            Recognizer,
            "IsResultType",
            candidate
        );

    private static bool IsErrorType(ITypeSymbol? candidate) =>
        RecognizerTestCompilations.InvokeInstance(
            RecognizerType,
            Recognizer,
            "IsErrorType",
            candidate
        );

    private static bool IsAggregateErrorType(ITypeSymbol? candidate) =>
        RecognizerTestCompilations.InvokeInstance(
            RecognizerType,
            Recognizer,
            "IsAggregateErrorType",
            candidate
        );

    [Test]
    public async Task IsAvailable_True_When_Types_Referenced()
    {
        await Assert.That(RecognizerTestCompilations.GetIsAvailable(Recognizer)).IsTrue();
    }

    [Test]
    public async Task IsAvailable_False_When_Types_Absent()
    {
        object recognizer = RecognizerTestCompilations.Resolve(
            RecognizerType,
            RecognizerTestCompilations.Create("public class C { }", withLightningArcReferences: false)
        );

        await Assert.That(RecognizerTestCompilations.GetIsAvailable(recognizer)).IsFalse();
    }

    [Test]
    public async Task IsResultType_NonGenericResult_True()
    {
        await Assert.That(IsResultType(FieldType("ResultField"))).IsTrue();
    }

    [Test]
    public async Task IsResultType_GenericResult_True()
    {
        await Assert.That(IsResultType(FieldType("GenericResultField"))).IsTrue();
    }

    [Test]
    public async Task IsResultType_Error_False()
    {
        await Assert.That(IsResultType(FieldType("ErrorField"))).IsFalse();
    }

    [Test]
    public async Task IsResultType_AggregateError_False()
    {
        await Assert.That(IsResultType(FieldType("AggregateField"))).IsFalse();
    }

    [Test]
    public async Task IsErrorType_Error_True()
    {
        await Assert.That(IsErrorType(FieldType("ErrorField"))).IsTrue();
    }

    [Test]
    public async Task IsErrorType_AggregateError_As_Error_True()
    {
        await Assert.That(IsErrorType(FieldType("AggregateField"))).IsTrue();
    }

    [Test]
    public async Task IsErrorType_Result_False()
    {
        await Assert.That(IsErrorType(FieldType("ResultField"))).IsFalse();
    }

    [Test]
    public async Task IsErrorType_GenericResult_False()
    {
        await Assert.That(IsErrorType(FieldType("GenericResultField"))).IsFalse();
    }

    [Test]
    public async Task IsAggregateErrorType_AggregateError_True()
    {
        await Assert.That(IsAggregateErrorType(FieldType("AggregateField"))).IsTrue();
    }

    [Test]
    public async Task IsAggregateErrorType_PlainError_False()
    {
        await Assert.That(IsAggregateErrorType(FieldType("ErrorField"))).IsFalse();
    }

    [Test]
    public async Task IsAggregateErrorType_Result_False()
    {
        await Assert.That(IsAggregateErrorType(FieldType("ResultField"))).IsFalse();
    }

    [Test]
    public async Task OtherNamespace_Result_Is_Not_ResultType()
    {
        await Assert.That(IsResultType(FieldType("ForeignResultField"))).IsFalse();
    }

    [Test]
    public async Task OtherNamespace_Error_Is_Not_ErrorType()
    {
        await Assert.That(IsErrorType(FieldType("ForeignErrorField"))).IsFalse();
    }

    [Test]
    public async Task Unrelated_PlainType_Matches_Nothing()
    {
        ITypeSymbol? plain = FieldType("PlainField");

        await Assert.That(IsResultType(plain)).IsFalse();
        await Assert.That(IsErrorType(plain)).IsFalse();
        await Assert.That(IsAggregateErrorType(plain)).IsFalse();
    }

    [Test]
    public async Task Null_Candidate_Matches_Nothing()
    {
        await Assert.That(IsResultType(null)).IsFalse();
        await Assert.That(IsErrorType(null)).IsFalse();
        await Assert.That(IsAggregateErrorType(null)).IsFalse();
    }

    [Test]
    public async Task Unavailable_Recognizer_Matches_Nothing()
    {
        CSharpCompilation compilation = RecognizerTestCompilations.Create(
            "public class C { public string Name = null!; }",
            withLightningArcReferences: false
        );
        object recognizer = RecognizerTestCompilations.Resolve(RecognizerType, compilation);
        ITypeSymbol? nameType = RecognizerTestCompilations.GetFieldType(compilation, "C", "Name");

        await Assert.That(
            RecognizerTestCompilations.InvokeInstance(
                RecognizerType,
                recognizer,
                "IsResultType",
                nameType
            )
        ).IsFalse();
        await Assert.That(
            RecognizerTestCompilations.InvokeInstance(
                RecognizerType,
                recognizer,
                "IsErrorType",
                nameType
            )
        ).IsFalse();
    }
}

/// <summary>
/// Shared Roslyn compilation factory for the recognizer unit tests. Lives in this file to
/// keep the task's four-file footprint; the other three recognizer test classes reuse it.
/// </summary>
internal static class RecognizerTestCompilations
{
    internal static readonly Assembly AnalyzersAssembly =
        typeof(ResultDiscardedAnalyzer).Assembly;

    private static readonly Lazy<IReadOnlyList<string>> RuntimeReferencePaths = new(
        () =>
        {
            string[] wanted =
            [
                "System.Private.CoreLib.dll",
                "System.Runtime.dll",
                "netstandard.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Data.Common.dll",
                "System.ComponentModel.Primitives.dll",
            ];
            string tpa =
                AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
            return tpa
                .Split(Path.PathSeparator)
                .Where(p => wanted.Contains(Path.GetFileName(p)))
                .ToList();
        }
    );

    internal static Type GetRecognizerType(string name) =>
        AnalyzersAssembly.GetType($"LightningArc.Analyzers.{name}")
        ?? throw new InvalidOperationException($"Recognizer type '{name}' not found.");

    internal static CSharpCompilation Create(string source, bool withLightningArcReferences = true)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        var references = RuntimeReferencePaths.Value
            .Select(p => MetadataReference.CreateFromFile(p))
            .Cast<MetadataReference>()
            .ToList();
        if (withLightningArcReferences)
        {
            references.Add(MetadataReference.CreateFromFile(typeof(Result).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Email).Assembly.Location));
            references.Add(
                MetadataReference.CreateFromFile(typeof(RepositoryBase).Assembly.Location)
            );
        }

        return CSharpCompilation.Create(
            "RecognizerTests",
            [tree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    internal static object Resolve(Type recognizerType, CSharpCompilation compilation) =>
        recognizerType
            .GetMethod("Resolve", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, [compilation])!;

    internal static bool InvokeInstance(
        Type recognizerType,
        object recognizer,
        string method,
        params object?[] args
    ) =>
        (bool)
            recognizerType
                .GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!
                .Invoke(recognizer, args)!;

    internal static bool InvokeStatic(
        Type recognizerType,
        string method,
        params object?[] args
    ) =>
        (bool)
            recognizerType
                .GetMethod(method, BindingFlags.Public | BindingFlags.Static)!
                .Invoke(null, args)!;

    internal static bool GetIsAvailable(object recognizer) =>
        (bool)
            recognizer
                .GetType()
                .GetProperty("IsAvailable", BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(recognizer)!;

    internal static INamedTypeSymbol? GetDeclaredType(CSharpCompilation compilation, string name)
    {
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel model = compilation.GetSemanticModel(tree);
            foreach (
                BaseTypeDeclarationSyntax declaration in tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<BaseTypeDeclarationSyntax>()
            )
            {
                if (
                    model.GetDeclaredSymbol(declaration) is INamedTypeSymbol symbol
                    && symbol.Name == name
                )
                {
                    return symbol;
                }
            }
        }

        return null;
    }

    internal static ITypeSymbol? GetFieldType(
        CSharpCompilation compilation,
        string containingTypeName,
        string fieldName
    ) =>
        GetDeclaredType(compilation, containingTypeName)
            ?.GetMembers(fieldName)
            .OfType<IFieldSymbol>()
            .FirstOrDefault()
            ?.Type;

    internal static (MemberAccessExpressionSyntax Access, SemanticModel Model) GetSingleMemberAccess(
        CSharpCompilation compilation,
        string memberName
    )
    {
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            var matches = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Where(m => m.Name.Identifier.ValueText == memberName)
                .ToList();
            if (matches.Count == 1)
            {
                return (matches[0], compilation.GetSemanticModel(tree));
            }
        }

        throw new InvalidOperationException(
            $"Expected exactly one '.{memberName}' access in test source."
        );
    }
}
