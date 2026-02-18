using Api.Data;
using Api.Features.Clients;
using Api.Features.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace Api.IntegrationTests;

public class SeedingTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task SeedDatabase_PopulatesTables_WhenEmpty()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        
        // Ensure clean slate
        await ClearDatabaseAsync();

        // Act
        var response = await httpClient.PostAsync("/api/seed", null, cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SeedResponse>();
        Assert.NotNull(result);
        Assert.Equal("Database seeded successfully", result.Message);
        Assert.Equal(2, result.Counts.Clients);
        Assert.Equal(2, result.Counts.Projects);
        Assert.Equal(1, result.Counts.Products);
        Assert.Equal(5, result.Counts.Tags);
        Assert.Equal(2, result.Counts.Questions);

        // Verify data in DB
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var clientsCount = await db.Clients.CountAsync();
        Assert.Equal(2, clientsCount);
        
        var projectsCount = await db.Projects.CountAsync();
        Assert.Equal(2, projectsCount);

        // Act 2: Idempotency check
        var response2 = await httpClient.PostAsync("/api/seed", null, cancellationToken);
        response2.EnsureSuccessStatusCode();
        var result2 = await response2.Content.ReadFromJsonAsync<SimpleMessageResponse>();
        Assert.Equal("Database already seeded", result2?.Message);
    }

    private async Task ClearDatabaseAsync()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Use ExecuteDeleteAsync to bypass change tracker and avoid concurrency issues
        // Delete in reverse dependency order
        await db.Projects.ExecuteDeleteAsync();
        await db.ProductTemplateLines.ExecuteDeleteAsync();
        await db.ProductTemplates.ExecuteDeleteAsync();
        await db.ModuleQuestions.ExecuteDeleteAsync();
        await db.Modules.ExecuteDeleteAsync();
        await db.ProductConfigQuestions.ExecuteDeleteAsync();
        await db.Products.ExecuteDeleteAsync();
        await db.ConfigurationAnswers.ExecuteDeleteAsync();
        await db.ConfigurationQuestions.ExecuteDeleteAsync();
        await db.Clients.ExecuteDeleteAsync();
        await db.QuestionAnswers.ExecuteDeleteAsync();
        await db.QuestionBankItems.ExecuteDeleteAsync();
        await db.MetricGroups.ExecuteDeleteAsync();
        await db.FieldworkMarkets.ExecuteDeleteAsync();
        await db.CommissioningMarkets.ExecuteDeleteAsync();
        await db.Tags.ExecuteDeleteAsync();
    }

    private class SeedResponse
    {
        public string Message { get; set; } = string.Empty;
        public SeedCounts Counts { get; set; } = new();
    }

    private class SeedCounts
    {
        public int Clients { get; set; }
        public int Projects { get; set; }
        public int Products { get; set; }
        public int Tags { get; set; }
        public int Questions { get; set; }
    }

    private class SimpleMessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
