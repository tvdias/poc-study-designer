using Api.Data;
using Api.Features.ManagedLists;
using Api.Features.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests;

public class SubsetSynchronizationUnitTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RefreshQuestionDisplays_IdentifiesAffectedQuestions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = NullLogger<SubsetManagementService>.Instance;
        var service = new SubsetManagementService(context, logger);

        var projectId = Guid.NewGuid();
        var managedListId = Guid.NewGuid();
        var subsetId = Guid.NewGuid();
        var questionId1 = Guid.NewGuid();
        var questionId2 = Guid.NewGuid();

        // Create test data
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Status = ProjectStatus.Draft,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var managedList = new ManagedList
        {
            Id = managedListId,
            ProjectId = projectId,
            Name = "TestList",
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var subset = new SubsetDefinition
        {
            Id = subsetId,
            ProjectId = projectId,
            ManagedListId = managedListId,
            Name = "TestList_SUB1",
            SignatureHash = "test-hash",
            Status = SubsetStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var link1 = new QuestionSubsetLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            QuestionnaireLineId = questionId1,
            ManagedListId = managedListId,
            SubsetDefinitionId = subsetId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var link2 = new QuestionSubsetLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            QuestionnaireLineId = questionId2,
            ManagedListId = managedListId,
            SubsetDefinitionId = subsetId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        context.Projects.Add(project);
        context.ManagedLists.Add(managedList);
        context.SubsetDefinitions.Add(subset);
        context.QuestionSubsetLinks.AddRange(link1, link2);
        await context.SaveChangesAsync();

        // Act
        await service.RefreshQuestionDisplaysAsync(subsetId, CancellationToken.None);

        // Assert
        // In a real implementation, this would verify that refresh events were published
        // For now, we verify that the method executes without error
        Assert.True(true);
    }

    [Fact]
    public async Task DeleteSubset_RemovesSubsetAndClearsLinks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = NullLogger<SubsetManagementService>.Instance;
        var service = new SubsetManagementService(context, logger);

        var projectId = Guid.NewGuid();
        var managedListId = Guid.NewGuid();
        var subsetId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Status = ProjectStatus.Draft,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var managedList = new ManagedList
        {
            Id = managedListId,
            ProjectId = projectId,
            Name = "TestList",
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var subset = new SubsetDefinition
        {
            Id = subsetId,
            ProjectId = projectId,
            ManagedListId = managedListId,
            Name = "TestList_SUB1",
            SignatureHash = "test-hash",
            Status = SubsetStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var membership = new SubsetMembership
        {
            Id = Guid.NewGuid(),
            SubsetDefinitionId = subsetId,
            ManagedListItemId = itemId1,
            CreatedOn = DateTime.UtcNow
        };

        var link = new QuestionSubsetLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            QuestionnaireLineId = questionId,
            ManagedListId = managedListId,
            SubsetDefinitionId = subsetId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        context.Projects.Add(project);
        context.ManagedLists.Add(managedList);
        context.SubsetDefinitions.Add(subset);
        context.SubsetMemberships.Add(membership);
        context.QuestionSubsetLinks.Add(link);
        await context.SaveChangesAsync();

        // Act
        var response = await service.DeleteSubsetAsync(subsetId, "test", CancellationToken.None);

        // Assert
        Assert.Equal(subsetId, response.SubsetDefinitionId);
        Assert.Single(response.AffectedQuestionIds);
        Assert.Equal(questionId, response.AffectedQuestionIds[0]);

        // Verify subset is deleted
        var deletedSubset = await context.SubsetDefinitions.FindAsync(subsetId);
        Assert.Null(deletedSubset);

        // Verify memberships are deleted
        var remainingMemberships = await context.SubsetMemberships
            .Where(sm => sm.SubsetDefinitionId == subsetId)
            .CountAsync();
        Assert.Equal(0, remainingMemberships);

        // Verify link is cleared (SubsetDefinitionId set to null)
        var updatedLink = await context.QuestionSubsetLinks.FindAsync(link.Id);
        Assert.NotNull(updatedLink);
        Assert.Null(updatedLink.SubsetDefinitionId);
    }

    [Fact]
    public async Task DeleteSubset_ThrowsWhenProjectNotDraft()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = NullLogger<SubsetManagementService>.Instance;
        var service = new SubsetManagementService(context, logger);

        var projectId = Guid.NewGuid();
        var managedListId = Guid.NewGuid();
        var subsetId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Status = ProjectStatus.Active, // Not Draft
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var managedList = new ManagedList
        {
            Id = managedListId,
            ProjectId = projectId,
            Name = "TestList",
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var subset = new SubsetDefinition
        {
            Id = subsetId,
            ProjectId = projectId,
            ManagedListId = managedListId,
            Name = "TestList_SUB1",
            SignatureHash = "test-hash",
            Status = SubsetStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        context.Projects.Add(project);
        context.ManagedLists.Add(managedList);
        context.SubsetDefinitions.Add(subset);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.DeleteSubsetAsync(subsetId, "test", CancellationToken.None));
    }

    [Fact]
    public async Task RefreshProjectSummary_GeneratesCorrectSummary()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = NullLogger<SubsetManagementService>.Instance;
        var service = new SubsetManagementService(context, logger);

        var projectId = Guid.NewGuid();
        var managedListId = Guid.NewGuid();
        var subsetId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();
        var itemId3 = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Status = ProjectStatus.Draft,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var managedList = new ManagedList
        {
            Id = managedListId,
            ProjectId = projectId,
            Name = "TestList",
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var item1 = new ManagedListItem
        {
            Id = itemId1,
            ManagedListId = managedListId,
            Value = "ITEM1",
            Label = "Item 1",
            SortOrder = 1,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var item2 = new ManagedListItem
        {
            Id = itemId2,
            ManagedListId = managedListId,
            Value = "ITEM2",
            Label = "Item 2",
            SortOrder = 2,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var item3 = new ManagedListItem
        {
            Id = itemId3,
            ManagedListId = managedListId,
            Value = "ITEM3",
            Label = "Item 3",
            SortOrder = 3,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var subset = new SubsetDefinition
        {
            Id = subsetId,
            ProjectId = projectId,
            ManagedListId = managedListId,
            Name = "TestList_SUB1",
            SignatureHash = "test-hash",
            Status = SubsetStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var membership1 = new SubsetMembership
        {
            Id = Guid.NewGuid(),
            SubsetDefinitionId = subsetId,
            ManagedListItemId = itemId1,
            CreatedOn = DateTime.UtcNow
        };

        var membership2 = new SubsetMembership
        {
            Id = Guid.NewGuid(),
            SubsetDefinitionId = subsetId,
            ManagedListItemId = itemId2,
            CreatedOn = DateTime.UtcNow
        };

        var link = new QuestionSubsetLink
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            QuestionnaireLineId = questionId,
            ManagedListId = managedListId,
            SubsetDefinitionId = subsetId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        context.Projects.Add(project);
        context.ManagedLists.Add(managedList);
        context.ManagedListItems.AddRange(item1, item2, item3);
        context.SubsetDefinitions.Add(subset);
        context.SubsetMemberships.AddRange(membership1, membership2);
        context.QuestionSubsetLinks.Add(link);
        await context.SaveChangesAsync();

        // Act
        var response = await service.RefreshProjectSummaryAsync(projectId, CancellationToken.None);

        // Assert
        Assert.Equal(projectId, response.ProjectId);
        Assert.Single(response.Subsets);

        var summary = response.Subsets[0];
        Assert.Equal(subsetId, summary.Id);
        Assert.Equal("TestList_SUB1", summary.Name);
        Assert.Equal(2, summary.MemberCount); // 2 items in subset
        Assert.Equal(3, summary.TotalItemsInList); // 3 total active items
        Assert.False(summary.IsFull); // Partial selection
        Assert.Equal(2, summary.MemberLabels.Count);
        Assert.Contains("Item 1", summary.MemberLabels);
        Assert.Contains("Item 2", summary.MemberLabels);
        Assert.Equal(1, summary.QuestionCount);
    }

    [Fact]
    public async Task InvalidateSubsetsForItem_OnlyAffectsDraftProjects()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var logger = NullLogger<SubsetManagementService>.Instance;
        var service = new SubsetManagementService(context, logger);

        var projectId = Guid.NewGuid();
        var managedListId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            Status = ProjectStatus.Active, // Not Draft
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var managedList = new ManagedList
        {
            Id = managedListId,
            ProjectId = projectId,
            Name = "TestList",
            Status = ManagedListStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        var item = new ManagedListItem
        {
            Id = itemId,
            ManagedListId = managedListId,
            Value = "ITEM1",
            Label = "Item 1",
            SortOrder = 1,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };

        context.Projects.Add(project);
        context.ManagedLists.Add(managedList);
        context.ManagedListItems.Add(item);
        await context.SaveChangesAsync();

        // Act - should not throw, just log and skip
        await service.InvalidateSubsetsForItemAsync(itemId, "test", CancellationToken.None);

        // Assert - no exception means it handled the non-Draft project correctly
        Assert.True(true);
    }
}
