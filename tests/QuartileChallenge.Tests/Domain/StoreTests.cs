using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Tests.Domain;

public class StoreTests : TestBase
{
    [Fact]
    public void CreateStore_WithValidData_ShouldCreateStore()
    {
        // Arrange
        var companyId = ValidCompanyId;
        var name = TestData.Store.ValidName;
        var location = TestData.Store.ValidLocation;

        // Act
        var store = new Store(companyId, name, location);

        // Assert
        Assert.NotEqual(Guid.Empty, store.Id);
        Assert.Equal(companyId, store.CompanyId);
        Assert.Equal(name, store.Name);
        Assert.Equal(location, store.Location);
        Assert.True(store.IsActive);
        Assert.NotEqual(default, store.CreatedAt);
        Assert.Null(store.ModifiedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void CreateStore_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var location = TestData.Store.ValidLocation;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Store(ValidCompanyId, invalidName!, location));
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void UpdateStore_WithValidData_ShouldUpdateStore()
    {
        // Arrange
        var store = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        var updatedName = TestData.Store.UpdatedName;
        var updatedLocation = TestData.Store.UpdatedLocation;

        // Act
        store.Update(updatedName, updatedLocation);

        // Assert
        Assert.Equal(updatedName, store.Name);
        Assert.Equal(updatedLocation, store.Location);
        Assert.NotNull(store.ModifiedAt);
    }

    [Fact]
    public void DeactivateStore_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var store = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);

        // Act
        store.Deactivate();

        // Assert
        Assert.False(store.IsActive);
        Assert.NotNull(store.ModifiedAt);
    }
}