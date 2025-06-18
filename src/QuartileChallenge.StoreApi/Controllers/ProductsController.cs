using Microsoft.AspNetCore.Mvc;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Interfaces;

namespace QuartileChallenge.StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(ToDto(product));
    }

    [HttpGet("store/{storeId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByStoreId(Guid storeId)
    {
        var products = await _productRepository.GetByStoreIdAsync(storeId);
        return Ok(products.Select(ToDto));
    }

    [HttpGet("company/{companyId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCompanyId(Guid companyId)
    {
        var products = await _productRepository.GetByCompanyIdAsync(companyId);
        return Ok(products.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto createProductDto)
    {
        var product = new Product(
            createProductDto.Name,
            createProductDto.Description,
            createProductDto.Price,
            createProductDto.StoreId
        );

        await _productRepository.AddAsync(product);
        
        _logger.LogInformation("Product created: {ProductId}", product.Id);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ToDto(product));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        product.Update(
            updateProductDto.Name,
            updateProductDto.Description,
            updateProductDto.Price
        );

        await _productRepository.UpdateAsync(product);
        
        _logger.LogInformation("Product updated: {ProductId}", product.Id);
        
        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        product.Deactivate();
        await _productRepository.UpdateAsync(product);
        
        _logger.LogInformation("Product deactivated: {ProductId}", product.Id);
        
        return NoContent();
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