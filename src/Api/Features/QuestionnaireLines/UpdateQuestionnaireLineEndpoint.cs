using Api.Data;
using Api.Features.QuestionnaireLines.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionnaireLines;

public static class UpdateQuestionnaireLineEndpoint
{
    public static void MapUpdateQuestionnaireLineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/projects/{projectId:guid}/questionnairelines/{id:guid}", async Task<Results<Ok<QuestionnaireLineDto>, ValidationProblem, NotFound>> (
            Guid projectId,
            Guid id,
            UpdateQuestionnaireLineRequest request,
            ApplicationDbContext context,
            IValidator<UpdateQuestionnaireLineRequest> validator,
            CancellationToken cancellationToken) =>
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            // Get the questionnaire
            var questionnaire = await context.Set<QuestionnaireLine>()
                .FirstOrDefaultAsync(pq => pq.Id == id && pq.ProjectId == projectId, cancellationToken);

            if (questionnaire == null)
            {
                return TypedResults.NotFound();
            }

            // Update editable fields
            questionnaire.QuestionText = request.QuestionText;
            questionnaire.QuestionTitle = request.QuestionTitle;
            questionnaire.QuestionRationale = request.QuestionRationale;
            questionnaire.ScraperNotes = request.ScraperNotes;
            questionnaire.CustomNotes = request.CustomNotes;
            questionnaire.RowSortOrder = request.RowSortOrder;
            questionnaire.ColumnSortOrder = request.ColumnSortOrder;
            questionnaire.AnswerMin = request.AnswerMin;
            questionnaire.AnswerMax = request.AnswerMax;
            questionnaire.QuestionFormatDetails = request.QuestionFormatDetails;
            questionnaire.ModifiedOn = DateTime.UtcNow;
            questionnaire.ModifiedBy = "system"; // TODO: Replace with actual user

            await context.SaveChangesAsync(cancellationToken);

            var response = new QuestionnaireLineDto(
                questionnaire.Id,
                questionnaire.ProjectId,
                questionnaire.QuestionBankItemId,
                questionnaire.SortOrder,
                questionnaire.VariableName,
                questionnaire.Version,
                questionnaire.QuestionText,
                questionnaire.QuestionTitle,
                questionnaire.QuestionType,
                questionnaire.Classification,
                questionnaire.QuestionRationale,
                questionnaire.ScraperNotes,
                questionnaire.CustomNotes,
                questionnaire.RowSortOrder,
                questionnaire.ColumnSortOrder,
                questionnaire.AnswerMin,
                questionnaire.AnswerMax,
                questionnaire.QuestionFormatDetails,
                questionnaire.IsDummy
            );

            return TypedResults.Ok(response);
        })
        .WithName("UpdateQuestionnaireLine")
        .WithTags("QuestionnaireLines");
    }
}
