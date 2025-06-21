using QuartileChallenge.Core.DTOs;

namespace QuartileChallenge.Tests.DTOs;

public class ProductDtoTests : TestBase
{
    [Fact]
    public void ProductDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var storeId = ValidStoreId;
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var isActive = true;

        // Act
        var dto = new ProductDto(id, name, description, price, storeId, createdAt, modifiedAt, isActive);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(description, dto.Description);
        Assert.Equal(price, dto.Price);
        Assert.Equal(storeId, dto.StoreId);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(modifiedAt, dto.ModifiedAt);
        Assert.Equal(isActive, dto.IsActive);
    }

    [Fact]
    public void ProductDto_Equality_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var storeId = ValidStoreId;
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var isActive = true;

        var dto1 = new ProductDto(id, name, description, price, storeId, createdAt, modifiedAt, isActive);
        var dto2 = new ProductDto(id, name, description, price, storeId, createdAt, modifiedAt, isActive);
        var dto3 = new ProductDto(Guid.NewGuid(), name, description, price, storeId, createdAt, modifiedAt, isActive);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
    }

    [Fact]
    public void ProductDto_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var dto = new ProductDto(
            Guid.NewGuid(),
            "Test Product",
            "Test Description",
            99.99m,
            ValidStoreId,
            DateTime.UtcNow,
            null,
            true
        );

        // Act
        var result = dto.ToString();

        // Assert
        Assert.Contains("Test Product", result);
        Assert.Contains("Test Description", result);
        Assert.Contains("99.99", result);
    }

    [Fact]
    public void CreateProductDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var storeId = ValidStoreId;

        // Act
        var dto = new CreateProductDto(name, description, price, storeId);

        // Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(description, dto.Description);
        Assert.Equal(price, dto.Price);
        Assert.Equal(storeId, dto.StoreId);
    }

    [Fact]
    public void CreateProductDto_Equality_WorksCorrectly()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var storeId = ValidStoreId;

        var dto1 = new CreateProductDto(name, description, price, storeId);
        var dto2 = new CreateProductDto(name, description, price, storeId);
        var dto3 = new CreateProductDto("Different", description, price, storeId);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
    }

    [Fact]
    public void UpdateProductDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var name = "Updated Product";
        var description = "Updated Description";
        var price = 149.99m;

        // Act
        var dto = new UpdateProductDto(name, description, price);

        // Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(description, dto.Description);
        Assert.Equal(price, dto.Price);
    }

    [Fact]
    public void UpdateProductDto_Equality_WorksCorrectly()
    {
        // Arrange
        var name = "Updated Product";
        var description = "Updated Description";
        var price = 149.99m;

        var dto1 = new UpdateProductDto(name, description, price);
        var dto2 = new UpdateProductDto(name, description, price);
        var dto3 = new UpdateProductDto("Different", description, price);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
    }
} 