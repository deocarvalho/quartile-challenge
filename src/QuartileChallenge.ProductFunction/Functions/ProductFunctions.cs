using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.Interfaces;

namespace QuartileChallenge.ProductFunction.Functions;

public class ProductFunctions
{
    private readonly IProductRepository _productRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductFunctions(IProductRepository productRepository)
    {
        _productRepository = productRepository;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Function("GetProducts")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequestData req)
    {
        var products = await _productRepository.GetAllAsync();
        var response = req.CreateResponse(HttpStatusCode.OK);
        
        await response.WriteAsJsonAsync(products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StoreId,
            p.CreatedAt,
            p.ModifiedAt,
            p.IsActive
        )));

        return response;
    }

    [Function("GetProductById")]
    public async Task<HttpResponseData> GetProductById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid product ID format");
            return badRequest;
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StoreId,
            product.CreatedAt,
            product.ModifiedAt,
            product.IsActive
        ));

        return response;
    }

    [Function("GetProductsByStore")]
    public async Task<HttpResponseData> GetProductsByStore(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/store/{storeId}")] HttpRequestData req,
        string storeId)
    {
        if (!Guid.TryParse(storeId, out var parsedStoreId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid store ID format");
            return badRequest;
        }

        var products = await _productRepository.GetByStoreIdAsync(parsedStoreId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        
        await response.WriteAsJsonAsync(products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StoreId,
            p.CreatedAt,
            p.ModifiedAt,
            p.IsActive
        )));

        return response;
    }

    [Function("CreateProduct")]
    public async Task<HttpResponseData> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestData req)
    {
        var createDto = await JsonSerializer.DeserializeAsync<CreateProductDto>(
            req.Body, _jsonOptions);

        if (createDto == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid request body");
            return badRequest;
        }

        var product = new Product(
            createDto.Name,
            createDto.Description,
            createDto.Price,
            createDto.StoreId
        );

        await _productRepository.AddAsync(product);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StoreId,
            product.CreatedAt,
            product.ModifiedAt,
            product.IsActive
        ));

        return response;
    }

    [Function("UpdateProduct")]
    public async Task<HttpResponseData> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid product ID format");
            return badRequest;
        }

        var updateDto = await JsonSerializer.DeserializeAsync<UpdateProductDto>(
            req.Body, _jsonOptions);

        if (updateDto == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid request body");
            return badRequest;
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        product.Update(
            updateDto.Name,
            updateDto.Description,
            updateDto.Price
        );

        await _productRepository.UpdateAsync(product);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StoreId,
            product.CreatedAt,
            product.ModifiedAt,
            product.IsActive
        ));

        return response;
    }

    [Function("DeleteProduct")]
    public async Task<HttpResponseData> DeleteProduct(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id}")] HttpRequestData req,
        string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid product ID format");
            return badRequest;
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        product.Deactivate();
        await _productRepository.UpdateAsync(product);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static ProductDto ToDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StoreId,
        product.CreatedAt,
        product.ModifiedAt,
        product.IsActive
    );
}