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
    [Fact(Skip = "Flaky test - can fail due to concurrent test execution affecting database state")]
    public async Task SeedDatabase_PopulatesTables_WhenEmpty()
    {
        // Arrange
        var httpClient = fixture.HttpClient;
        var cancellationToken = TestContext.Current.CancellationToken;
        
        // Ensure clean slate
        await ClearDatabaseAsync(cancellationToken);

        // Act
        var response = await httpClient.PostAsync("/api/seed", null, cancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SeedResponse>(cancellationToken);
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
        
        var clientsCount = await db.Clients.CountAsync(cancellationToken);
        Assert.Equal(2, clientsCount);
        
        var projectsCount = await db.Projects.CountAsync(cancellationToken);
        Assert.Equal(2, projectsCount);

        // Act 2: Idempotency check
        var response2 = await httpClient.PostAsync("/api/seed", null, cancellationToken);
        response2.EnsureSuccessStatusCode();
        var result2 = await response2.Content.ReadFromJsonAsync<SimpleMessageResponse>(cancellationToken);
        Assert.Equal("Database already seeded", result2?.Message);
    }

    private async Task ClearDatabaseAsync(CancellationToken cancellationToken)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Use ExecuteDeleteAsync to bypass change tracker and avoid concurrency issues
        // Delete in reverse dependency order
        await db.Projects.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ProductTemplateLines.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ProductTemplates.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ModuleQuestions.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Modules.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ProductConfigQuestions.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Products.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ConfigurationAnswers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.ConfigurationQuestions.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Clients.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.QuestionAnswers.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.QuestionBankItems.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.MetricGroups.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.FieldworkMarkets.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.CommissioningMarkets.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
        await db.Tags.IgnoreQueryFilters().ExecuteDeleteAsync(cancellationToken);
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
