using Api.Features.Projects;
using Api.Features.QuestionBank;
using Api.Features.ProjectQuestionnaires;
using System.Net.Http.Json;

namespace Api.E2ETests;

/// <summary>
/// E2E tests for Project Questionnaire management in the Designer app.
/// These tests cover project creation, question assignment, and question reordering.
/// </summary>
public class ProjectQuestionnaireE2ETests(E2ETestFixture fixture)
{
    [Fact]
    public async Task CreateProjectAndAssignQuestions_ThroughUI_ShouldPersistInDatabase()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();
        var projectName = $"E2E Test Project {Guid.NewGuid()}";

        // Ensure we have questions in the bank via API
        using var apiClient = fixture.CreateApiClient();
        var questionsResponse = await apiClient.GetAsync("/api/question-bank", TestContext.Current.CancellationToken);
        questionsResponse.EnsureSuccessStatusCode();
        var questions = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionBankItemDto>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questions);
        Assert.NotEmpty(questions);

        try
        {
            // Act - Step 1: Create a new project
            await page.GotoAsync($"{designerUrl}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            
            // Wait for the projects list to load
            await page.Locator("table").WaitForAsync(new() { Timeout = 15000 });

            // Click Create Project button
            await page.Locator("button:has-text('Create Project')").ClickAsync();

            // Fill in project details
            var nameInput = page.Locator("input[type='text']").First;
            await nameInput.WaitForAsync(new() { Timeout = 5000 });
            await nameInput.FillAsync(projectName);

            // Click Save button
            var saveButton = page.Locator("button:has-text('Save')");
            await saveButton.ClickAsync();

            // Wait for navigation to project detail page
            await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/projects/.*"), new() { Timeout = 10000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Extract project ID from URL
            var url = page.Url;
            var projectId = url.Split('/').Last();
            Assert.False(string.IsNullOrEmpty(projectId), "Expected project ID in URL");

            // Act - Step 2: Navigate to Questionnaire section
            var questionnaireButton = page.Locator("button:has-text('Questionnaire')");
            await questionnaireButton.WaitForAsync(new() { Timeout = 10000 });
            await questionnaireButton.ClickAsync();

            // Wait for questionnaire section to load
            await page.Locator("h2:has-text('Questionnaire Structure')").WaitForAsync(new() { Timeout = 10000 });

            // Act - Step 3: Import a question from library
            var importButton = page.Locator("button:has-text('Import from Library')");
            await importButton.ClickAsync();

            // Wait for side panel to open
            await page.Locator("h2:has-text('Import from Library')").WaitForAsync(new() { Timeout = 5000 });

            // Select the first question in the list
            var firstQuestion = page.Locator(".question-item").First;
            await firstQuestion.WaitForAsync(new() { Timeout = 5000 });
            await firstQuestion.ClickAsync();

            // Click Add Question button
            var addButton = page.Locator("button:has-text('Add Question')");
            await addButton.ClickAsync();

            // Wait for the side panel to close and question to appear in table
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Verify question appears in the questionnaire table
            var questionTable = page.Locator("table tbody tr").First;
            await questionTable.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await questionTable.IsVisibleAsync(), "Expected at least one question row in the table");

            // Assert - Verify in database via API
            var questionnaireResponse = await apiClient.GetAsync($"/api/projects/{projectId}/questionnaires", TestContext.Current.CancellationToken);
            questionnaireResponse.EnsureSuccessStatusCode();
            var questionnaires = await questionnaireResponse.Content.ReadFromJsonAsync<List<ProjectQuestionnaireDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(questionnaires);
            Assert.Single(questionnaires);
            Assert.Equal(0, questionnaires[0].SortOrder);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ReorderQuestions_ThroughUI_ShouldUpdateSortOrderInDatabase()
    {
        // Arrange - Create a project and add multiple questions via API
        using var apiClient = fixture.CreateApiClient();
        var projectName = $"E2E Reorder Test {Guid.NewGuid()}";

        // Create project
        var createProjectResponse = await apiClient.PostAsJsonAsync("/api/projects",
            new { name = projectName, description = "E2E Test Project" },
            TestContext.Current.CancellationToken);
        createProjectResponse.EnsureSuccessStatusCode();
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(project);

        // Get available questions
        var questionsResponse = await apiClient.GetAsync("/api/question-bank", TestContext.Current.CancellationToken);
        questionsResponse.EnsureSuccessStatusCode();
        var questions = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionBankItemDto>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questions);
        Assert.True(questions.Count >= 2, "Need at least 2 questions for reordering test");

        // Add first question
        var addResponse1 = await apiClient.PostAsJsonAsync($"/api/projects/{project.Id}/questionnaires",
            new { questionBankItemId = questions[0].Id },
            TestContext.Current.CancellationToken);
        addResponse1.EnsureSuccessStatusCode();
        var questionnaire1 = await addResponse1.Content.ReadFromJsonAsync<AddProjectQuestionnaireResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questionnaire1);

        // Add second question
        var addResponse2 = await apiClient.PostAsJsonAsync($"/api/projects/{project.Id}/questionnaires",
            new { questionBankItemId = questions[1].Id },
            TestContext.Current.CancellationToken);
        addResponse2.EnsureSuccessStatusCode();
        var questionnaire2 = await addResponse2.Content.ReadFromJsonAsync<AddProjectQuestionnaireResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questionnaire2);

        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Navigate to the project's questionnaire section
            await page.GotoAsync($"{designerUrl}/projects/{project.Id}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Click Questionnaire section
            var questionnaireButton = page.Locator("button:has-text('Questionnaire')");
            await questionnaireButton.WaitForAsync(new() { Timeout = 10000 });
            await questionnaireButton.ClickAsync();

            // Wait for questionnaire table to load
            await page.Locator("h2:has-text('Questionnaire Structure')").WaitForAsync(new() { Timeout = 10000 });
            var rows = page.Locator("table tbody tr");
            await Expect(rows).ToHaveCountAsync(2);

            // Get the initial order of questions by reading variable names
            var firstRowVariableName = await rows.Nth(0).Locator("td").Nth(1).TextContentAsync();
            var secondRowVariableName = await rows.Nth(1).Locator("td").Nth(1).TextContentAsync();
            Assert.NotNull(firstRowVariableName);
            Assert.NotNull(secondRowVariableName);

            // Act - Simulate drag and drop by using Playwright's dragTo method
            // Note: HTML5 drag-and-drop requires native events which Playwright supports
            var firstRow = rows.Nth(0);
            var secondRow = rows.Nth(1);
            
            // Drag first row to second row position
            await firstRow.DragToAsync(secondRow);

            // Wait for the drag operation to complete and the order to update
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(1000, TestContext.Current.CancellationToken); // Give time for the update request to complete

            // Assert - Verify the order changed in UI
            var newFirstRowVariableName = await rows.Nth(0).Locator("td").Nth(1).TextContentAsync();
            var newSecondRowVariableName = await rows.Nth(1).Locator("td").Nth(1).TextContentAsync();

            Assert.Equal(secondRowVariableName, newFirstRowVariableName);
            Assert.Equal(firstRowVariableName, newSecondRowVariableName);

            // Assert - Verify sort order updated in database via API
            var questionnaireResponse = await apiClient.GetAsync($"/api/projects/{project.Id}/questionnaires", TestContext.Current.CancellationToken);
            questionnaireResponse.EnsureSuccessStatusCode();
            var questionnaires = await questionnaireResponse.Content.ReadFromJsonAsync<List<ProjectQuestionnaireDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(questionnaires);
            Assert.Equal(2, questionnaires.Count);

            // Verify the sort order values are updated (order should be swapped)
            var firstQuestionnaireAfter = questionnaires.First(q => q.QuestionBankItem.VariableName == newFirstRowVariableName);
            var secondQuestionnaireAfter = questionnaires.First(q => q.QuestionBankItem.VariableName == newSecondRowVariableName);
            Assert.Equal(0, firstQuestionnaireAfter.SortOrder);
            Assert.Equal(1, secondQuestionnaireAfter.SortOrder);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DeleteQuestion_ThroughUI_ShouldRemoveFromDatabase()
    {
        // Arrange - Create a project and add a question via API
        using var apiClient = fixture.CreateApiClient();
        var projectName = $"E2E Delete Question Test {Guid.NewGuid()}";

        // Create project
        var createProjectResponse = await apiClient.PostAsJsonAsync("/api/projects",
            new { name = projectName, description = "E2E Test Project" },
            TestContext.Current.CancellationToken);
        createProjectResponse.EnsureSuccessStatusCode();
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(project);

        // Get a question
        var questionsResponse = await apiClient.GetAsync("/api/question-bank", TestContext.Current.CancellationToken);
        questionsResponse.EnsureSuccessStatusCode();
        var questions = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionBankItemDto>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questions);
        Assert.NotEmpty(questions);

        // Add question to project
        var addResponse = await apiClient.PostAsJsonAsync($"/api/projects/{project.Id}/questionnaires",
            new { questionBankItemId = questions[0].Id },
            TestContext.Current.CancellationToken);
        addResponse.EnsureSuccessStatusCode();

        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Navigate to the project's questionnaire section
            await page.GotoAsync($"{designerUrl}/projects/{project.Id}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Click Questionnaire section
            var questionnaireButton = page.Locator("button:has-text('Questionnaire')");
            await questionnaireButton.WaitForAsync(new() { Timeout = 10000 });
            await questionnaireButton.ClickAsync();

            // Wait for questionnaire table to load
            await page.Locator("h2:has-text('Questionnaire Structure')").WaitForAsync(new() { Timeout = 10000 });
            var rows = page.Locator("table tbody tr");
            await Expect(rows).ToHaveCountAsync(1);

            // Set up dialog handler to accept confirmation
            page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

            // Act - Click the delete/remove button in the row
            var deleteButton = rows.First.Locator("button[title='Remove']");
            await deleteButton.ClickAsync();

            // Wait for deletion to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Verify the question no longer appears in the table (should show empty state)
            var emptyState = page.Locator("text=No questions added yet");
            await emptyState.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await emptyState.IsVisibleAsync(), "Expected empty state message after deleting last question");

            // Assert - Verify removed from database via API
            var questionnaireResponse = await apiClient.GetAsync($"/api/projects/{project.Id}/questionnaires", TestContext.Current.CancellationToken);
            questionnaireResponse.EnsureSuccessStatusCode();
            var questionnaires = await questionnaireResponse.Content.ReadFromJsonAsync<List<ProjectQuestionnaireDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(questionnaires);
            Assert.Empty(questionnaires);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ExpandQuestionRow_ThroughUI_ShouldShowDetails()
    {
        // Arrange - Create a project and add a question via API
        using var apiClient = fixture.CreateApiClient();
        var projectName = $"E2E Expand Test {Guid.NewGuid()}";

        // Create project
        var createProjectResponse = await apiClient.PostAsJsonAsync("/api/projects",
            new { name = projectName, description = "E2E Test Project" },
            TestContext.Current.CancellationToken);
        createProjectResponse.EnsureSuccessStatusCode();
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(project);

        // Get a question
        var questionsResponse = await apiClient.GetAsync("/api/question-bank", TestContext.Current.CancellationToken);
        questionsResponse.EnsureSuccessStatusCode();
        var questions = await questionsResponse.Content.ReadFromJsonAsync<List<QuestionBankItemDto>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(questions);
        Assert.NotEmpty(questions);

        // Add question to project
        var addResponse = await apiClient.PostAsJsonAsync($"/api/projects/{project.Id}/questionnaires",
            new { questionBankItemId = questions[0].Id },
            TestContext.Current.CancellationToken);
        addResponse.EnsureSuccessStatusCode();

        var page = await fixture.CreatePageAsync();
        var designerUrl = fixture.GetDesignerAppUrl();

        try
        {
            // Navigate to the project's questionnaire section
            await page.GotoAsync($"{designerUrl}/projects/{project.Id}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Click Questionnaire section
            var questionnaireButton = page.Locator("button:has-text('Questionnaire')");
            await questionnaireButton.WaitForAsync(new() { Timeout = 10000 });
            await questionnaireButton.ClickAsync();

            // Wait for questionnaire table to load
            await page.Locator("h2:has-text('Questionnaire Structure')").WaitForAsync(new() { Timeout = 10000 });
            var rows = page.Locator("table tbody tr");
            await Expect(rows).ToHaveCountAsync(1);

            // Act - Click the expand button
            var expandButton = rows.First.Locator("button[title='Expand']");
            await expandButton.ClickAsync();

            // Wait for expanded row to appear
            await page.WaitForTimeoutAsync(500); // Brief wait for animation

            // Assert - Verify the expanded row contains detail labels
            var expandedContent = page.Locator(".expanded-content");
            await expandedContent.WaitForAsync(new() { Timeout = 5000 });
            Assert.True(await expandedContent.IsVisibleAsync(), "Expected expanded content to be visible");

            // Verify specific detail fields are visible
            var variableNameLabel = expandedContent.Locator("text=Variable Name:");
            Assert.True(await variableNameLabel.IsVisibleAsync(), "Expected 'Variable Name:' label in expanded content");

            var questionTextLabel = expandedContent.Locator("text=Question Text:");
            Assert.True(await questionTextLabel.IsVisibleAsync(), "Expected 'Question Text:' label in expanded content");

            // Act - Click collapse button
            var collapseButton = rows.First.Locator("button[title='Collapse']");
            await collapseButton.ClickAsync();

            // Assert - Verify expanded content is hidden
            await Expect(expandedContent).ToBeHiddenAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static ILocatorAssertions Expect(ILocator locator)
        => Assertions.Expect(locator);
}

// DTOs for API responses
public record QuestionBankItemDto(Guid Id, string VariableName, int Version, string? QuestionText, string? QuestionType, string? Classification);
public record ProjectDto(Guid Id, string Name, string? Description);
public record ProjectQuestionnaireDto(Guid Id, Guid ProjectId, Guid QuestionBankItemId, int SortOrder, QuestionBankItemSummaryDto QuestionBankItem);
public record QuestionBankItemSummaryDto(Guid Id, string VariableName, int Version, string? QuestionText, string? QuestionType, string? Classification);
public record AddProjectQuestionnaireResponse(Guid Id, Guid ProjectId, Guid QuestionBankItemId, int SortOrder, QuestionBankItemSummaryDto QuestionBankItem);
