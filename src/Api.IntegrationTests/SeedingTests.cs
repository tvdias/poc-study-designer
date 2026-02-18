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
        await db.Projects.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ProductTemplateLines.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ProductTemplates.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ModuleQuestions.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Modules.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ProductConfigQuestions.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ConfigurationAnswers.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.ConfigurationQuestions.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Clients.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.QuestionAnswers.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.QuestionBankItems.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.MetricGroups.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.FieldworkMarkets.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.CommissioningMarkets.IgnoreQueryFilters().ExecuteDeleteAsync();
        await db.Tags.IgnoreQueryFilters().ExecuteDeleteAsync();
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
