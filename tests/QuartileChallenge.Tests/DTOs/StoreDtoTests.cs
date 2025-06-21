using QuartileChallenge.Core.DTOs;

namespace QuartileChallenge.Tests.DTOs;

public class StoreDtoTests : TestBase
{
    [Fact]
    public void StoreDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var name = "Test Store";
        var location = "Test Location";
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var isActive = true;

        // Act
        var dto = new StoreDto(id, companyId, name, location, createdAt, modifiedAt, isActive);

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(companyId, dto.CompanyId);
        Assert.Equal(name, dto.Name);
        Assert.Equal(location, dto.Location);
        Assert.Equal(createdAt, dto.CreatedAt);
        Assert.Equal(modifiedAt, dto.ModifiedAt);
        Assert.Equal(isActive, dto.IsActive);
    }

    [Fact]
    public void StoreDto_Equality_WorksCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var name = "Test Store";
        var location = "Test Location";
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var isActive = true;

        var dto1 = new StoreDto(id, companyId, name, location, createdAt, modifiedAt, isActive);
        var dto2 = new StoreDto(id, companyId, name, location, createdAt, modifiedAt, isActive);
        var dto3 = new StoreDto(Guid.NewGuid(), companyId, name, location, createdAt, modifiedAt, isActive);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
    }

    [Fact]
    public void StoreDto_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var dto = new StoreDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Store",
            "Test Location",
            DateTime.UtcNow,
            null,
            true
        );

        // Act
        var result = dto.ToString();

        // Assert
        Assert.Contains("Test Store", result);
        Assert.Contains("Test Location", result);
    }

    [Fact]
    public void CreateStoreDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var name = "Test Store";
        var location = "Test Location";

        // Act
        var dto = new CreateStoreDto(companyId, name, location);

        // Assert
        Assert.Equal(companyId, dto.CompanyId);
        Assert.Equal(name, dto.Name);
        Assert.Equal(location, dto.Location);
    }

    [Fact]
    public void CreateStoreDto_Equality_WorksCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var name = "Test Store";
        var location = "Test Location";

        var dto1 = new CreateStoreDto(companyId, name, location);
        var dto2 = new CreateStoreDto(companyId, name, location);
        var dto3 = new CreateStoreDto(Guid.NewGuid(), name, location);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
    }

    [Fact]
    public void UpdateStoreDto_Constructor_SetsAllProperties()
    {
        // Arrange
        var name = "Updated Store";
        var location = "Updated Location";

        // Act
        var dto = new UpdateStoreDto(name, location);

        // Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(location, dto.Location);
    }

    [Fact]
    public void UpdateStoreDto_Equality_WorksCorrectly()
    {
        // Arrange
        var name = "Updated Store";
        var location = "Updated Location";

        var dto1 = new UpdateStoreDto(name, location);
        var dto2 = new UpdateStoreDto(name, location);
        var dto3 = new UpdateStoreDto("Different", location);

        // Act & Assert
        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
    }
} 