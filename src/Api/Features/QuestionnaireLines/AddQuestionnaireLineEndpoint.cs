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

            string variableName;
            int version;
            string? questionText = null;
            string? questionTitle = null;
            string? questionType = null;
            string? classification = null;
            string? questionRationale = null;
            string? scraperNotes = null;
            string? customNotes = null;
            int? rowSortOrder = null;
            int? columnSortOrder = null;
            int? answerMin = null;
            int? answerMax = null;
            string? questionFormatDetails = null;
            bool isDummy = false;

            // If QuestionBankItemId is provided, import from question bank
            if (request.QuestionBankItemId.HasValue)
            {
                var questionBankItem = await context.QuestionBankItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.Id == request.QuestionBankItemId.Value, cancellationToken);

                if (questionBankItem == null)
                {
                    return TypedResults.NotFound();
                }

                // Check if this question is already added to the project
                var exists = await context.Set<QuestionnaireLine>()
                    .AnyAsync(pq => pq.ProjectId == projectId && pq.QuestionBankItemId == request.QuestionBankItemId.Value, cancellationToken);

                if (exists)
                {
                    return TypedResults.Conflict("This question has already been added to the project questionnaire.");
                }

                // Copy fields from QuestionBankItem
                variableName = questionBankItem.VariableName;
                version = questionBankItem.Version;
                questionText = questionBankItem.QuestionText;
                questionTitle = questionBankItem.QuestionTitle;
                questionType = questionBankItem.QuestionType;
                classification = questionBankItem.Classification;
                questionRationale = questionBankItem.QuestionRationale;
                scraperNotes = questionBankItem.ScraperNotes;
                customNotes = questionBankItem.CustomNotes;
                rowSortOrder = questionBankItem.RowSortOrder;
                columnSortOrder = questionBankItem.ColumnSortOrder;
                answerMin = questionBankItem.AnswerMin;
                answerMax = questionBankItem.AnswerMax;
                questionFormatDetails = questionBankItem.QuestionFormatDetails;
                isDummy = questionBankItem.IsDummy;
            }
            else
            {
                // Manual question addition - use provided values
                variableName = request.VariableName!; // Validated by FluentValidation
                version = request.Version ?? 1; // Default to version 1
                questionText = request.QuestionText;
                questionTitle = request.QuestionTitle;
                questionType = request.QuestionType;
                classification = request.Classification;
                questionRationale = request.QuestionRationale;
                scraperNotes = request.ScraperNotes;
                customNotes = request.CustomNotes;
                rowSortOrder = request.RowSortOrder;
                columnSortOrder = request.ColumnSortOrder;
                answerMin = request.AnswerMin;
                answerMax = request.AnswerMax;
                questionFormatDetails = request.QuestionFormatDetails;
                isDummy = request.IsDummy ?? false;
            }

            // Get the next sort order
            var maxSortOrder = await context.Set<QuestionnaireLine>()
                .Where(pq => pq.ProjectId == projectId)
                .MaxAsync(pq => (int?)pq.SortOrder, cancellationToken) ?? -1;

            // Create questionnaire line
            var questionnaireLine = new QuestionnaireLine
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                QuestionBankItemId = request.QuestionBankItemId,
                SortOrder = maxSortOrder + 1,
                
                // Set editable fields
                VariableName = variableName,
                Version = version,
                QuestionText = questionText,
                QuestionTitle = questionTitle,
                QuestionType = questionType,
                Classification = classification,
                QuestionRationale = questionRationale,
                ScraperNotes = scraperNotes,
                CustomNotes = customNotes,
                RowSortOrder = rowSortOrder,
                ColumnSortOrder = columnSortOrder,
                AnswerMin = answerMin,
                AnswerMax = answerMax,
                QuestionFormatDetails = questionFormatDetails,
                IsDummy = isDummy,
                
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
