using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Recognizers;

/// <summary>
/// Pins the true/false boundaries of <c>LightningArc.Analyzers.RepositoryTypeRecognizer</c>.
/// </summary>
/// <remarks>
/// True API shapes verified against
/// <c>src/Analyzers/Recognizers/RepositoryTypeRecognizer.cs</c>: <c>Resolve(Compilation)</c>,
/// <c>IsAvailable</c>, <c>RepositoryBaseType</c>, <c>InheritsFromRepositoryBase</c> (walks
/// ancestors only, starting at <c>BaseType</c> — so <c>RepositoryBase</c> itself is false,
/// and callers needing is-or-derives also compare <c>RepositoryBaseType</c>).
/// </remarks>
[Category("Recognizers")]
public class RepositoryTypeRecognizerTests
{
    private static readonly Type RecognizerType = RecognizerTestCompilations.GetRecognizerType(
        "RepositoryTypeRecognizer"
    );

    private const string ProbeSource = """
        using System.Data.Common;
        using LightningArc.Data.ADO.Repositories;

        namespace Other
        {
            public abstract class RepositoryBase { }
        }

        public class Entity { }
        public class Dto { }

        public class DirectRepository : RepositoryBase
        {
            public DirectRepository() : base((DbConnection)null!, (DbTransaction)null!) { }
        }

        public class GenericRepository : RepositoryBase<Entity>
        {
            public GenericRepository() : base((DbConnection)null!, (DbTransaction)null!) { }
        }

        public class TwoKeyRepository : RepositoryBase<Entity, Dto>
        {
            public TwoKeyRepository() : base((DbConnection)null!, (DbTransaction)null!) { }
        }

        public class MiddleRepository : DirectRepository
        {
            public MiddleRepository() : base() { }
        }

        public class ForeignRepository : Other.RepositoryBase { }

        public class PlainClass { }
        """;

    private static readonly CSharpCompilation Compilation = RecognizerTestCompilations.Create(
        ProbeSource
    );

    private static readonly object Recognizer = RecognizerTestCompilations.Resolve(
        RecognizerType,
        Compilation
    );

    private static bool InheritsFrom(string name) =>
        RecognizerTestCompilations.InvokeInstance(
            RecognizerType,
            Recognizer,
            "InheritsFromRepositoryBase",
            [RecognizerTestCompilations.GetDeclaredType(Compilation, name)]
        );

    [Test]
    public async Task IsAvailable_True_When_Types_Referenced()
    {
        await Assert.That(RecognizerTestCompilations.GetIsAvailable(Recognizer)).IsTrue();
    }

    [Test]
    public async Task IsAvailable_False_When_Types_Absent()
    {
        CSharpCompilation compilation = RecognizerTestCompilations.Create(
            "public class C { }",
            withLightningArcReferences: false
        );
        object recognizer = RecognizerTestCompilations.Resolve(RecognizerType, compilation);

        await Assert.That(RecognizerTestCompilations.GetIsAvailable(recognizer)).IsFalse();
    }

    [Test]
    public async Task Direct_Subclass_True()
    {
        await Assert.That(InheritsFrom("DirectRepository")).IsTrue();
    }

    [Test]
    public async Task Generic_Variant_Subclass_True()
    {
        await Assert.That(InheritsFrom("GenericRepository")).IsTrue();
    }

    [Test]
    public async Task TwoGeneric_Variant_Subclass_True()
    {
        await Assert.That(InheritsFrom("TwoKeyRepository")).IsTrue();
    }

    [Test]
    public async Task Indirect_Subclass_True()
    {
        await Assert.That(InheritsFrom("MiddleRepository")).IsTrue();
    }

    [Test]
    public async Task RepositoryBase_Itself_False()
    {
        INamedTypeSymbol? repositoryBase = Compilation.GetTypeByMetadataName(
            "LightningArc.Data.ADO.Repositories.RepositoryBase"
        );

        await Assert.That(repositoryBase).IsNotNull();
        await Assert.That(
            RecognizerTestCompilations.InvokeInstance(
                RecognizerType,
                Recognizer,
                "InheritsFromRepositoryBase",
                [repositoryBase]
            )
        ).IsFalse();
    }

    [Test]
    public async Task Unrelated_SameName_RepositoryBase_False()
    {
        await Assert.That(InheritsFrom("ForeignRepository")).IsFalse();
    }

    [Test]
    public async Task Plain_Class_False()
    {
        await Assert.That(InheritsFrom("PlainClass")).IsFalse();
    }

    [Test]
    public async Task Null_Symbol_False()
    {
        await Assert.That(
            RecognizerTestCompilations.InvokeInstance(
                RecognizerType,
                Recognizer,
                "InheritsFromRepositoryBase",
                [null]
            )
        ).IsFalse();
    }

    [Test]
    public async Task Unavailable_Recognizer_Matches_Nothing()
    {
        CSharpCompilation compilation = RecognizerTestCompilations.Create(
            "public class C { }",
            withLightningArcReferences: false
        );
        object recognizer = RecognizerTestCompilations.Resolve(RecognizerType, compilation);

        await Assert.That(
            RecognizerTestCompilations.InvokeInstance(
                RecognizerType,
                recognizer,
                "InheritsFromRepositoryBase",
                [RecognizerTestCompilations.GetDeclaredType(compilation, "C")]
            )
        ).IsFalse();
    }
}
