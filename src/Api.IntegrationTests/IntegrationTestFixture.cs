using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Data;

[assembly: AssemblyFixture(typeof(Api.IntegrationTests.IntegrationTestFixture))]

namespace Api.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public HttpClient HttpClient { get; private set; } = null!;
    public JsonSerializerOptions JsonOptions { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        await _postgresContainer.StartAsync();

        // Create WebApplicationFactory
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove default providers
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                    logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                });
                
                // Provide connection strings via UseSetting so they are available to WebApplicationBuilder
                builder.UseSetting("ConnectionStrings:studydb", _postgresContainer.GetConnectionString());

                // Use ConfigureServices (not ConfigureTestServices) to intercept BEFORE Aspire validates
                builder.ConfigureServices((context, services) =>
                {
                    // Find and remove the Aspire DbContext registration
                    var dbContextDescriptors = services
                        .Where(d => d.ServiceType == typeof(ApplicationDbContext) ||
                                   d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                                   (d.ServiceType.IsGenericType && 
                                    d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                        .ToList();

                    foreach (var descriptor in dbContextDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    // Re-add DbContext with direct Npgsql configuration (bypassing Aspire)
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString());
                    });
                });
            });

        // Create HttpClient
        HttpClient = Factory.CreateClient();
        
        // Configure JSON options
        JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        JsonOptions.Converters.Add(new JsonStringEnumConverter());

        // Apply migrations
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }


    }
}
