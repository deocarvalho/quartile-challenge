using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuartileChallenge.Tests.Infrastructure.Procedures;

public class InsertProductProcedureTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Store _store;

    public InsertProductProcedureTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuartileChallengeDb_Test;Trusted_Connection=True;")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Setup test data
        _store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        _context.Stores.Add(_store);
        _context.SaveChanges();

        // Create procedure
        _context.Database.ExecuteSqlRaw(File.ReadAllText("../../../database/procedures/sp_InsertProduct.sql"));
    }

    [Fact]
    public async Task InsertProduct_WithValidData_InsertsProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var idParam = new SqlParameter
        {
            ParameterName = "@Id",
            SqlDbType = SqlDbType.UniqueIdentifier,
            Direction = ParameterDirection.Output
        };

        // Act
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC dbo.sp_InsertProduct 
                @Name = {name}, 
                @Description = {description}, 
                @Price = {price}, 
                @StoreId = {_store.Id}, 
                @Id = {idParam} OUTPUT");

        // Assert
        var insertedId = (Guid)idParam.Value;
        var product = await _context.Products.FindAsync(insertedId);
        Assert.NotNull(product);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(_store.Id, product.StoreId);
    }

    [Fact]
    public async Task InsertProduct_WithInvalidStore_ThrowsError()
    {
        // Arrange
        var invalidStoreId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => 
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @Id uniqueidentifier
                EXEC dbo.sp_InsertProduct 
                    @Name = 'Test', 
                    @Description = 'Test', 
                    @Price = 99.99, 
                    @StoreId = {invalidStoreId}, 
                    @Id = @Id OUTPUT"));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}