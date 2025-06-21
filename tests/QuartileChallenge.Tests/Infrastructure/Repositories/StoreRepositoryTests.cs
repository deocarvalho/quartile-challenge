using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace QuartileChallenge.Tests.Infrastructure.Repositories;

public class StoreRepositoryTests : TestBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly StoreRepository _repository;

    public StoreRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"StoreDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new StoreRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddStoreToDatabase()
    {
        // Arrange
        var store = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);

        // Act
        var result = await _repository.AddAsync(store);

        // Assert
        Assert.NotNull(result);
        var savedStore = await _context.Stores.FindAsync(result.Id);
        Assert.NotNull(savedStore);
        Assert.Equal(store.Name, savedStore.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsStore()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Test Store", "Test Location");
        await _context.Stores.AddAsync(store);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(store.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(store.Id, result.Id);
        Assert.Equal(store.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonexistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStores()
    {
        // Arrange
        var store1 = new Store(ValidCompanyId, "Store 1", "Location 1");
        var store2 = new Store(ValidCompanyId, "Store 2", "Location 2");
        
        await _context.Stores.AddRangeAsync(store1, store2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, s => s.Id == store1.Id);
        Assert.Contains(results, s => s.Id == store2.Id);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_ShouldReturnCompanyStores()
    {
        // Arrange
        var store1 = new Store(ValidCompanyId, "Store 1", "Location 1");
        var store2 = new Store(ValidCompanyId, "Store 2", "Location 2");
        var store3 = new Store(Guid.NewGuid(), "Store 3", "Location 3");

        await _context.Stores.AddRangeAsync(store1, store2, store3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByCompanyIdAsync(ValidCompanyId);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, s => Assert.Equal(ValidCompanyId, s.CompanyId));
    }

    [Fact]
    public async Task GetByCompanyIdAsync_WithNonexistentCompanyId_ReturnsEmpty()
    {
        // Arrange
        var nonexistentCompanyId = Guid.NewGuid();

        // Act
        var results = await _repository.GetByCompanyIdAsync(nonexistentCompanyId);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesStoreInDatabase()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Original Name", "Original Location");
        await _context.Stores.AddAsync(store);
        await _context.SaveChangesAsync();

        // Modify the store
        store.Update("Updated Name", "Updated Location");

        // Act
        await _repository.UpdateAsync(store);

        // Assert
        var updatedStore = await _context.Stores.FindAsync(store.Id);
        Assert.NotNull(updatedStore);
        Assert.Equal("Updated Name", updatedStore.Name);
        Assert.Equal("Updated Location", updatedStore.Location);
        Assert.NotNull(updatedStore.ModifiedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStoreFromDatabase()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Test Store", "Test Location");
        await _context.Stores.AddAsync(store);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(store);

        // Assert
        var deletedStore = await _context.Stores.FindAsync(store.Id);
        Assert.Null(deletedStore);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}