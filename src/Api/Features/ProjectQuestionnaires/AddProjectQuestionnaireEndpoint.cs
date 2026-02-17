using Api.Data;
using Api.Features.ProjectQuestionnaires.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ProjectQuestionnaires;

public static class AddProjectQuestionnaireEndpoint
{
    public static void MapAddProjectQuestionnaireEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/projects/{projectId:guid}/questionnaires", async Task<Results<Created<AddProjectQuestionnaireResponse>, ValidationProblem, NotFound, Conflict<string>>> (
            Guid projectId,
            AddProjectQuestionnaireRequest request,
            ApplicationDbContext context,
            IValidator<AddProjectQuestionnaireRequest> validator,
            CancellationToken cancellationToken) =>
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            // Check if project exists
            var projectExists = await context.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
            if (!projectExists)
            {
                return TypedResults.NotFound();
            }

            // Check if question bank item exists
            var questionBankItem = await context.QuestionBankItems
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == request.QuestionBankItemId, cancellationToken);

            if (questionBankItem == null)
            {
                return TypedResults.NotFound();
            }

            // Check if this question is already added to the project
            var exists = await context.Set<ProjectQuestionnaire>()
                .AnyAsync(pq => pq.ProjectId == projectId && pq.QuestionBankItemId == request.QuestionBankItemId, cancellationToken);

            if (exists)
            {
                return TypedResults.Conflict("This question has already been added to the project questionnaire.");
            }

            // Get the next sort order
            var maxSortOrder = await context.Set<ProjectQuestionnaire>()
                .Where(pq => pq.ProjectId == projectId)
                .MaxAsync(pq => (int?)pq.SortOrder, cancellationToken) ?? -1;

            // Copy fields from QuestionBankItem to ProjectQuestionnaire
            var projectQuestionnaire = new ProjectQuestionnaire
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                QuestionBankItemId = request.QuestionBankItemId,
                SortOrder = maxSortOrder + 1,
                
                // Copy editable fields from question bank
                VariableName = questionBankItem.VariableName,
                Version = questionBankItem.Version,
                QuestionText = questionBankItem.QuestionText,
                QuestionTitle = questionBankItem.QuestionTitle,
                QuestionType = questionBankItem.QuestionType,
                Classification = questionBankItem.Classification,
                QuestionRationale = questionBankItem.QuestionRationale,
                ScraperNotes = questionBankItem.ScraperNotes,
                CustomNotes = questionBankItem.CustomNotes,
                RowSortOrder = questionBankItem.RowSortOrder,
                ColumnSortOrder = questionBankItem.ColumnSortOrder,
                AnswerMin = questionBankItem.AnswerMin,
                AnswerMax = questionBankItem.AnswerMax,
                QuestionFormatDetails = questionBankItem.QuestionFormatDetails,
                IsDummy = questionBankItem.IsDummy,
                
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system" // TODO: Replace with actual user
            };

            context.Set<ProjectQuestionnaire>().Add(projectQuestionnaire);
            await context.SaveChangesAsync(cancellationToken);

            var response = new AddProjectQuestionnaireResponse(
                projectQuestionnaire.Id,
                projectQuestionnaire.ProjectId,
                projectQuestionnaire.QuestionBankItemId,
                projectQuestionnaire.SortOrder,
                projectQuestionnaire.VariableName,
                projectQuestionnaire.Version,
                projectQuestionnaire.QuestionText,
                projectQuestionnaire.QuestionTitle,
                projectQuestionnaire.QuestionType,
                projectQuestionnaire.Classification,
                projectQuestionnaire.QuestionRationale
            );

            return TypedResults.Created($"/api/projects/{projectId}/questionnaires/{projectQuestionnaire.Id}", response);
        })
        .WithName("AddProjectQuestionnaire")
        .WithTags("ProjectQuestionnaires");
    }
}
