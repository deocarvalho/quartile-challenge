using Microsoft.OpenApi.Models;
using QuartileChallenge.Infrastructure;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QuartileChallenge.IntegrationTests")]

namespace QuartileChallenge.StoreApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddInfrastructure(builder.Configuration);

        // Configure OpenAPI/Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Quartile Store API", 
                Version = "v1",
                Description = @"API for managing stores in a multi-company environment.

## Setup Guide
1. Clone the repository
2. Update appsettings.json with your database connection
3. Run migrations using `dotnet ef database update`
4. Start the application using `dotnet run`

## Authentication
Use Bearer authentication with JWT tokens.

## Environment Configuration
- Development: https://localhost:5001
- Staging: https://staging-api.quartile.com
- Production: https://api.quartile.com",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@quartile.com"
                },
                License = new OpenApiLicense
                {
                    Name = "Internal Use Only",
                    Url = new Uri("https://quartile.com/license")
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Add security definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Add response examples
            c.UseInlineDefinitionsForEnums();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Store API v1");
                c.RoutePrefix = "api-docs";
                c.DocumentTitle = "Quartile Store API Documentation";
                c.EnableDeepLinking();
                c.DisplayRequestDuration();
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}