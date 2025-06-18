using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Data.Commands;
using System.Text.Json;

namespace QuartileChallenge.Tests.Infrastructure.Commands;

public class InsertProductCommandTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly InsertProductCommand _command;
    private readonly Store _store;

    public InsertProductCommandTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuartileChallengeDb_Test;Trusted_Connection=True;")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Create store for testing
        _store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        _context.Stores.Add(_store);
        _context.SaveChanges();

        // Create stored procedure
        _context.Database.ExecuteSqlRaw(File.ReadAllText("../../../database/functions/fn_GetProductsAsJson.sql"));
        _context.Database.ExecuteSqlRaw(File.ReadAllText("../../../database/procedures/sp_InsertProduct.sql"));

        _command = new InsertProductCommand(_context);
    }

    [Fact]
    public async Task InsertProduct_WithValidData_ReturnsIdAndJson()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;

        // Act
        var (id, json) = await _command.ExecuteAsync(name, description, price, _store.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(json);
        
        var product = await _context.Products.FindAsync(id);
        Assert.NotNull(product);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(_store.Id, product.StoreId);
    }

    [Fact]
    public async Task InsertProduct_WithInvalidStore_ThrowsException()
    {
        // Arrange
        var invalidStoreId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _command.ExecuteAsync("Test", "Test", 99.99m, invalidStoreId));
    }

    [Theory]
    [InlineData("", "Description", 99.99)]
    [InlineData(null, "Description", 99.99)]
    [InlineData("Name", "Description", -1)]
    public async Task InsertProduct_WithInvalidData_ThrowsException(
        string? name, string description, decimal price)
    {
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _command.ExecuteAsync(name!, description, price, _store.Id));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}