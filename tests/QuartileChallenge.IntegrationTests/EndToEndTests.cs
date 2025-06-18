using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.StoreApi;
using System.Text.Json;
using Xunit;

namespace QuartileChallenge.IntegrationTests;

public class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public EndToEndTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task FullStoreAndProductWorkflow_Success()
    {
        // 1. Create Store
        var createStoreDto = new CreateStoreDto(
            Guid.NewGuid(),
            "Test Store",
            "Test Location"
        );

        var storeResponse = await _client.PostAsJsonAsync("/api/stores", createStoreDto);
        Assert.Equal(HttpStatusCode.Created, storeResponse.StatusCode);
        
        var storeContent = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();
        Assert.NotNull(storeContent);
        var storeId = storeContent.Id;

        // 2. Create Product in Store
        var createProductDto = new CreateProductDto(
            "Test Product",
            "Test Description",
            99.99m,
            storeId
        );

        var productResponse = await _client.PostAsJsonAsync("/api/products", createProductDto);
        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);
        
        var productContent = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(productContent);
        var productId = productContent.Id;

        // 3. Verify Store Products
        var storeProductsResponse = await _client.GetAsync($"/api/products/store/{storeId}");
        Assert.Equal(HttpStatusCode.OK, storeProductsResponse.StatusCode);
        
        var products = await storeProductsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.NotNull(products);
        Assert.Contains(products, p => p.Id == productId);

        // 4. Update Product
        var updateProductDto = new UpdateProductDto(
            "Updated Product",
            "Updated Description",
            149.99m
        );

        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{productId}", updateProductDto);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 5. Verify Update
        var updatedProductResponse = await _client.GetAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.OK, updatedProductResponse.StatusCode);
        
        var updatedProduct = await updatedProductResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updatedProduct);
        Assert.Equal(updateProductDto.Name, updatedProduct.Name);
        Assert.Equal(updateProductDto.Description, updatedProduct.Description);
        Assert.Equal(updateProductDto.Price, updatedProduct.Price);

        // 6. Delete Product
        var deleteResponse = await _client.DeleteAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 7. Verify Product is Inactive
        var deletedProductResponse = await _client.GetAsync($"/api/products/{productId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedProductResponse.StatusCode);
    }

    [Fact]
    public async Task MultiCompanyScenario_Success()
    {
        // 1. Create Two Companies' Stores
        var company1Id = Guid.NewGuid();
        var company2Id = Guid.NewGuid();

        var store1Dto = new CreateStoreDto(company1Id, "Company 1 Store", "Location 1");
        var store2Dto = new CreateStoreDto(company2Id, "Company 2 Store", "Location 2");

        var store1Response = await _client.PostAsJsonAsync("/api/stores", store1Dto);
        var store2Response = await _client.PostAsJsonAsync("/api/stores", store2Dto);

        Assert.Equal(HttpStatusCode.Created, store1Response.StatusCode);
        Assert.Equal(HttpStatusCode.Created, store2Response.StatusCode);

        var store1 = await store1Response.Content.ReadFromJsonAsync<StoreDto>();
        var store2 = await store2Response.Content.ReadFromJsonAsync<StoreDto>();

        // 2. Add Products to Both Stores
        var product1Dto = new CreateProductDto("Product 1", "Description 1", 99.99m, store1!.Id);
        var product2Dto = new CreateProductDto("Product 2", "Description 2", 149.99m, store2!.Id);

        await _client.PostAsJsonAsync("/api/products", product1Dto);
        await _client.PostAsJsonAsync("/api/products", product2Dto);

        // 3. Verify Company Filtering
        var company1Products = await _client.GetAsync($"/api/stores/company/{company1Id}");
        var company2Products = await _client.GetAsync($"/api/stores/company/{company2Id}");

        Assert.Equal(HttpStatusCode.OK, company1Products.StatusCode);
        Assert.Equal(HttpStatusCode.OK, company2Products.StatusCode);

        var company1Results = await company1Products.Content.ReadFromJsonAsync<List<StoreDto>>();
        var company2Results = await company2Products.Content.ReadFromJsonAsync<List<StoreDto>>();

        Assert.NotNull(company1Results);
        Assert.NotNull(company2Results);
        Assert.Single(company1Results);
        Assert.Single(company2Results);
        Assert.Equal(company1Id, company1Results[0].CompanyId);
        Assert.Equal(company2Id, company2Results[0].CompanyId);
    }

    [Fact]
    public async Task PerformanceTest_Success()
    {
        // 1. Create Store
        var storeDto = new CreateStoreDto(Guid.NewGuid(), "Performance Store", "Location");
        var storeResponse = await _client.PostAsJsonAsync("/api/stores", storeDto);
        var store = await storeResponse.Content.ReadFromJsonAsync<StoreDto>();
        Assert.NotNull(store);

        // 2. Create Multiple Products
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var productDto = new CreateProductDto(
                $"Product {i}",
                $"Description {i}",
                99.99m + i,
                store.Id
            );
            tasks.Add(_client.PostAsJsonAsync("/api/products", productDto));
        }

        // 3. Wait for all products to be created
        await Task.WhenAll(tasks);

        // 4. Measure Query Performance
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var productsResponse = await _client.GetAsync($"/api/products/store/{store.Id}");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, productsResponse.StatusCode);
        Assert.True(sw.ElapsedMilliseconds < 1000, "Query took too long"); // Should complete within 1 second
    }
}