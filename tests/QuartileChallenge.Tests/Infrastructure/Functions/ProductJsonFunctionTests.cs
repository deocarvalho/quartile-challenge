using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using System.Text.Json;

namespace QuartileChallenge.Tests.Infrastructure.Functions;

public class ProductJsonFunctionTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Store _store;
    private readonly Product _product;

    public ProductJsonFunctionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuartileChallengeDb_Test;Trusted_Connection=True;")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Setup test data
        _store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        _context.Stores.Add(_store);
        
        _product = new Product("Test Product", "Test Description", 99.99m, _store.Id);
        _context.Products.Add(_product);
        
        _context.SaveChanges();

        // Create function
        _context.Database.ExecuteSqlRaw(File.ReadAllText("../../../database/functions/fn_GetProductsAsJson.sql"));
    }

    [Fact]
    public async Task GetProductsAsJson_ReturnsValidJson()
    {
        // Act
        var result = await _context.Database
            .SqlQuery<string>($"SELECT dbo.fn_GetProductsAsJson(NULL, NULL) as json")
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var products = jsonDoc.RootElement.EnumerateArray();
        Assert.True(products.Any());
    }

    [Fact]
    public async Task GetProductsAsJson_WithCompanyId_ReturnsFilteredResults()
    {
        // Act
        var result = await _context.Database
            .SqlQuery<string>($"SELECT dbo.fn_GetProductsAsJson('{_store.CompanyId}', NULL) as json")
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var products = jsonDoc.RootElement.EnumerateArray();
        Assert.True(products.Any());
        Assert.All(products, p => Assert.Equal(_store.CompanyId.ToString(), 
            p.GetProperty("companyId").GetString()));
    }

    [Fact]
    public async Task GetProductsAsJson_WithStoreId_ReturnsFilteredResults()
    {
        // Act
        var result = await _context.Database
            .SqlQuery<string>($"SELECT dbo.fn_GetProductsAsJson(NULL, '{_store.Id}') as json")
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var products = jsonDoc.RootElement.EnumerateArray();
        Assert.True(products.Any());
        Assert.All(products, p => Assert.Equal(_store.Id.ToString(), 
            p.GetProperty("storeId").GetString()));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}