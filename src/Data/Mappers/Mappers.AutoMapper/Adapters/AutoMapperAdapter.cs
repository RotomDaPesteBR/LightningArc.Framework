namespace LightningArc.Mappers.AutoMapper.Adapters;

/// <summary>
/// Adapter implementation of <see cref="LightningArc.Data.Abstractions.Mappers.IMapper"/> using the AutoMapper library.
/// </summary>
/// <param name="mapper">The AutoMapper mapper instance.</param>
public class AutoMapperAdapter(global::AutoMapper.IMapper mapper)
    : LightningArc.Data.Abstractions.Mappers.IMapper
{
    private readonly global::AutoMapper.IMapper _mapper = mapper;

    /// <inheritdoc />
    public object Instance => _mapper;

    /// <inheritdoc />
    public TDestination Map<TDestination>(object source) => _mapper.Map<TDestination>(source);

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
        _mapper.Map(source, destination);
}
