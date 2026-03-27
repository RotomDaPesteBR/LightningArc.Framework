# Mappers Usage Guide

**Mappers** provides adapters for popular mapping libraries, allowing the `IMapper` abstraction from `Data.Abstractions` to be used with concrete implementations like Mapster or AutoMapper.

## Available Adapters

### Mapster (`Mappers.Mapster`)

Uses the high-performance **Mapster** library.

#### Registration
```csharp
using LightningArc.Mappers.Mapster.DependencyInjection;

// Registers Mapster and the IMapper adapter
services.AddMapsterAdapter();
```

### AutoMapper (`Mappers.AutoMapper`)

Uses the industry-standard **AutoMapper** library (Version 16.0+).

#### Registration
```csharp
using LightningArc.Mappers.AutoMapper.DependencyInjection;

// Registers the IMapper adapter (AutoMapper must be registered separately)
services.AddAutoMapper(typeof(MyProfile));
services.AddAutoMapperAdapter();
```

## Advanced Usage

### Accessing the Underlying Engine

If you need to access specific features of the underlying mapping library that are not exposed via the `IMapper` interface, you can use the `Instance` property.

```csharp
public class MyService(IMapper mapper)
{
    public void ConfigureMapping()
    {
        // For AutoMapper
        var autoMapper = (AutoMapper.IMapper)mapper.Instance;
        
        // For Mapster
        var mapster = (MapsterMapper.IMapper)mapper.Instance;
    }
}
```

## Usage in Repositories

When using `RepositoryBase<TEntity, TResult>`, the injected mapper will automatically handle the transformation if provided.

```csharp
public class UserRepository : RepositoryBase<UserEntity, UserDto>
{
    public UserRepository(IConnectionFactory factory, IMapper mapper) 
        : base(factory, mapper) { }

    public async Task<UserDto> GetDtoAsync(int id)
    {
        // ... get entity ...
        return Mapper.Map<UserDto>(entity);
    }
}
```
