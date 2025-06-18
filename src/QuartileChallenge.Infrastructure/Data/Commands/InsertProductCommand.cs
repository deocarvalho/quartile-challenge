using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace QuartileChallenge.Infrastructure.Data.Commands;

public class InsertProductCommand
{
    private readonly ApplicationDbContext _context;

    public InsertProductCommand(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Guid Id, string? Json)> ExecuteAsync(
        string name,
        string description,
        decimal price,
        Guid storeId)
    {
        var nameParam = new SqlParameter("@Name", name);
        var descParam = new SqlParameter("@Description", (object?)description ?? DBNull.Value);
        var priceParam = new SqlParameter("@Price", price);
        var storeIdParam = new SqlParameter("@StoreId", storeId);
        var idParam = new SqlParameter
        {
            ParameterName = "@Id",
            SqlDbType = SqlDbType.UniqueIdentifier,
            Direction = ParameterDirection.Output
        };

        // Use FormattableString for SqlQuery
        var result = await _context.Database
            .SqlQuery<string>($"""
                DECLARE @Json nvarchar(max);
                EXEC dbo.sp_InsertProduct 
                    @Name = {nameParam},
                    @Description = {descParam},
                    @Price = {priceParam},
                    @StoreId = {storeIdParam},
                    @Id = {idParam} OUTPUT;
                SELECT dbo.fn_GetProductsAsJson(NULL, {storeIdParam}) AS ProductJson;
                """)
            .FirstOrDefaultAsync();

        return ((Guid)idParam.Value, result);
    }
}