using LightningArc.Data.Abstractions.Mappers;

namespace LightningArc.Data.Abstractions.Tests;

public class MapperExtensionsTests
{
    private class TestEntity { public int Id { get; set; } }
    private class TestDto { public int Id { get; set; } }

    private class TestMapper : IMapper
    {
        public object Instance => throw new NotImplementedException();

        public TDestination Map<TDestination>(object source)
        {
            if (source is TestEntity entity && typeof(TDestination) == typeof(TestDto))
            {
                return (TDestination)(object)new TestDto { Id = entity.Id };
            }
            throw new NotImplementedException();
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            throw new NotImplementedException();
        }
    }

    [Test]
    public async Task Map_ShouldMapItem()
    {
        // Arrange
        var mapper = new TestMapper();
        var entity = new TestEntity { Id = 1 };

        // Act
        var dto = mapper.Map<TestDto>(entity);

        // Assert
        await Assert.That(dto).IsNotNull();
        await Assert.That(dto.Id).IsEqualTo(1);
    }
}
