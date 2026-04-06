using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LightningArc.Data.Abstractions.Mappers;
using LightningArc.Data.EntityFramework.Repositories;
using LightningArc.Data.ADO.Tests.Mocks;

namespace LightningArc.Data.Tests.Repositories;

public class EntityFrameworkRepositoryTests
{
    public class TestEntity { public int Id { get; set; } }
    
    public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    public class TestRepository : RepositoryBase<TestDbContext, TestEntity>
    {
        public TestRepository(TestDbContext context) : base(context) { }
        public TestRepository(TestDbContext context, IMapper? mapper, ILogger? logger) : base(context, mapper, logger) { }

        public DbSet<TestEntity> GetDbSet() => DbSet;
        public ILogger GetLogger() => Logger;
        public IMapper GetMapper() => Mapper;
    }

    private DbContextOptions<TestDbContext> CreateOptions() => new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    [Test]
    public async Task Repository_ShouldInitializeDbSet()
    {
        // Arrange
        using TestDbContext context = new(CreateOptions());
        TestRepository repository = new(context);

        // Act
        var dbSet = repository.GetDbSet();

        // Assert
        await Assert.That(dbSet).IsNotNull();
    }

    [Test]
    public async Task Logger_ShouldNotBeNull_EvenWhenNullProvided()
    {
        // Arrange
        using TestDbContext context = new(CreateOptions());
        TestRepository repository = new(context, null, null);

        // Act
        ILogger logger = repository.GetLogger();

        // Assert
        await Assert.That(logger).IsNotNull();
    }

    [Test]
    public async Task Mapper_ShouldThrow_WhenNotProvided()
    {
        // Arrange
        using TestDbContext context = new(CreateOptions());
        TestRepository repository = new(context);

        // Act & Assert
        await Assert.That(() => repository.GetMapper()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Mapper_ShouldReturnProvidedInstance()
    {
        // Arrange
        using TestDbContext context = new(CreateOptions());
        MockMapper mockMapper = new();
        TestRepository repository = new(context, mockMapper, null);

        // Act
        IMapper mapper = repository.GetMapper();

        // Assert
        await Assert.That(mapper).IsEqualTo(mockMapper);
    }
}


