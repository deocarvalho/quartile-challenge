using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Infrastructure.Data;

namespace QuartileChallenge.StoreApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Test database connection
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "healthy" });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy" });
        }
    }
}