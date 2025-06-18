using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Data.Queries;
using System.Text.Json;

namespace QuartileChallenge.Tests.Infrastructure.Queries;

public class GetProductsJsonQueryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetProductsJsonQuery _query;
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Store _store;

    public GetProductsJsonQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuartileChallengeDb_Test;Trusted_Connection=True;")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        
        // Setup test data
        _store = new Store(_companyId, "Test Store", "Test Location");
        _context.Stores.Add(_store);
        
        var product = new Product("Test Product", "Test Description", 99.99m, _store.Id);
        _context.Products.Add(product);
        
        _context.SaveChanges();

        // Create function
        _context.Database.ExecuteSqlRaw(File.ReadAllText("../../../database/functions/fn_GetProductsAsJson.sql"));
        
        _query = new GetProductsJsonQuery(_context);
    }

    [Fact]
    public async Task GetProductsJson_ReturnsValidJson()
    {
        // Act
        var json = await _query.ExecuteAsync();

        // Assert
        Assert.NotNull(json);
        var result = JsonSerializer.Deserialize<JsonDocument>(json);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetProductsJson_WithCompanyId_ReturnsFilteredResults()
    {
        // Act
        var json = await _query.ExecuteAsync(_companyId);

        // Assert
        Assert.NotNull(json);
        var result = JsonSerializer.Deserialize<JsonDocument>(json);
        Assert.NotNull(result);
        Assert.Contains(_companyId.ToString(), json);
    }

    [Fact]
    public async Task GetProductsJson_WithStoreId_ReturnsFilteredResults()
    {
        // Act
        var json = await _query.ExecuteAsync(null, _store.Id);

        // Assert
        Assert.NotNull(json);
        var result = JsonSerializer.Deserialize<JsonDocument>(json);
        Assert.NotNull(result);
        Assert.Contains(_store.Id.ToString(), json);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}