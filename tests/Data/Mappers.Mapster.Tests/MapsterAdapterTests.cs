using LightningArc.Mappers.Mapster.Adapters;
using Mapster;
using MapsterMapper;

namespace LightningArc.Mappers.Tests;

public class MapsterAdapterTests
{
    public class Source { public string Name { get; set; } = ""; }
    public class Destination { public string Name { get; set; } = ""; }

    [Test]
    public async Task Map_ShouldTransformSourceToDestination()
    {
        // Arrange
        TypeAdapterConfig config = new();
        Mapper mapper = new(config);
        MapsterAdapter adapter = new(mapper);
        Source source = new() { Name = "Test" };

        // Act
        Destination result = adapter.Map<Destination>(source);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Name).IsEqualTo(source.Name);
    }

    [Test]
    public async Task Map_WithExistingDestination_ShouldUpdateDestination()
    {
        // Arrange
        TypeAdapterConfig config = new();
        Mapper mapper = new(config);
        MapsterAdapter adapter = new(mapper);
        Source source = new() { Name = "Updated" };
        Destination destination = new() { Name = "Original" };

        // Act
        Destination result = adapter.Map(source, destination);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Name).IsEqualTo("Updated");
    }

    // Note: Mapster throws ArgumentNullException on null source - behavior validated in usage
}

