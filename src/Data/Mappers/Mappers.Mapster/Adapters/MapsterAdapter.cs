namespace LightningArc.Mappers.Mapster.Adapters;

/// <summary>
/// Adapter implementation of <see cref="LightningArc.Data.Abstractions.Mappers.IMapper"/> using the Mapster library.
/// </summary>
/// <param name="mapper">The Mapster mapper instance.</param>
public class MapsterAdapter(MapsterMapper.IMapper mapper)
    : LightningArc.Data.Abstractions.Mappers.IMapper
{
    private readonly MapsterMapper.IMapper _mapper = mapper;

    /// <inheritdoc />
    public object Instance => _mapper;

    /// <inheritdoc />
    public TDestination Map<TDestination>(object source) => _mapper.Map<TDestination>(source);

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
        _mapper.Map(source, destination);
}
