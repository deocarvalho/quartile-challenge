using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Tests.Domain;

public class StoreValidationTests : TestBase
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Store_Constructor_WithWhitespaceOnlyName_ThrowsArgumentException(string whitespace)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Store(ValidCompanyId, whitespace, "Location"));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Store name cannot be null, empty, or whitespace", exception.Message);
    }

    [Fact]
    public void Store_Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Store(ValidCompanyId, null!, "Location"));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Store name cannot be null, empty, or whitespace", exception.Message);
    }

    [Fact]
    public void Store_Constructor_WithMinimumValidValues_CreatesStore()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var name = "A"; // Single character name
        var location = ""; // Empty location is allowed
        
        // Act
        var store = new Store(companyId, name, location);

        // Assert
        Assert.Equal(companyId, store.CompanyId);
        Assert.Equal(name, store.Name);
        Assert.Equal(location, store.Location);
        Assert.True(store.IsActive);
        Assert.NotEqual(Guid.Empty, store.Id);
        Assert.True(store.CreatedAt > DateTime.MinValue);
        Assert.Null(store.ModifiedAt);
    }

    [Fact]
    public void Store_Constructor_WithLongName_CreatesStore()
    {
        // Arrange
        var longName = new string('A', 1000); // Very long name
        var location = "Location";

        // Act
        var store = new Store(ValidCompanyId, longName, location);

        // Assert
        Assert.Equal(longName, store.Name);
        Assert.Equal(1000, store.Name.Length);
    }

    [Fact]
    public void Store_Constructor_WithLongLocation_CreatesStore()
    {
        // Arrange
        var name = "Store Name";
        var longLocation = new string('B', 5000); // Very long location

        // Act
        var store = new Store(ValidCompanyId, name, longLocation);

        // Assert
        Assert.Equal(longLocation, store.Location);
        Assert.Equal(5000, store.Location.Length);
    }

    [Fact]
    public void Store_Constructor_WithNullLocation_CreatesStore()
    {
        // Arrange
        var name = "Store Name";
        string? location = null;

        // Act
        var store = new Store(ValidCompanyId, name, location!);

        // Assert
        Assert.Null(store.Location);
    }

    [Fact]
    public void Store_Constructor_WithEmptyGuidCompanyId_CreatesStore()
    {
        // Arrange
        var name = "Store Name";
        var location = "Location";
        var emptyCompanyId = Guid.Empty;

        // Act
        var store = new Store(emptyCompanyId, name, location);

        // Assert
        Assert.Equal(emptyCompanyId, store.CompanyId);
        Assert.Equal(Guid.Empty, store.CompanyId);
    }

    [Fact]
    public void Store_Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var store1 = new Store(ValidCompanyId, "Store 1", "Location 1");
        var store2 = new Store(ValidCompanyId, "Store 2", "Location 2");

        // Assert
        Assert.NotEqual(store1.Id, store2.Id);
        Assert.NotEqual(Guid.Empty, store1.Id);
        Assert.NotEqual(Guid.Empty, store2.Id);
    }

    [Fact]
    public void Store_Constructor_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var store = new Store(ValidCompanyId, "Store", "Location");

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        Assert.True(store.CreatedAt >= beforeCreation);
        Assert.True(store.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Store_Constructor_SetsIsActiveToTrue()
    {
        // Act
        var store = new Store(ValidCompanyId, "Store", "Location");

        // Assert
        Assert.True(store.IsActive);
    }

    [Fact]
    public void Store_Constructor_SetsModifiedAtToNull()
    {
        // Act
        var store = new Store(ValidCompanyId, "Store", "Location");

        // Assert
        Assert.Null(store.ModifiedAt);
    }

    [Theory]
    [InlineData("Special Characters: !@#$%^&*()")]
    [InlineData("Unicode: 测试商店名称")]
    [InlineData("Numbers: Store123")]
    [InlineData("Mixed: Store-Name_2023!")]
    public void Store_Constructor_WithSpecialCharactersInName_CreatesStore(string specialName)
    {
        // Act
        var store = new Store(ValidCompanyId, specialName, "Location");

        // Assert
        Assert.Equal(specialName, store.Name);
    }

    [Theory]
    [InlineData("123 Main Street, City, State 12345")]
    [InlineData("Building A, Floor 5, Room 501")]
    [InlineData("Coordinates: 40.7128,-74.0060")]
    [InlineData("Unicode Location: 北京市朝阳区")]
    public void Store_Constructor_WithVariousLocationFormats_CreatesStore(string locationFormat)
    {
        // Act
        var store = new Store(ValidCompanyId, "Store", locationFormat);

        // Assert
        Assert.Equal(locationFormat, store.Location);
    }

    [Fact]
    public void Store_Update_AllowsEmptyLocation()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Original Location");

        // Act
        store.Update("Updated Name", "");

        // Assert
        Assert.Equal("", store.Location);
        Assert.Equal("Updated Name", store.Name);
        Assert.NotNull(store.ModifiedAt);
    }

    [Fact]
    public void Store_Update_AllowsNullLocation()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Original Location");

        // Act
        store.Update("Updated Name", null!);

        // Assert
        Assert.Null(store.Location);
        Assert.Equal("Updated Name", store.Name);
        Assert.NotNull(store.ModifiedAt);
    }

    [Fact]
    public void Store_Deactivate_SetsModifiedAtToCurrentTime()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Location");
        var beforeDeactivation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        store.Deactivate();

        // Assert
        var afterDeactivation = DateTime.UtcNow.AddSeconds(1);
        Assert.NotNull(store.ModifiedAt);
        Assert.True(store.ModifiedAt >= beforeDeactivation);
        Assert.True(store.ModifiedAt <= afterDeactivation);
    }

    [Fact]
    public void Store_Update_SetsModifiedAtToCurrentTime()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Location");
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        store.Update("Updated Store", "Updated Location");

        // Assert
        var afterUpdate = DateTime.UtcNow.AddSeconds(1);
        Assert.NotNull(store.ModifiedAt);
        Assert.True(store.ModifiedAt >= beforeUpdate);
        Assert.True(store.ModifiedAt <= afterUpdate);
    }

    [Fact]
    public void Store_MultipleUpdates_UpdatesModifiedAtEachTime()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Location");

        // Act & Assert - First update
        store.Update("Updated 1", "Location 1");
        var firstModifiedAt = store.ModifiedAt;
        Assert.NotNull(firstModifiedAt);

        // Wait a small amount to ensure time difference
        Thread.Sleep(10);

        // Act & Assert - Second update
        store.Update("Updated 2", "Location 2");
        var secondModifiedAt = store.ModifiedAt;
        Assert.NotNull(secondModifiedAt);
        Assert.True(secondModifiedAt > firstModifiedAt);
    }

    [Fact]
    public void Store_DeactivateAfterUpdate_PreservesModifiedAt()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Location");
        store.Update("Updated Store", "Updated Location");
        var updateModifiedAt = store.ModifiedAt;

        // Wait a small amount to ensure time difference
        Thread.Sleep(10);

        // Act
        store.Deactivate();

        // Assert
        Assert.NotNull(store.ModifiedAt);
        Assert.True(store.ModifiedAt > updateModifiedAt);
        Assert.False(store.IsActive);
    }

    [Fact]  
    public void Store_UpdateAfterDeactivate_UpdatesModifiedAt()
    {
        // Arrange
        var store = new Store(ValidCompanyId, "Store", "Location");
        store.Deactivate();
        var deactivateModifiedAt = store.ModifiedAt;

        // Wait a small amount to ensure time difference
        Thread.Sleep(10);

        // Act
        store.Update("Reactivated Store", "New Location");

        // Assert
        Assert.NotNull(store.ModifiedAt);
        Assert.True(store.ModifiedAt > deactivateModifiedAt);
        Assert.False(store.IsActive); // Note: Update doesn't reactivate
        Assert.Equal("Reactivated Store", store.Name);
        Assert.Equal("New Location", store.Location);
    }
} 