using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Data;

[assembly: AssemblyFixture(typeof(Api.IntegrationTests.IntegrationTestFixture))]

namespace Api.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private WebApplicationFactory<Program>? _factory;

    public HttpClient HttpClient { get; private set; } = null!;
    public JsonSerializerOptions JsonOptions { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _postgresContainer.StartAsync();

        // Start Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await _redisContainer.StartAsync();

        // Create WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                // Provide connection strings via UseSetting so they are available to WebApplicationBuilder
                builder.UseSetting("ConnectionStrings:studydb", _postgresContainer.GetConnectionString());
                builder.UseSetting("ConnectionStrings:cache", _redisContainer.GetConnectionString());


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
        HttpClient = _factory.CreateClient();
        
        // Configure JSON options
        JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        JsonOptions.Converters.Add(new JsonStringEnumConverter());

        // Apply migrations
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }
}
