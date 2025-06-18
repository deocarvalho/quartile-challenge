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

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}