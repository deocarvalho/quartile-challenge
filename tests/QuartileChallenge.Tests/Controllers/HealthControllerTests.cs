using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.StoreApi.Controllers;

namespace QuartileChallenge.Tests.Controllers;

public class HealthControllerTests : TestBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _controller = new HealthController(_context);
    }

    [Fact]
    public async Task Get_WithHealthyDatabase_ReturnsOkWithHealthyStatus()
    {
        // Act
        var result = await _controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        var statusProperty = response.GetType().GetProperty("status");
        Assert.NotNull(statusProperty);
        Assert.Equal("healthy", statusProperty.GetValue(response));
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
} 