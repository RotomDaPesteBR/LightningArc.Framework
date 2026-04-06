using LightningArc.Data.ADO.Factories;
using LightningArc.Data.ADO.Tests.Mocks;

namespace LightningArc.Data.Tests.Factories;

public class RepositoryFactoryTests
{
    private readonly MockConnectionFactory _connectionFactory = new();
    private readonly MockMapper _mapper = new();

    [Test]
    public async Task Create_ShouldReturnRepositoryWithoutMapper_WhenNoMapperProvidedToFactory()
    {
        // Arrange
        RepositoryFactory factory = new(_connectionFactory);

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
        RepositoryFactory factory = new(_connectionFactory, _mapper);

        // Act
        TestRepository repository = factory.Create<TestRepository>();

        // Assert
        await Assert.That(repository).IsNotNull();
        await Assert.That(repository.InjectedMapper).IsEqualTo(_mapper);
    }
}


