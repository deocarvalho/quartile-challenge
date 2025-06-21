using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Tests.Domain;

public class ProductValidationTests : TestBase
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Product_Constructor_WithWhitespaceOnlyName_ThrowsArgumentException(string whitespace)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product(whitespace, "Description", 10.99m, ValidStoreId));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Product name cannot be null, empty, or whitespace", exception.Message);
    }

    [Fact]
    public void Product_Constructor_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product(null!, "Description", 10.99m, ValidStoreId));
        Assert.Equal("name", exception.ParamName);
        Assert.Contains("Product name cannot be null, empty, or whitespace", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-999.99)]
    public void Product_Constructor_WithZeroOrNegativePrice_ThrowsArgumentException(decimal invalidPrice)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product("Valid Name", "Description", invalidPrice, ValidStoreId));
        Assert.Equal("price", exception.ParamName);
        Assert.Contains("Product price must be greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(99.99)]
    [InlineData(1000)]
    [InlineData(9999.99)]
    public void Product_Constructor_WithValidPrice_CreatesProduct(decimal validPrice)
    {
        // Act
        var product = new Product("Valid Name", "Description", validPrice, ValidStoreId);

        // Assert
        Assert.Equal(validPrice, product.Price);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_Constructor_WithMinimumValidValues_CreatesProduct()
    {
        // Arrange
        var name = "A"; // Single character name
        var description = ""; // Empty description is allowed
        var price = 0.01m; // Minimum positive price
        var storeId = Guid.NewGuid();

        // Act
        var product = new Product(name, description, price, storeId);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(storeId, product.StoreId);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.True(product.CreatedAt > DateTime.MinValue);
        Assert.Null(product.ModifiedAt);
    }

    [Fact]
    public void Product_Constructor_WithLongName_CreatesProduct()
    {
        // Arrange
        var longName = new string('A', 1000); // Very long name
        var description = "Description";
        var price = 10.99m;

        // Act
        var product = new Product(longName, description, price, ValidStoreId);

        // Assert
        Assert.Equal(longName, product.Name);
        Assert.Equal(1000, product.Name.Length);
    }

    [Fact]
    public void Product_Constructor_WithLongDescription_CreatesProduct()
    {
        // Arrange
        var name = "Product Name";
        var longDescription = new string('B', 5000); // Very long description
        var price = 10.99m;

        // Act
        var product = new Product(name, longDescription, price, ValidStoreId);

        // Assert
        Assert.Equal(longDescription, product.Description);
        Assert.Equal(5000, product.Description.Length);
    }

    [Fact]
    public void Product_Constructor_WithNullDescription_CreatesProduct()
    {
        // Arrange
        var name = "Product Name";
        string? description = null;
        var price = 10.99m;

        // Act
        var product = new Product(name, description!, price, ValidStoreId);

        // Assert
        Assert.Null(product.Description);
    }

    [Fact]
    public void Product_Constructor_WithEmptyGuidStoreId_CreatesProduct()
    {
        // Arrange
        var name = "Product Name";
        var description = "Description";
        var price = 10.99m;
        var emptyStoreId = Guid.Empty;

        // Act
        var product = new Product(name, description, price, emptyStoreId);

        // Assert
        Assert.Equal(emptyStoreId, product.StoreId);
        Assert.Equal(Guid.Empty, product.StoreId);
    }

    [Fact]
    public void Product_Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var product1 = new Product("Product 1", "Description", 10.99m, ValidStoreId);
        var product2 = new Product("Product 2", "Description", 20.99m, ValidStoreId);

        // Assert
        Assert.NotEqual(product1.Id, product2.Id);
        Assert.NotEqual(Guid.Empty, product1.Id);
        Assert.NotEqual(Guid.Empty, product2.Id);
    }

    [Fact]
    public void Product_Constructor_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var product = new Product("Product", "Description", 10.99m, ValidStoreId);

        // Assert
        var afterCreation = DateTime.UtcNow.AddSeconds(1);
        Assert.True(product.CreatedAt >= beforeCreation);
        Assert.True(product.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Product_Constructor_SetsIsActiveToTrue()
    {
        // Act
        var product = new Product("Product", "Description", 10.99m, ValidStoreId);

        // Assert
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_Constructor_SetsModifiedAtToNull()
    {
        // Act
        var product = new Product("Product", "Description", 10.99m, ValidStoreId);

        // Assert
        Assert.Null(product.ModifiedAt);
    }

    [Theory]
    [InlineData("Special Characters: !@#$%^&*()")]
    [InlineData("Unicode: 测试产品名称")]
    [InlineData("Numbers: Product123")]
    [InlineData("Mixed: Product-Name_2023!")]
    public void Product_Constructor_WithSpecialCharactersInName_CreatesProduct(string specialName)
    {
        // Act
        var product = new Product(specialName, "Description", 10.99m, ValidStoreId);

        // Assert
        Assert.Equal(specialName, product.Name);
    }

    [Theory]
    [InlineData(999999999.99)]
    [InlineData(0.000001)]
    [InlineData(123456.789)]
    public void Product_Constructor_WithExtremeValidPrices_CreatesProduct(decimal extremePrice)
    {
        // Act
        var product = new Product("Product", "Description", extremePrice, ValidStoreId);

        // Assert
        Assert.Equal(extremePrice, product.Price);
    }

    [Fact]
    public void Product_Constructor_WithMaxDecimalPrice_CreatesProduct()
    {
        // Act
        var product = new Product("Product", "Description", decimal.MaxValue, ValidStoreId);

        // Assert
        Assert.Equal(decimal.MaxValue, product.Price);
    }

    [Fact]
    public void Product_Update_AllowsEmptyDescription()
    {
        // Arrange
        var product = new Product("Product", "Original Description", 10.99m, ValidStoreId);

        // Act
        product.Update("Updated Name", "", 20.99m);

        // Assert
        Assert.Equal("", product.Description);
        Assert.Equal("Updated Name", product.Name);
        Assert.Equal(20.99m, product.Price);
        Assert.NotNull(product.ModifiedAt);
    }

    [Fact]
    public void Product_Update_AllowsNullDescription()
    {
        // Arrange
        var product = new Product("Product", "Original Description", 10.99m, ValidStoreId);

        // Act
        product.Update("Updated Name", null!, 20.99m);

        // Assert
        Assert.Null(product.Description);
        Assert.Equal("Updated Name", product.Name);
        Assert.Equal(20.99m, product.Price);
        Assert.NotNull(product.ModifiedAt);
    }

    [Fact]
    public void Product_Deactivate_SetsModifiedAtToCurrentTime()
    {
        // Arrange
        var product = new Product("Product", "Description", 10.99m, ValidStoreId);
        var beforeDeactivation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        product.Deactivate();

        // Assert
        var afterDeactivation = DateTime.UtcNow.AddSeconds(1);
        Assert.NotNull(product.ModifiedAt);
        Assert.True(product.ModifiedAt >= beforeDeactivation);
        Assert.True(product.ModifiedAt <= afterDeactivation);
    }

    [Fact]
    public void Product_Update_SetsModifiedAtToCurrentTime()
    {
        // Arrange
        var product = new Product("Product", "Description", 10.99m, ValidStoreId);
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);

        // Act
        product.Update("Updated", "Updated Description", 99.99m);

        // Assert
        var afterUpdate = DateTime.UtcNow.AddSeconds(1);
        Assert.NotNull(product.ModifiedAt);
        Assert.True(product.ModifiedAt >= beforeUpdate);
        Assert.True(product.ModifiedAt <= afterUpdate);
    }
} 