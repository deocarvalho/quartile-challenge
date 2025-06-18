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
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductFunctionsTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _functions = new ProductFunctions(_mockRepository.Object);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId),
            new("Product 2", "Description 2", 199.99m, ValidStoreId)
        };

        _mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(products);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.GetProducts(mockRequest.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new Product(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.GetProductById(mockRequest.Object, product.Id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.GetProductById(mockRequest.Object, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId)).ReturnsAsync((Product?)null);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.GetProductById(mockRequest.Object, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetProductsByStore_WithValidStoreId_ReturnsProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new(TestData.Product.ValidName, TestData.Product.ValidDescription, TestData.Product.ValidPrice, ValidStoreId),
            new("Product 2", "Description 2", 199.99m, ValidStoreId)
        };

        _mockRepository.Setup(repo => repo.GetByStoreIdAsync(ValidStoreId))
            .ReturnsAsync(products);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.GetProductsByStore(mockRequest.Object, ValidStoreId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var createDto = new CreateProductDto(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        var requestBody = JsonSerializer.Serialize(createDto);
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock(requestBody);

        Product? savedProduct = null;
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => savedProduct = p)
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _functions.CreateProduct(mockRequest.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(savedProduct);
        Assert.Equal(createDto.Name, savedProduct.Name);
        Assert.Equal(createDto.Description, savedProduct.Description);
        Assert.Equal(createDto.Price, savedProduct.Price);
        Assert.Equal(createDto.StoreId, savedProduct.StoreId);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        var existingProduct = new Product(
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
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock(requestBody);

        _mockRepository.Setup(repo => repo.GetByIdAsync(existingProduct.Id))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _functions.UpdateProduct(mockRequest.Object, existingProduct.Id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(updateDto.Name, existingProduct.Name);
        Assert.Equal(updateDto.Description, existingProduct.Description);
        Assert.Equal(updateDto.Price, existingProduct.Price);
        Assert.NotNull(existingProduct.ModifiedAt);
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
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock(requestBody);

        // Act
        var result = await _functions.UpdateProduct(mockRequest.Object, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
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
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock(requestBody);

        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _functions.UpdateProduct(mockRequest.Object, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var product = new Product(
            TestData.Product.ValidName,
            TestData.Product.ValidDescription,
            TestData.Product.ValidPrice,
            ValidStoreId
        );

        _mockRepository.Setup(repo => repo.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.DeleteProduct(mockRequest.Object, product.Id.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        Assert.False(product.IsActive);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.DeleteProduct(mockRequest.Object, "invalid-guid");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Product?)null);

        var mockRequest = FunctionTestHelper.CreateHttpRequestMock();

        // Act
        var result = await _functions.DeleteProduct(mockRequest.Object, nonexistentId.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}