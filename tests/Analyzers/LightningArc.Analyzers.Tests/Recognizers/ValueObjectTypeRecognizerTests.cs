using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TUnit.Core;

namespace LightningArc.Analyzers.Tests.Recognizers;

/// <summary>
/// Pins the true/false boundaries of <c>LightningArc.Analyzers.ValueObjectTypeRecognizer</c>.
/// </summary>
/// <remarks>
/// True API shape verified against
/// <c>src/Analyzers/Recognizers/ValueObjectTypeRecognizer.cs</c>: a static
/// <c>IsValueObjectType(ITypeSymbol?)</c> with layered recognition — (1) any implemented
/// interface whose simple name is <c>IValueObject</c> (namespace-agnostic, any arity,
/// walked over the base-type chain), (2) declaration directly inside the
/// <c>LightningArc.Primitives</c> namespace.
/// </remarks>
[Category("Recognizers")]
public class ValueObjectTypeRecognizerTests
{
    private static readonly Type RecognizerType = RecognizerTestCompilations.GetRecognizerType(
        "ValueObjectTypeRecognizer"
    );

    private const string ProbeSource = """
        using LightningArc.Primitives;
        using LightningArc.Primitives.ValueObjects;

        namespace Consumer
        {
            public interface IValueObject { }

            public interface IValueObject<T>
            {
                T Value { get; }
            }
        }

        namespace LightningArc.Primitives
        {
            public class BareMarker { }
        }

        public class Money : Consumer.IValueObject<decimal>
        {
            public decimal Value => 0;
        }

        public class Note : Consumer.IValueObject { }

        public class SpecialMoney : Money { }

        public class PlainHolder { }

        public class Probe
        {
            public Email EmailField = null!;
            public Cpf CpfField = null!;
            public Money MoneyField = null!;
            public Note NoteField = null!;
            public SpecialMoney SpecialMoneyField = null!;
            public BareMarker BareField = null!;
            public PlainHolder PlainField = null!;
            public string TextField = null!;
        }
        """;

    private static readonly CSharpCompilation Compilation = RecognizerTestCompilations.Create(
        ProbeSource
    );

    private static bool IsValueObject(string fieldName) =>
        RecognizerTestCompilations.InvokeStatic(
            RecognizerType,
            "IsValueObjectType",
            [
                RecognizerTestCompilations.GetFieldType(Compilation, "Probe", fieldName),
            ]
        );

    [Test]
    public async Task BuiltIn_Email_True()
    {
        await Assert.That(IsValueObject("EmailField")).IsTrue();
    }

    [Test]
    public async Task BuiltIn_Cpf_True()
    {
        await Assert.That(IsValueObject("CpfField")).IsTrue();
    }

    [Test]
    public async Task Consumer_Generic_Interface_True()
    {
        await Assert.That(IsValueObject("MoneyField")).IsTrue();
    }

    [Test]
    public async Task Consumer_NonGeneric_SameName_Interface_Outside_Primitives_True()
    {
        await Assert.That(IsValueObject("NoteField")).IsTrue();
    }

    [Test]
    public async Task Derived_From_ValueObject_True()
    {
        await Assert.That(IsValueObject("SpecialMoneyField")).IsTrue();
    }

    [Test]
    public async Task Bare_Type_In_Primitives_Namespace_True()
    {
        await Assert.That(IsValueObject("BareField")).IsTrue();
    }

    [Test]
    public async Task Unrelated_Named_Type_False()
    {
        await Assert.That(IsValueObject("PlainField")).IsFalse();
    }

    [Test]
    public async Task Unrelated_Plain_Type_False()
    {
        await Assert.That(IsValueObject("TextField")).IsFalse();
    }

    [Test]
    public async Task Null_Candidate_False()
    {
        await Assert.That(
            RecognizerTestCompilations.InvokeStatic(
                RecognizerType,
                "IsValueObjectType",
                [null]
            )
        ).IsFalse();
    }
}
