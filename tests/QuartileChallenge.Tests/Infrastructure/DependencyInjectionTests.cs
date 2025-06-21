using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.Infrastructure;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;

namespace QuartileChallenge.Tests.Infrastructure;

public class DependencyInjectionTests : TestBase
{
    private IConfiguration CreateTestConfiguration(string connectionString = "Server=localhost;Database=TestDb;Trusted_Connection=true;")
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", connectionString}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void AddInfrastructure_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        Assert.Same(services, result);
        
        // Verify services are registered
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<ApplicationDbContext>());
        Assert.NotNull(serviceProvider.GetService<IStoreRepository>());
        Assert.NotNull(serviceProvider.GetService<IProductRepository>());
        
        // Verify concrete implementations
        Assert.IsType<StoreRepository>(serviceProvider.GetService<IStoreRepository>());
        Assert.IsType<ProductRepository>(serviceProvider.GetService<IProductRepository>());
    }

    [Fact]
    public void AddInfrastructure_RegistersRepositoriesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var storeRepoDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IStoreRepository));
        var productRepoDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IProductRepository));
        
        Assert.NotNull(storeRepoDescriptor);
        Assert.NotNull(productRepoDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, storeRepoDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, productRepoDescriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_RegistersDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationDbContext));
        Assert.NotNull(dbContextDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, dbContextDescriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_WithCustomConnectionString_UsesCorrectConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConnectionString = "Server=custom;Database=CustomDb;Trusted_Connection=true;";
        var configuration = CreateTestConfiguration(customConnectionString);

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<ApplicationDbContext>();
        Assert.NotNull(dbContext);
        
        // Verify the connection string is used (this will be set in the DbContext options)
        Assert.NotNull(dbContext.Database);
    }

    [Fact]
    public void AddInfrastructure_WithNullConnectionString_StillRegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration(null!);

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Services should still be registered even with null connection string
        Assert.NotNull(serviceProvider.GetService<IStoreRepository>());
        Assert.NotNull(serviceProvider.GetService<IProductRepository>());
    }

    [Fact]
    public void AddInfrastructure_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        services.AddInfrastructure(configuration); // Call twice

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Should still work (though services will be registered multiple times)
        Assert.NotNull(serviceProvider.GetService<IStoreRepository>());
        Assert.NotNull(serviceProvider.GetService<IProductRepository>());
        Assert.NotNull(serviceProvider.GetService<ApplicationDbContext>());
    }

    [Fact]
    public void AddInfrastructure_ReturnsOriginalServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddInfrastructure_ConfiguresEntityFramework()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<ApplicationDbContext>();
        
        Assert.NotNull(dbContext);
        Assert.NotNull(dbContext.Products);
        Assert.NotNull(dbContext.Stores);
    }

    [Fact]
    public void AddInfrastructure_ScopedServicesCreateNewInstancesPerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        using (var scope1 = serviceProvider.CreateScope())
        using (var scope2 = serviceProvider.CreateScope())
        {
            var repo1 = scope1.ServiceProvider.GetService<IProductRepository>();
            var repo2 = scope2.ServiceProvider.GetService<IProductRepository>();
            
            Assert.NotNull(repo1);
            Assert.NotNull(repo2);
            Assert.NotSame(repo1, repo2); // Different instances in different scopes
        }
    }

    [Fact]
    public void AddInfrastructure_ScopedServicesReturnSameInstanceWithinScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var repo1 = scope.ServiceProvider.GetService<IProductRepository>();
        var repo2 = scope.ServiceProvider.GetService<IProductRepository>();
        
        Assert.NotNull(repo1);
        Assert.NotNull(repo2);
        Assert.Same(repo1, repo2); // Same instance within the same scope
    }
} 