using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Infrastructure.Data;
using System.Collections.Concurrent;
using System.Data;
using Xunit;

namespace QuartileChallenge.IntegrationTests.Infrastructure.SqlComponents;

public class InsertProductProcedureIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task InsertProduct_WithConcurrentInserts_MaintainsDataIntegrity()
    {
        // Arrange
        var store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        await Context.Stores.AddAsync(store);
        await Context.SaveChangesAsync();

        var tasks = new List<Task>();
        var insertedIds = new ConcurrentBag<Guid>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var task = Task.Run(async () =>
            {
                // Create a new context for each thread
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(Context.Database.GetConnectionString())
                    .Options;

                await using var threadContext = new ApplicationDbContext(options);

                var idParam = new SqlParameter
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.UniqueIdentifier,
                    Direction = ParameterDirection.Output
                };

                await threadContext.Database.ExecuteSqlInterpolatedAsync($@"
                    EXEC dbo.sp_InsertProduct 
                        @Name = {"Product " + i}, 
                        @Description = {"Description " + i}, 
                        @Price = {99.99m + i}, 
                        @StoreId = {store.Id}, 
                        @Id = {idParam} OUTPUT");

                insertedIds.Add((Guid)idParam.Value);
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        // Assert
        var allProducts = await Context.Products.ToListAsync();
        Assert.Equal(10, allProducts.Count);
        Assert.Equal(10, insertedIds.Count);
        Assert.Equal(insertedIds.Count, insertedIds.Distinct().Count()); // All IDs should be unique
    }

    [Fact]
    public async Task InsertProduct_WithTransactions_HandlesRollbackCorrectly()
    {
        // Arrange
        var store = new Store(Guid.NewGuid(), "Test Store", "Test Location");
        await Context.Stores.AddAsync(store);
        await Context.SaveChangesAsync();

        var initialCount = await Context.Products.CountAsync();

        // Act & Assert
        await using var transaction = await Context.Database.BeginTransactionAsync();
        
        try
        {
            var idParam = new SqlParameter
            {
                ParameterName = "@Id",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Direction = ParameterDirection.Output
            };

            // First insert (should succeed)
            await Context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.sp_InsertProduct 
                    @Name = {"Valid Product"}, 
                    @Description = {"Description"}, 
                    @Price = {99.99m}, 
                    @StoreId = {store.Id}, 
                    @Id = {idParam} OUTPUT");

            // Second insert (should fail)
            await Assert.ThrowsAsync<SqlException>(() => 
                Context.Database.ExecuteSqlInterpolatedAsync($@"
                    EXEC dbo.sp_InsertProduct 
                        @Name = {null}, 
                        @Description = {"Description"}, 
                        @Price = {99.99m}, 
                        @StoreId = {store.Id}, 
                        @Id = {idParam} OUTPUT"));

            await transaction.RollbackAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Verify no products were added
        var finalCount = await Context.Products.CountAsync();
        Assert.Equal(initialCount, finalCount);
    }
}