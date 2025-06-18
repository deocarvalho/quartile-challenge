using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using System.Text.Json;
using Xunit;

namespace QuartileChallenge.IntegrationTests.Infrastructure.SqlComponents;

public class ProductJsonFunctionIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetProductsAsJson_WithMultipleCompanies_ReturnsCorrectResults()
    {
        // Arrange
        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();

        var store1 = new Store(company1, "Store 1", "Location 1");
        var store2 = new Store(company1, "Store 2", "Location 2");
        var store3 = new Store(company2, "Store 3", "Location 3");

        await Context.Stores.AddRangeAsync(store1, store2, store3);
        await Context.SaveChangesAsync();

        var products = new[]
        {
            new Product("Product 1", "Desc 1", 10.99m, store1.Id),
            new Product("Product 2", "Desc 2", 20.99m, store1.Id),
            new Product("Product 3", "Desc 3", 30.99m, store2.Id),
            new Product("Product 4", "Desc 4", 40.99m, store3.Id)
        };

        await Context.Products.AddRangeAsync(products);
        await Context.SaveChangesAsync();

        // Act
        var result = await Context.Database
            .SqlQuery<string>($"SELECT dbo.fn_GetProductsAsJson('{company1}', NULL) as json")
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        var jsonDoc = JsonDocument.Parse(result);
        var resultProducts = jsonDoc.RootElement.EnumerateArray().ToList();
        
        Assert.Equal(3, resultProducts.Count); // Only products from company1
        Assert.All(resultProducts, p => Assert.Equal(company1.ToString(), 
            p.GetProperty("companyId").GetString()));
    }
}