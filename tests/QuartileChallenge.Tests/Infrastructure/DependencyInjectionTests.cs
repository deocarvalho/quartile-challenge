using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.Infrastructure;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;

namespace QuartileChallenge.Tests.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Data Source=:memory:;"}
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ApplicationDbContext>());
        Assert.NotNull(serviceProvider.GetService<IStoreRepository>());
        Assert.NotNull(serviceProvider.GetService<IProductRepository>());

        Assert.IsType<StoreRepository>(serviceProvider.GetService<IStoreRepository>());
        Assert.IsType<ProductRepository>(serviceProvider.GetService<IProductRepository>());
    }
}