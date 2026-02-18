using Api.Data;
using Api.Features.QuestionnaireLines.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionnaireLines;

public static class AddQuestionnaireLineEndpoint
{
    public static void MapAddQuestionnaireLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/projects/{projectId:guid}/questionnairelines", async Task<Results<Created<AddQuestionnaireLineResponse>, ValidationProblem, NotFound, Conflict<string>>> (
            Guid projectId,
            AddQuestionnaireLineRequest request,
            ApplicationDbContext context,
            IValidator<AddQuestionnaireLineRequest> validator,
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
            var exists = await context.Set<QuestionnaireLine>()
                .AnyAsync(pq => pq.ProjectId == projectId && pq.QuestionBankItemId == request.QuestionBankItemId, cancellationToken);

            if (exists)
            {
                return TypedResults.Conflict("This question has already been added to the project questionnaire.");
            }

            // Get the next sort order
            var maxSortOrder = await context.Set<QuestionnaireLine>()
                .Where(pq => pq.ProjectId == projectId)
                .MaxAsync(pq => (int?)pq.SortOrder, cancellationToken) ?? -1;

            // Copy fields from QuestionBankItem to QuestionnaireLine
            var questionnaireLine = new QuestionnaireLine
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

            context.Set<QuestionnaireLine>().Add(questionnaireLine);
            await context.SaveChangesAsync(cancellationToken);

            var response = new AddQuestionnaireLineResponse(
                questionnaireLine.Id,
                questionnaireLine.ProjectId,
                questionnaireLine.QuestionBankItemId,
                questionnaireLine.SortOrder,
                questionnaireLine.VariableName,
                questionnaireLine.Version,
                questionnaireLine.QuestionText,
                questionnaireLine.QuestionTitle,
                questionnaireLine.QuestionType,
                questionnaireLine.Classification,
                questionnaireLine.QuestionRationale
            );

            return TypedResults.Created($"/api/projects/{projectId}/questionnairelines/{questionnaireLine.Id}", response);
        })
        .WithName("AddQuestionnaireLine")
        .WithTags("QuestionnaireLines");
    }
}
