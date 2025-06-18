using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Tests.Domain;

public class ProductTests : TestBase
{
    [Fact]
    public void CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var name = TestData.Product.ValidName;
        var description = TestData.Product.ValidDescription;
        var price = TestData.Product.ValidPrice;

        // Act
        var product = new Product(name, description, price, ValidStoreId);

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(ValidStoreId, product.StoreId);
        Assert.True(product.IsActive);
        Assert.NotEqual(default, product.CreatedAt);
        Assert.Null(product.ModifiedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void CreateProduct_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product(invalidName!, TestData.Product.ValidDescription, 
                       TestData.Product.ValidPrice, ValidStoreId));
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void CreateProduct_WithInvalidPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product(TestData.Product.ValidName, TestData.Product.ValidDescription, 
                       invalidPrice, ValidStoreId));
        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void UpdateProduct_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, 
                                TestData.Product.ValidDescription,
                                TestData.Product.ValidPrice, 
                                ValidStoreId);
        
        // Act
        product.Update(TestData.Product.UpdatedName, 
                      TestData.Product.UpdatedDescription,
                      TestData.Product.UpdatedPrice);

        // Assert
        Assert.Equal(TestData.Product.UpdatedName, product.Name);
        Assert.Equal(TestData.Product.UpdatedDescription, product.Description);
        Assert.Equal(TestData.Product.UpdatedPrice, product.Price);
        Assert.NotNull(product.ModifiedAt);
    }

    [Fact]
    public void DeactivateProduct_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = new Product(TestData.Product.ValidName, 
                                TestData.Product.ValidDescription,
                                TestData.Product.ValidPrice, 
                                ValidStoreId);

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
        Assert.NotNull(product.ModifiedAt);
    }
}