using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;

namespace QuartileChallenge.Tests.Infrastructure.Repositories;

// Concrete implementation for testing
public class TestRepository : BaseRepository<Product>
{
    public TestRepository(ApplicationDbContext context) : base(context) { }
}

public class BaseRepositoryTests : TestBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TestRepository _repository;
    private readonly Store _testStore;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        // Create a test store to satisfy foreign key constraints
        _testStore = new Store(ValidCompanyId, "Test Store", "Test Location");
        _context.Stores.Add(_testStore);
        _context.SaveChanges();

        _repository = new TestRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var product = new Product("Test Product", "Test Description", 99.99m, _testStore.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
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
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var product1 = new Product("Product 1", "Description 1", 10.99m, _testStore.Id);
        var product2 = new Product("Product 2", "Description 2", 20.99m, _testStore.Id);
        
        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var products = result.ToList();
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p.Id == product1.Id);
        Assert.Contains(products, p => p.Id == product2.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithNoEntities_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        // Arrange
        var product = new Product("Test Product", "Test Description", 99.99m, _testStore.Id);

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.Equal(product, result);
        
        // Verify it was added to database
        var savedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(product.Name, savedProduct.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntityInDatabase()
    {
        // Arrange
        var product = new Product("Original Name", "Original Description", 99.99m, _testStore.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Modify the product
        product.Update("Updated Name", "Updated Description", 149.99m);

        // Act
        await _repository.UpdateAsync(product);

        // Assert
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Name", updatedProduct.Name);
        Assert.Equal("Updated Description", updatedProduct.Description);
        Assert.Equal(149.99m, updatedProduct.Price);
        Assert.NotNull(updatedProduct.ModifiedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntityFromDatabase()
    {
        // Arrange
        var product = new Product("Test Product", "Test Description", 99.99m, _testStore.Id);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(product);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
} 