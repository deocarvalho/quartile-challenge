using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.StoreApi.Controllers;

namespace QuartileChallenge.Tests.Controllers;

public class ProductsControllerTests : TestBase
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId),
            new("Product 2", "Description 2", 20.99m, ValidStoreId)
        };

        _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithProduct()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId);
        _mockRepository.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(product.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(product.Id, returnedProduct.Id);
        Assert.Equal(product.Name, returnedProduct.Name);
        
        _mockRepository.Verify(repo => repo.GetByIdAsync(product.Id), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(nonexistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
    }

    [Fact]
    public async Task GetByStoreId_ReturnsOkWithProducts()
    {
        // Arrange
        var storeId = ValidStoreId;
        var products = new List<Product>
        {
            new(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, storeId),
            new("Product 2", "Description 2", 20.99m, storeId)
        };

        _mockRepository.Setup(repo => repo.GetByStoreIdAsync(storeId)).ReturnsAsync(products);

        // Act
        var result = await _controller.GetByStoreId(storeId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        Assert.All(returnedProducts, p => Assert.Equal(storeId, p.StoreId));
        
        _mockRepository.Verify(repo => repo.GetByStoreIdAsync(storeId), Times.Once);
    }

    [Fact]
    public async Task GetByCompanyId_ReturnsOkWithProducts()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var products = new List<Product>
        {
            new(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId),
            new("Product 2", "Description 2", 20.99m, ValidStoreId)
        };

        _mockRepository.Setup(repo => repo.GetByCompanyIdAsync(companyId)).ReturnsAsync(products);

        // Act
        var result = await _controller.GetByCompanyId(companyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        
        _mockRepository.Verify(repo => repo.GetByCompanyIdAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        Product? capturedProduct = null;
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), createdResult.ActionName);
        
        var returnedProduct = Assert.IsType<ProductDto>(createdResult.Value);
        Assert.NotEqual(Guid.Empty, returnedProduct.Id);
        Assert.Equal(createDto.Name, returnedProduct.Name);
        Assert.Equal(createDto.Description, returnedProduct.Description);
        Assert.Equal(createDto.Price, returnedProduct.Price);
        Assert.Equal(createDto.StoreId, returnedProduct.StoreId);
        Assert.True(returnedProduct.IsActive);

        Assert.NotNull(capturedProduct);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidIdAndDto_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId);
        var updateDto = new UpdateProductDto(
            TestData.Product.UpdatedName,
            TestData.Product.UpdatedDescription,
            TestData.Product.UpdatedPrice
        );

        _mockRepository.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _mockRepository.Setup(repo => repo.UpdateAsync(product)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(product.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        
        Assert.Equal(updateDto.Name, returnedProduct.Name);
        Assert.Equal(updateDto.Description, returnedProduct.Description);
        Assert.Equal(updateDto.Price, returnedProduct.Price);
        Assert.NotNull(returnedProduct.ModifiedAt);

        _mockRepository.Verify(repo => repo.GetByIdAsync(product.Id), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task Update_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        var updateDto = new UpdateProductDto(
            TestData.Product.UpdatedName,
            TestData.Product.UpdatedDescription,
            TestData.Product.UpdatedPrice
        );

        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update(nonexistentId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId);
        _mockRepository.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _mockRepository.Setup(repo => repo.UpdateAsync(product)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(product.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.False(product.IsActive); // Verify product was deactivated
        Assert.NotNull(product.ModifiedAt);

        _mockRepository.Verify(repo => repo.GetByIdAsync(product.Id), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Delete(nonexistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
} 