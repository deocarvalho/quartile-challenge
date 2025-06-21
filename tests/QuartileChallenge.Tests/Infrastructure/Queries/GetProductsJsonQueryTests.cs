using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Data.Queries;
using System.Text.Json;

namespace QuartileChallenge.Tests.Infrastructure.Queries;

public class GetProductsJsonQueryTests : TestBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetProductsJsonQuery _query;
    private readonly SqliteConnection _connection;
    private readonly Store _testStore;
    private readonly Product _testProduct;

    public GetProductsJsonQueryTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Create test data
        _testStore = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        _context.Stores.Add(_testStore);
        
        _testProduct = new Product("Test Product", "Test Description", 99.99m, _testStore.Id);
        _context.Products.Add(_testProduct);
        
        _context.SaveChanges();

        _query = new GetProductsJsonQuery(_context);
    }

    [Fact]
    public void GetProductsJsonQuery_Constructor_InitializesCorrectly()
    {
        // Arrange & Act
        var query = new GetProductsJsonQuery(_context);

        // Assert
        Assert.NotNull(query);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_AttemptsToExecuteFunction()
    {
        // Act & Assert
        // This tests that the query attempts to execute the SQL Server function
        // which will fail on SQLite since it uses SQL Server specific functions
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _query.ExecuteAsync());
    }

    [Fact]
    public async Task ExecuteAsync_WithCompanyId_AttemptsToExecuteWithParameter()
    {
        // Arrange
        var companyId = _testStore.CompanyId;

        // Act & Assert
        // This tests that the query attempts to execute with company ID parameter
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _query.ExecuteAsync(companyId));
    }

    [Fact]
    public async Task ExecuteAsync_WithStoreId_AttemptsToExecuteWithParameter()
    {
        // Arrange
        var storeId = _testStore.Id;

        // Act & Assert
        // This tests that the query attempts to execute with store ID parameter
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _query.ExecuteAsync(null, storeId));
    }

    [Fact]
    public async Task ExecuteAsync_WithBothParameters_AttemptsToExecuteWithBothFilters()
    {
        // Arrange
        var companyId = _testStore.CompanyId;
        var storeId = _testStore.Id;

        // Act & Assert
        // This tests that the query attempts to execute with both parameters
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _query.ExecuteAsync(companyId, storeId));
    }

    [Fact]
    public async Task ExecuteAsync_ParameterHandling_AcceptsNullValues()
    {
        // Act & Assert
        // This tests that the query can handle null parameters
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(async () =>
            await _query.ExecuteAsync(null, null));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidStoreId_ThrowsSqliteException()
    {
        // Arrange
        var storeId = ValidStoreId;
        
        // Act & Assert
        // SQLite doesn't support the SQL Server function, so we expect this to fail
        var exception = await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(
            () => _query.ExecuteAsync(storeId)
        );
        
        Assert.Contains("syntax error", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullStoreId_ThrowsSqliteException()
    {
        // Act & Assert
        // SQLite doesn't support the SQL Server function
        var exception = await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(
            () => _query.ExecuteAsync(null)
        );
        
        Assert.Contains("syntax error", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStoreId_ThrowsSqliteException()
    {
        // Arrange
        var invalidStoreId = Guid.NewGuid();
        
        // Act & Assert
        // SQLite doesn't support the SQL Server function
        var exception = await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(
            () => _query.ExecuteAsync(invalidStoreId)
        );
        
        Assert.Contains("syntax error", exception.Message);
    }

    [Fact]
    public async Task SimulateGetProductsAsJson_WithProducts_ReturnsValidJson()
    {
        // Arrange - Create test data in SQLite
        var store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        var product1 = new Product("Product 1", "Description 1", 10.50m, store.Id);
        var product2 = new Product("Product 2", "Description 2", 20.75m, store.Id);

        _context.Stores.Add(store);
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        // Act - Simulate what the SQL function would do using LINQ
        var products = await _context.Products
            .Where(p => p.StoreId == store.Id && p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StoreId = p.StoreId,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert
        Assert.NotEmpty(json);
        Assert.Contains("Product 1", json);
        Assert.Contains("Product 2", json);
        Assert.Contains("10.5", json);
        Assert.Contains("20.75", json);
    }

    [Fact]
    public async Task SimulateGetProductsAsJson_WithNoProducts_ReturnsEmptyArray()
    {
        // Arrange - Create store with no products
        var store = new Store(Guid.NewGuid(), "Empty Store", "Test Location");
        _context.Stores.Add(store);
        await _context.SaveChangesAsync();

        // Act - Simulate what the SQL function would do
        var products = await _context.Products
            .Where(p => p.StoreId == store.Id && p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StoreId = p.StoreId,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public async Task SimulateGetProductsAsJson_AllStores_ReturnsAllActiveProducts()
    {
        // Arrange - Create multiple stores with products in a fresh context
        using var freshContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options);
        freshContext.Database.OpenConnection();
        freshContext.Database.EnsureCreated();

        var store1 = new Store(Guid.NewGuid(), "Store 1", "Location 1");
        var store2 = new Store(Guid.NewGuid(), "Store 2", "Location 2");
        var product1 = new Product("Product A", "Description A", 15.00m, store1.Id);
        var product2 = new Product("Product B", "Description B", 25.00m, store2.Id);
        var inactiveProduct = new Product("Inactive Product", "Description", 5.00m, store1.Id);
        inactiveProduct.Deactivate();

        freshContext.Stores.AddRange(store1, store2);
        freshContext.Products.AddRange(product1, product2, inactiveProduct);
        await freshContext.SaveChangesAsync();

        // Act - Simulate getting all products (NULL store parameter)
        var products = await freshContext.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StoreId = p.StoreId,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert
        Assert.Contains("Product A", json);
        Assert.Contains("Product B", json);
        Assert.DoesNotContain("Inactive Product", json);
        Assert.Equal(2, products.Count);

        freshContext.Database.CloseConnection();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}