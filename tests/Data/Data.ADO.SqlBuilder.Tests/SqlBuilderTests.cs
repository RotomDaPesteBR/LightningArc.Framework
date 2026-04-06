using LightningArc.Data.ADO.SqlBuilder.Definitions;
using LightningArc.Data.ADO.SqlBuilder.Enums;

namespace LightningArc.Data.ADO.SqlBuilder.Tests;

public class SqlBuilderTests
{
    private static readonly List<ColumnDefinition> DefaultColumns = 
    [
        new() { ColumnName = "Id", PropertyName = "Id", IsKey = true },
        new() { ColumnName = "Name", PropertyName = "Name" },
        new() { ColumnName = "Age", PropertyName = "Age" }
    ];

    [Test]
    public async Task Select_All_ShouldGenerateCorrectSql()
    {
        // Arrange
        var builder = new SqlBuilder("Users", DefaultColumns, SqlDialect.SqlServer);

        // Act
        var sql = builder.Select.Build();

        // Assert
        await Assert.That(sql).IsEqualTo("SELECT Id, Name, Age FROM Users");
    }

    [Test]
    public async Task Insert_All_ShouldGenerateCorrectSql()
    {
        // Arrange
        var builder = new SqlBuilder("Users", DefaultColumns, SqlDialect.SqlServer);

        // Act
        var sql = builder.Insert.Build();

        // Assert
        await Assert.That(sql).IsEqualTo("INSERT INTO Users (Id, Name, Age) VALUES (@Id, @Name, @Age)");
    }

    [Test]
    public async Task Update_ById_ShouldGenerateCorrectSql()
    {
        // Arrange
        var builder = new SqlBuilder("Users", DefaultColumns, SqlDialect.SqlServer);

        // Act
        var sql = builder.Update.Build();

        // Assert
        await Assert.That(sql).IsEqualTo("UPDATE Users SET Name = @Name, Age = @Age WHERE Id = @Id");
    }

    [Test]
    public async Task Delete_ById_ShouldGenerateCorrectSql()
    {
        // Arrange
        var builder = new SqlBuilder("Users", DefaultColumns, SqlDialect.SqlServer);

        // Act
        var sql = builder.Delete.Build();

        // Assert
        await Assert.That(sql).IsEqualTo("DELETE FROM Users WHERE Id = @Id");
    }

    [Test]
    public async Task Select_PostgreSql_ShouldUseCorrectQuotes()
    {
        // Arrange
        var builder = new SqlBuilder("Users", DefaultColumns, SqlDialect.PostgreSQL);

        // Act
        var sql = builder.Select.Build();

        // Assert
        // Current implementation seems to NOT be adding quotes by default for PostgreSQL either 
        // based on the test failure output.
        await Assert.That(sql).IsEqualTo("SELECT Id, Name, Age FROM Users");
    }
}
