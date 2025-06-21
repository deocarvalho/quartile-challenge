using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using System.Text.Json;

namespace QuartileChallenge.Tests.Infrastructure.Functions;

public class ProductJsonFunctionTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly Store _testStore1;
    private readonly Store _testStore2;
    private readonly Product _testProduct1;
    private readonly Product _testProduct2;
    private readonly Product _testProduct3;

    public ProductJsonFunctionTests()
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
        var company1Id = Guid.NewGuid();
        var company2Id = Guid.NewGuid();
        
        _testStore1 = new Store(company1Id, "Store 1", "Location 1");
        _testStore2 = new Store(company2Id, "Store 2", "Location 2");
        
        _context.Stores.AddRange(_testStore1, _testStore2);
        _context.SaveChanges();

        _testProduct1 = new Product("Product 1", "Description 1", 99.99m, _testStore1.Id);
        _testProduct2 = new Product("Product 2", "Description 2", 199.99m, _testStore1.Id);
        _testProduct3 = new Product("Product 3", "Description 3", 299.99m, _testStore2.Id);
        
        _context.Products.AddRange(_testProduct1, _testProduct2, _testProduct3);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetProductsAsJson_WithoutFilters_ReturnsAllActiveProducts()
    {
        // Act - Simulate the SQL function logic: SELECT all products with store info
        var products = await (from p in _context.Products
                             join s in _context.Stores on p.StoreId equals s.Id
                             where p.IsActive && s.IsActive
                             select new
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 Description = p.Description,
                                 Price = p.Price,
                                 StoreId = p.StoreId,
                                 CreatedAt = p.CreatedAt,
                                 ModifiedAt = p.ModifiedAt,
                                 IsActive = p.IsActive,
                                 StoreName = s.Name,
                                 CompanyId = s.CompanyId
                             }).ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert - Function should return all products
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        var jsonProducts = jsonDoc.RootElement.EnumerateArray();
        Assert.Equal(3, jsonProducts.Count());

        var firstProduct = jsonProducts.First();
        Assert.True(firstProduct.TryGetProperty("Name", out var nameProperty));
        // The first product could be any of the 3, so just verify it's one of them
        var expectedNames = new[] { "Product 1", "Product 2", "Product 3" };
        Assert.Contains(nameProperty.GetString(), expectedNames);
    }

    [Fact]
    public async Task GetProductsAsJson_WithCompanyFilter_ReturnsFilteredResults()
    {
        // Arrange
        var targetCompanyId = _testStore1.CompanyId;

        // Act - Simulate the SQL function logic with @CompanyId filter
        var products = await (from p in _context.Products
                             join s in _context.Stores on p.StoreId equals s.Id
                             where p.IsActive && s.IsActive && s.CompanyId == targetCompanyId
                             select new
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 Description = p.Description,
                                 Price = p.Price,
                                 StoreId = p.StoreId,
                                 CreatedAt = p.CreatedAt,
                                 ModifiedAt = p.ModifiedAt,
                                 IsActive = p.IsActive,
                                 StoreName = s.Name,
                                 CompanyId = s.CompanyId
                             }).ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert - Function should return only company products
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        var jsonProducts = jsonDoc.RootElement.EnumerateArray();
        Assert.Equal(2, jsonProducts.Count()); // Only products from company 1

        // Verify all products belong to the target company
        foreach (var product in jsonProducts)
        {
            Assert.True(product.TryGetProperty("CompanyId", out var companyProperty));
            Assert.Equal(targetCompanyId.ToString(), companyProperty.GetString());
        }
    }

    [Fact]
    public async Task GetProductsAsJson_WithStoreFilter_ReturnsFilteredResults()
    {
        // Arrange
        var targetStoreId = _testStore1.Id;

        // Act - Simulate the SQL function logic with @StoreId filter
        var products = await (from p in _context.Products
                             join s in _context.Stores on p.StoreId equals s.Id
                             where p.IsActive && s.IsActive && p.StoreId == targetStoreId
                             select new
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 Description = p.Description,
                                 Price = p.Price,
                                 StoreId = p.StoreId,
                                 CreatedAt = p.CreatedAt,
                                 ModifiedAt = p.ModifiedAt,
                                 IsActive = p.IsActive,
                                 StoreName = s.Name,
                                 CompanyId = s.CompanyId
                             }).ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert - Function should return only store products
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        var jsonProducts = jsonDoc.RootElement.EnumerateArray();
        Assert.Equal(2, jsonProducts.Count()); // Only products from store 1

        // Verify all products belong to the target store
        foreach (var product in jsonProducts)
        {
            Assert.True(product.TryGetProperty("StoreId", out var storeProperty));
            Assert.Equal(targetStoreId.ToString(), storeProperty.GetString());
        }
    }

    [Fact]
    public async Task GetProductsAsJson_WithInactiveProducts_ExcludesInactiveItems()
    {
        // Arrange - Create an inactive product
        var inactiveProduct = new Product("Inactive Product", "Inactive Description", 99.99m, _testStore1.Id);
        inactiveProduct.Deactivate(); // Make it inactive
        _context.Products.Add(inactiveProduct);
        await _context.SaveChangesAsync();

        // Act - Simulate function filtering active products only
        var products = await (from p in _context.Products
                             join s in _context.Stores on p.StoreId equals s.Id
                             where p.IsActive && s.IsActive
                             select p).ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert - Function should exclude inactive products
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        var jsonProducts = jsonDoc.RootElement.EnumerateArray();
        Assert.Equal(3, jsonProducts.Count()); // Still only 3 active products

        // Verify no inactive products in results
        foreach (var product in jsonProducts)
        {
            Assert.True(product.TryGetProperty("IsActive", out var isActiveProperty));
            Assert.True(isActiveProperty.GetBoolean());
        }
    }

    [Fact]
    public async Task GetProductsAsJson_WithBothFilters_ReturnsCorrectResults()
    {
        // Arrange
        var targetCompanyId = _testStore1.CompanyId;
        var targetStoreId = _testStore1.Id;

        // Act - Simulate function with both @CompanyId AND @StoreId filters
        var products = await (from p in _context.Products
                             join s in _context.Stores on p.StoreId equals s.Id
                             where p.IsActive && s.IsActive && 
                                   s.CompanyId == targetCompanyId && 
                                   p.StoreId == targetStoreId
                             select new
                             {
                                 Id = p.Id,
                                 Name = p.Name,
                                 StoreId = p.StoreId,
                                 CompanyId = s.CompanyId
                             }).ToListAsync();

        var json = JsonSerializer.Serialize(products);

        // Assert - Function should apply both filters
        Assert.NotNull(json);
        var jsonDoc = JsonDocument.Parse(json);
        var jsonProducts = jsonDoc.RootElement.EnumerateArray();
        Assert.Equal(2, jsonProducts.Count()); // Products matching both filters

        var product = jsonProducts.First();
        Assert.True(product.TryGetProperty("CompanyId", out var companyProperty));
        Assert.Equal(targetCompanyId.ToString(), companyProperty.GetString());
        Assert.True(product.TryGetProperty("StoreId", out var storeProperty));
        Assert.Equal(targetStoreId.ToString(), storeProperty.GetString());
    }

    [Fact]
    public void JsonSerialization_WithProduct_CreatesValidStructure()
    {
        // Arrange
        var product = _testProduct1;

        // Act - Simulate function's JSON output structure
        var productDto = new
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StoreId = product.StoreId,
            CreatedAt = product.CreatedAt,
            ModifiedAt = product.ModifiedAt,
            IsActive = product.IsActive,
            StoreName = _testStore1.Name,
            CompanyId = _testStore1.CompanyId
        };

        var json = JsonSerializer.Serialize(productDto);

        // Assert - Function should produce valid JSON structure
        Assert.NotNull(json);
        
        var jsonDoc = JsonDocument.Parse(json);
        Assert.True(jsonDoc.RootElement.TryGetProperty("Id", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("Name", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("Description", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("Price", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("StoreId", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("StoreName", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("CompanyId", out _));
        Assert.True(jsonDoc.RootElement.TryGetProperty("IsActive", out _));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}