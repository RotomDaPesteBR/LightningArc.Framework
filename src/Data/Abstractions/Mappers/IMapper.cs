namespace LightningArc.Data.Abstractions.Mappers;

/// <summary>
/// Defines a contract for object-to-object mapping.
/// </summary>
/// <remarks>
/// This interface allows the decoupling of the data access layer from specific mapping libraries
/// (e.g., Mapster, AutoMapper). Adapters should implement this interface to wrap the concrete library.
/// </remarks>
public interface IMapper
{
    /// <summary>
    /// Gets the underlying mapping engine instance (e.g., AutoMapper's IMapper or Mapster's ServiceMapper).
    /// </summary>
    object Instance { get; }

    /// <summary>
    /// Maps a source object to a new destination object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <returns>A new instance of <typeparamref name="TDestination"/> containing the mapped data.</returns>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps a source object to an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDestination">The type of the destination object.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination object to map to.</param>
    /// <returns>The <paramref name="destination"/> object updated with mapped data.</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}

