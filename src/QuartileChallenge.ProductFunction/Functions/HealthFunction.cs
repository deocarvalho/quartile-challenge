using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Infrastructure.Data;

namespace QuartileChallenge.ProductFunction.Functions;

public class HealthFunction
{
    private readonly ApplicationDbContext _context;

    public HealthFunction(ApplicationDbContext context)
    {
        _context = context;
    }

    [Function("Health")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        try
        {
            await _context.Database.CanConnectAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { status = "healthy" });
            return response;
        }
        catch
        {
            var response = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            await response.WriteAsJsonAsync(new { status = "unhealthy" });
            return response;
        }
    }
}