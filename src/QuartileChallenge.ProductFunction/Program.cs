using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.Infrastructure;
using QuartileChallenge.Infrastructure.Data;
using QuartileChallenge.Infrastructure.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Add database context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(context.Configuration["ConnectionStrings:DefaultConnection"]));

        // Add repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
    })
    .Build();

await host.RunAsync();