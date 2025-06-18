using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Infrastructure.Data;
using Testcontainers.MsSql;
using Xunit;

namespace QuartileChallenge.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;
    protected readonly ApplicationDbContext Context;

    protected IntegrationTestBase()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithName($"sql_integration_test_{Guid.NewGuid()}")
            .WithPassword("YourStrong@Passw0rd")
            .Build();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        Context = new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await Context.Database.EnsureCreatedAsync();
        
        // Load SQL scripts
        await LoadSqlScripts();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }

    private async Task LoadSqlScripts()
    {
        // Load and execute function script
        var functionScript = await File.ReadAllTextAsync("database/functions/fn_GetProductsAsJson.sql");
        await Context.Database.ExecuteSqlRawAsync(functionScript);

        // Load and execute procedure script
        var procedureScript = await File.ReadAllTextAsync("database/procedures/sp_InsertProduct.sql");
        await Context.Database.ExecuteSqlRawAsync(procedureScript);
    }
}