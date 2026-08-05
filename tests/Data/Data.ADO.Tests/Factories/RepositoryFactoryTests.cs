using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace LightningArc.Data.Tests.Factories;

public class RepositoryFactoryTests
{
    private readonly IServiceProvider _serviceProvider =
        new ServiceCollection().BuildServiceProvider();
    private readonly MockConnectionFactory _connectionFactory = new();
    private readonly MockMapper _mapper = new();

    [Test]
    public async Task Create_ShouldReturnRepositoryWithoutMapper_WhenNoMapperProvidedToFactory()
    {
        // Arrange
        // Added _serviceProvider as the first argument
        RepositoryFactory factory = new(_serviceProvider, _connectionFactory);

        // Act
        TestRepository repository = factory.Create<TestRepository>();

        // Assert
        await Assert.That(repository).IsNotNull();
        await Assert.That(() => repository.InjectedMapper).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Create_ShouldReturnRepositoryWithMapper_WhenMapperIsProvidedToFactory()
    {
        // Arrange
        // Added _serviceProvider as the first argument, followed by connection factory and mapper
        RepositoryFactory factory = new(_serviceProvider, _connectionFactory, _mapper);

        // Act
        TestRepository repository = factory.Create<TestRepository>();

        // Assert
        await Assert.That(repository).IsNotNull();
        await Assert.That(repository.InjectedMapper).IsEqualTo(_mapper);
    }
}
