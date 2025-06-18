using Microsoft.AspNetCore.Mvc;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Interfaces;

namespace QuartileChallenge.StoreApi.Controllers;

/// <summary>
/// Controller for managing store operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IStoreRepository storeRepository, ILogger<StoresController> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active stores
    /// </summary>
    /// <returns>A list of stores</returns>
    /// <response code="200">Returns the list of stores</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StoreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetAll()
    {
        var stores = await _storeRepository.GetAllAsync();
        return Ok(stores.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoreDto>> GetById(Guid id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
            return NotFound();

        return Ok(ToDto(store));
    }

    [HttpGet("company/{companyId:guid}")]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetByCompanyId(Guid companyId)
    {
        var stores = await _storeRepository.GetByCompanyIdAsync(companyId);
        return Ok(stores.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<StoreDto>> Create(CreateStoreDto createStoreDto)
    {
        var store = new Store(
            createStoreDto.CompanyId,
            createStoreDto.Name,
            createStoreDto.Location
        );

        await _storeRepository.AddAsync(store);
        
        _logger.LogInformation("Store created: {StoreId}", store.Id);
        
        return CreatedAtAction(
            nameof(GetById),
            new { id = store.Id },
            ToDto(store));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StoreDto>> Update(Guid id, UpdateStoreDto updateStoreDto)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
            return NotFound();

        store.Update(
            updateStoreDto.Name,
            updateStoreDto.Location
        );

        await _storeRepository.UpdateAsync(store);
        
        _logger.LogInformation("Store updated: {StoreId}", store.Id);
        
        return Ok(ToDto(store));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
            return NotFound();

        store.Deactivate();
        await _storeRepository.UpdateAsync(store);
        
        _logger.LogInformation("Store deactivated: {StoreId}", store.Id);
        
        return NoContent();
    }

    private static StoreDto ToDto(Store store) => new(
        store.Id,
        store.CompanyId,
        store.Name,
        store.Location,
        store.CreatedAt,
        store.ModifiedAt,
        store.IsActive
    );
}