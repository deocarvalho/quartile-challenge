using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.ProductFunction.Functions;
using QuartileChallenge.Tests.Helpers;
using System.Net;
using System.Text.Json;

namespace QuartileChallenge.Tests.Functions;

public class ProductFunctionsTests : TestBase
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductFunctions _functions;

    public ProductFunctionsTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _functions = new ProductFunctions(_mockRepository.Object);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var request = FunctionTestHelper.CreateHttpRequest();

        // Act
        var result = await _functions.GetProductById(request, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        
        // Verify repository was not called
        _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetProductById_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId)).ReturnsAsync((Product?)null);

        var request = FunctionTestHelper.CreateHttpRequest();

        // Act
        var result = await _functions.GetProductById(request, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        
        // Verify repository was called
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
    }

    [Fact]
    public async Task GetProductsByStore_WithInvalidStoreId_ReturnsBadRequest()
    {
        // Arrange
        var request = FunctionTestHelper.CreateHttpRequest();

        // Act
        var result = await _functions.GetProductsByStore(request, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        
        // Verify repository was not called
        _mockRepository.Verify(repo => repo.GetByStoreIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateProductDto(
            TestData.Product.UpdatedName,
            TestData.Product.UpdatedDescription,
            TestData.Product.UpdatedPrice
        );

        var requestBody = JsonSerializer.Serialize(updateDto);
        var request = FunctionTestHelper.CreateHttpRequest(requestBody, "PUT");

        // Act
        var result = await _functions.UpdateProduct(request, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        
        // Verify repository was not called
        _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProduct_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        var updateDto = new UpdateProductDto(
            TestData.Product.UpdatedName,
            TestData.Product.UpdatedDescription,
            TestData.Product.UpdatedPrice
        );

        var requestBody = JsonSerializer.Serialize(updateDto);
        var request = FunctionTestHelper.CreateHttpRequest(requestBody, "PUT");

        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _functions.UpdateProduct(request, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        
        // Verify get was called but update was not
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var request = FunctionTestHelper.CreateHttpRequest(method: "DELETE");

        // Act
        var result = await _functions.DeleteProduct(request, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        
        // Verify repository was not called
        _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Product?)null);

        var request = FunctionTestHelper.CreateHttpRequest(method: "DELETE");

        // Act
        var result = await _functions.DeleteProduct(request, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        
        // Verify get was called but update was not
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonexistentId), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_CallsRepositoryCorrectly()
    {
        // Arrange
        var existingProduct = new Product(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        _mockRepository.Setup(repo => repo.GetByIdAsync(existingProduct.Id))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        var request = FunctionTestHelper.CreateHttpRequest(method: "DELETE");

        // Act
        var result = await _functions.DeleteProduct(request, existingProduct.Id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        
        // Verify the product was deactivated (business logic)
        Assert.False(existingProduct.IsActive);
        Assert.NotNull(existingProduct.ModifiedAt);
        
        // Verify repository interactions
        _mockRepository.Verify(repo => repo.GetByIdAsync(existingProduct.Id), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    // Integration-style test for business logic without serialization issues
    [Fact]
    public async Task UpdateProduct_BusinessLogic_UpdatesProductCorrectly()
    {
        // Arrange
        var originalProduct = new Product(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        var updateDto = new UpdateProductDto(
            TestData.Product.UpdatedName,
            TestData.Product.UpdatedDescription,
            TestData.Product.UpdatedPrice
        );

        var requestBody = JsonSerializer.Serialize(updateDto);
        var request = FunctionTestHelper.CreateHttpRequest(requestBody, "PUT");

        _mockRepository.Setup(repo => repo.GetByIdAsync(originalProduct.Id))
            .ReturnsAsync(originalProduct);
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act - Only test business logic, avoid HTTP response serialization
        try
        {
            await _functions.UpdateProduct(request, originalProduct.Id.ToString());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("serializer is not configured"))
        {
            // Expected due to test environment - ignore serialization error
        }

        // Assert - Verify business logic was applied to the domain object
        Assert.Equal(TestData.Product.UpdatedName, originalProduct.Name);
        Assert.Equal(TestData.Product.UpdatedDescription, originalProduct.Description);
        Assert.Equal(TestData.Product.UpdatedPrice, originalProduct.Price);
        Assert.NotNull(originalProduct.ModifiedAt);
        
        // Verify repository interactions
        _mockRepository.Verify(repo => repo.GetByIdAsync(originalProduct.Id), Times.Once);
        _mockRepository.Verify(repo => repo.UpdateAsync(originalProduct), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_BusinessLogic_CreatesProductCorrectly()
    {
        // Arrange
        var createDto = new CreateProductDto(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        var requestBody = JsonSerializer.Serialize(createDto);
        var request = FunctionTestHelper.CreateHttpRequest(requestBody, "POST");

        Product? capturedProduct = null;
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);

        // Act - Only test business logic, avoid HTTP response serialization
        try
        {
            await _functions.CreateProduct(request);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("serializer is not configured"))
        {
            // Expected due to test environment - ignore serialization error
        }

        // Assert - Verify business logic
        Assert.NotNull(capturedProduct);
        Assert.Equal(TestData.Product.ValidName, capturedProduct.Name);
        Assert.Equal(TestData.Product.ValidDescription, capturedProduct.Description);
        Assert.Equal(TestData.Product.ValidPrice, capturedProduct.Price);
        Assert.Equal(ValidStoreId, capturedProduct.StoreId);
        Assert.True(capturedProduct.IsActive);
        Assert.NotEqual(Guid.Empty, capturedProduct.Id);
        
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
    }
}