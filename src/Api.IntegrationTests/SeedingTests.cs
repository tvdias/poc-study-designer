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
        
        var clientsIndex = await db.Clients.CountAsync();
        Assert.Equal(2, clientsIndex);
        
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

        // Delete in reverse dependency order
        db.Projects.RemoveRange(db.Projects);
        db.ProductTemplateLines.RemoveRange(db.ProductTemplateLines);
        db.ProductTemplates.RemoveRange(db.ProductTemplates);
        db.ModuleQuestions.RemoveRange(db.ModuleQuestions);
        
        // Modules self-ref
        var modules = await db.Modules.ToListAsync();
        db.Modules.RemoveRange(modules);

        db.ProductConfigQuestions.RemoveRange(db.ProductConfigQuestions);
        db.Products.RemoveRange(db.Products);
        db.ConfigurationAnswers.RemoveRange(db.ConfigurationAnswers);
        db.ConfigurationQuestions.RemoveRange(db.ConfigurationQuestions);
        db.Clients.RemoveRange(db.Clients);
        db.QuestionAnswers.RemoveRange(db.QuestionAnswers);
        db.QuestionBankItems.RemoveRange(db.QuestionBankItems);
        db.MetricGroups.RemoveRange(db.MetricGroups);
        db.FieldworkMarkets.RemoveRange(db.FieldworkMarkets);
        db.CommissioningMarkets.RemoveRange(db.CommissioningMarkets);
        db.Tags.RemoveRange(db.Tags);

        await db.SaveChangesAsync();
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
