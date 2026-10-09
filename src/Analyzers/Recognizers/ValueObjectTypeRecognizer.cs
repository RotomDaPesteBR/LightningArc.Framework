using System.Linq;
using Microsoft.CodeAnalysis;

namespace LightningArc.Analyzers;

/// <summary>
/// Structural recognition of LightningArc Value Object types (built-in ones like <c>Email</c>,
/// <c>Cpf</c>, <c>Cnpj</c>, <c>PhoneNumber</c>, <c>Url</c>, and any consumer-defined type
/// implementing the Value Object contract).
/// </summary>
internal static class ValueObjectTypeRecognizer
{
    private const string _primitivesNamespace = "LightningArc.Primitives";
    private const string _valueObjectInterfaceSimpleName = "IValueObject";

    /// <summary>
    /// Determines whether <paramref name="candidate"/> is a LightningArc Value Object.
    /// </summary>
    /// <remarks>
    /// Recognition is layered, walking the base-type chain so a type deriving from a Value
    /// Object base class (rather than implementing the interface directly) is still recognized:
    /// 1. Implements an interface whose simple name is "IValueObject" — matched by
    ///    <see cref="ITypeSymbol.OriginalDefinition"/>, so this covers both a non-generic
    ///    <c>IValueObject</c> and any arity of <c>IValueObject&lt;T&gt;</c> without needing to
    ///    hardcode which shape the library actually uses. Checked namespace-agnostically on
    ///    purpose, so consumer-defined Value Objects outside LightningArc.Primitives are still
    ///    recognized.
    /// 2. Is declared directly inside the LightningArc.Primitives namespace — covers the
    ///    built-in types even in the rare case one doesn't implement the interface itself
    ///    (e.g. an abstract intermediate base class).
    /// </remarks>
    public static bool IsValueObjectType(ITypeSymbol? candidate)
    {
        for (ITypeSymbol? current = candidate; current != null; current = current.BaseType)
        {
            if (current is not INamedTypeSymbol named)
            {
                continue;
            }

            if (
                named.AllInterfaces.Any(i =>
                    i.OriginalDefinition.Name == _valueObjectInterfaceSimpleName
                )
            )
            {
                return true;
            }

            if (named.ContainingNamespace?.ToDisplayString() == _primitivesNamespace)
            {
                return true;
            }
        }

        return false;
    }
}
