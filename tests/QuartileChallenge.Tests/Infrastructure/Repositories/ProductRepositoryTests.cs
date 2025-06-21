using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace QuartileChallenge.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests : TestBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;
    private readonly Store _testStore;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProductDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
        
        // Create a test store
        _testStore = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        _context.Stores.Add(_testStore);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, 
                                TestData.Product.ValidDescription,
                                TestData.Product.ValidPrice, 
                                _testStore.Id);

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.NotNull(result);
        var savedProduct = await _context.Products.FindAsync(result.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(product.Name, savedProduct.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
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
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var product1 = new Product("Product 1", "Description 1", 10.99m, _testStore.Id);
        var product2 = new Product("Product 2", "Description 2", 20.99m, _testStore.Id);
        
        await _context.Products.AddRangeAsync(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, p => p.Id == product1.Id);
        Assert.Contains(results, p => p.Id == product2.Id);
    }

    [Fact]
    public async Task GetByStoreIdAsync_ShouldReturnStoreProducts()
    {
        // Arrange
        var product1 = new Product("Product 1", "Description 1", 10.99m, _testStore.Id);
        var product2 = new Product("Product 2", "Description 2", 20.99m, _testStore.Id);
        var product3 = new Product("Product 3", "Description 3", 30.99m, Guid.NewGuid());

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByStoreIdAsync(_testStore.Id);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, p => Assert.Equal(_testStore.Id, p.StoreId));
    }

    [Fact]
    public async Task GetByStoreIdAsync_WithNonexistentStoreId_ReturnsEmpty()
    {
        // Arrange
        var nonexistentStoreId = Guid.NewGuid();

        // Act
        var results = await _repository.GetByStoreIdAsync(nonexistentStoreId);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_ShouldReturnCompanyProducts()
    {
        // Arrange
        var product1 = new Product("Product 1", "Description 1", 10.99m, _testStore.Id);
        var product2 = new Product("Product 2", "Description 2", 20.99m, _testStore.Id);
        
        // Create another store with different company
        var otherStore = new Store(Guid.NewGuid(), "Other Store", "Other Location");
        _context.Stores.Add(otherStore);
        var product3 = new Product("Product 3", "Description 3", 30.99m, otherStore.Id);

        await _context.Products.AddRangeAsync(product1, product2, product3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByCompanyIdAsync(ValidCompanyId);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, p => Assert.Equal(_testStore.Id, p.StoreId));
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
    public async Task UpdateAsync_UpdatesProductInDatabase()
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
    public async Task DeleteAsync_RemovesProductFromDatabase()
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
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}