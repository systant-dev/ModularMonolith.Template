using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Storage;
using Moq;
using System.Data.Common;

namespace ModularMonolith.Template.SharedKernel.Tests.Storage;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Storage)]
public sealed class SqlServerConnectionFactoryTests
{
    #region Constant Declarations

    private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=ModularMonolith;Trusted_Connection=True";

    #endregion

    #region Test Method Declarations

    [Fact]
    public void Constructor_ShouldNormaliseConnectionString()
    {
        SqlServerConnectionFactory factory = new(ConnectionString);

        factory.ConnectionString.Should().Contain("Data Source=(localdb)\\mssqllocaldb");
        factory.ConnectionString.Should().Contain("Initial Catalog=ModularMonolith");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_ForEmptyConnectionString(string? connectionString)
    {
        Func<SqlServerConnectionFactory> act = () => new SqlServerConnectionFactory(connectionString!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_ForInvalidConnectionString()
    {
        Func<SqlServerConnectionFactory> act = () => new SqlServerConnectionFactory("Not a valid=;connection=string=broken");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task CreateOpenConnectionAsync_WhenOpenFails_DisposesConnection()
    {
        Mock<DbConnection> connection = new();
        connection.Setup(dbConnection => dbConnection.OpenAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("Simulated open failure."));

        SqlServerConnectionFactory factory = new(ConnectionString, () => connection.Object);

        Func<Task> act = () => factory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
        connection.Verify(dbConnection => dbConnection.DisposeAsync(), Times.Once);
    }

    #endregion
}
