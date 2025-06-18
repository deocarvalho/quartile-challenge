using Microsoft.EntityFrameworkCore;

namespace QuartileChallenge.Infrastructure.Data.Queries;

public class GetProductsJsonQuery
{
    private readonly ApplicationDbContext _context;

    public GetProductsJsonQuery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ExecuteAsync(Guid? companyId = null, Guid? storeId = null)
    {
        return await _context.Database
            .SqlQuery<string>($"""
                SELECT dbo.fn_GetProductsAsJson({(object?)companyId ?? DBNull.Value}, {(object?)storeId ?? DBNull.Value})
                """)
            .FirstOrDefaultAsync();
    }
}