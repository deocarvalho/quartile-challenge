using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.DTOs;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.StoreApi.Controllers;

namespace QuartileChallenge.Tests.Controllers;

public class StoresControllerTests : TestBase
{
    private readonly Mock<IStoreRepository> _mockRepository;
    private readonly Mock<ILogger<StoresController>> _mockLogger;
    private readonly StoresController _controller;

    public StoresControllerTests()
    {
        _mockRepository = new Mock<IStoreRepository>();
        _mockLogger = new Mock<ILogger<StoresController>>();
        _controller = new StoresController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllStores()
    {
        // Arrange
        var stores = new List<Store>
        {
            new(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation),
            new(ValidCompanyId, "Store 2", "Location 2")
        };
        _mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(stores);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStores = Assert.IsAssignableFrom<IEnumerable<StoreDto>>(okResult.Value);
        Assert.Equal(2, returnedStores.Count());
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsStore()
    {
        // Arrange
        var store = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        _mockRepository.Setup(repo => repo.GetByIdAsync(store.Id))
            .ReturnsAsync(store);

        // Act
        var result = await _controller.GetById(store.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStore = Assert.IsType<StoreDto>(okResult.Value);
        Assert.Equal(store.Id, returnedStore.Id);
    }

    [Fact]
    public async Task GetById_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _controller.GetById(nonexistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedStore()
    {
        // Arrange
        var createDto = new CreateStoreDto(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        Store? savedStore = null;
        _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<Store>()))
            .Callback<Store>(store => savedStore = store)
            .ReturnsAsync((Store store) => store);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedStore = Assert.IsType<StoreDto>(createdAtResult.Value);
        Assert.NotNull(savedStore);
        Assert.Equal(createDto.Name, savedStore.Name);
        Assert.Equal(createDto.Location, savedStore.Location);
        Assert.Equal(createDto.CompanyId, savedStore.CompanyId);
    }

    [Fact]
    public async Task Update_WithExistingId_ReturnsUpdatedStore()
    {
        // Arrange
        var existingStore = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        var updateDto = new UpdateStoreDto(TestData.Store.UpdatedName, TestData.Store.UpdatedLocation);
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(existingStore.Id))
            .ReturnsAsync(existingStore);
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Store>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(existingStore.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStore = Assert.IsType<StoreDto>(okResult.Value);
        Assert.Equal(updateDto.Name, returnedStore.Name);
        Assert.Equal(updateDto.Location, returnedStore.Location);
    }

    [Fact]
    public async Task Update_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        var updateDto = new UpdateStoreDto(TestData.Store.UpdatedName, TestData.Store.UpdatedLocation);
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _controller.Update(nonexistentId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var existingStore = new Store(ValidCompanyId, TestData.Store.ValidName, TestData.Store.ValidLocation);
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(existingStore.Id))
            .ReturnsAsync(existingStore);
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Store>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(existingStore.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.False(existingStore.IsActive);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Store>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonexistentId_ReturnsNotFound()
    {
        // Arrange
        var nonexistentId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonexistentId))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _controller.Delete(nonexistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task GetByCompanyId_ReturnsCompanyStores()
    {
        // Arrange
        var stores = new List<Store>
        {
            new(ValidCompanyId, "Store 1", "Location 1"),
            new(ValidCompanyId, "Store 2", "Location 2")
        };
        _mockRepository.Setup(repo => repo.GetByCompanyIdAsync(ValidCompanyId))
            .ReturnsAsync(stores);

        // Act
        var result = await _controller.GetByCompanyId(ValidCompanyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStores = Assert.IsAssignableFrom<IEnumerable<StoreDto>>(okResult.Value);
        Assert.Equal(2, returnedStores.Count());
        Assert.All(returnedStores, store => Assert.Equal(ValidCompanyId, store.CompanyId));
    }
}