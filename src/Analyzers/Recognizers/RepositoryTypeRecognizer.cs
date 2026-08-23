using Microsoft.CodeAnalysis;

namespace LightningArc.Analyzers;

/// <summary>
/// Resolved <c>LightningArc.Data.ADO.Repositories.RepositoryBase</c> symbol for a single
/// compilation, with an instance method for recognizing whether a type derives from it —
/// directly or through one of its generic variants (<c>RepositoryBase&lt;TEntity&gt;</c>,
/// <c>RepositoryBase&lt;TEntity, TResult&gt;</c>).
/// </summary>
internal readonly struct RepositoryTypeRecognizer(INamedTypeSymbol? repositoryBaseType)
{
    private const string _repositoryBaseMetadataName =
        "LightningArc.Data.ADO.Repositories.RepositoryBase";

    public INamedTypeSymbol? RepositoryBaseType { get; } = repositoryBaseType;

    public bool IsAvailable => RepositoryBaseType != null;

    /// <summary>
    /// Resolves the RepositoryBase symbol for a compilation. Call once per compilation start,
    /// same pattern as <see cref="ResultTypeRecognizer.Resolve"/>.
    /// </summary>
    public static RepositoryTypeRecognizer Resolve(Compilation compilation) =>
        new(compilation.GetTypeByMetadataName(_repositoryBaseMetadataName));

    /// <summary>
    /// Determines whether <paramref name="symbol"/> derives from RepositoryBase, directly or
    /// through one of its generic variants.
    /// </summary>
    public bool InheritsFromRepositoryBase(INamedTypeSymbol? symbol)
    {
        if (RepositoryBaseType == null)
        {
            return false;
        }

        for (
            INamedTypeSymbol? current = symbol?.BaseType;
            current != null;
            current = current.BaseType
        )
        {
            if (SymbolEqualityComparer.Default.Equals(current, RepositoryBaseType))
            {
                return true;
            }
        }

        return false;
    }
}
