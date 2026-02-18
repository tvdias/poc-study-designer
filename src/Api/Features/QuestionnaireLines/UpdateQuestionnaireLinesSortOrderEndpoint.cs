using Api.Data;
using Api.Features.QuestionnaireLines.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.QuestionnaireLines;

public static class UpdateQuestionnaireLinesSortOrderEndpoint
{
    public static void MapUpdateQuestionnaireLinesSortOrderEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/projects/{projectId:guid}/questionnairelines/sort-order", async Task<Results<NoContent, ValidationProblem, NotFound>> (
            Guid projectId,
            UpdateQuestionnaireLinesSortOrderRequest request,
            ApplicationDbContext context,
            IValidator<UpdateQuestionnaireLinesSortOrderRequest> validator,
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

            // Get all questionnaires for this project
            var questionnaireIds = request.Items.Select(i => i.Id).ToList();
            var questionnaires = await context.Set<QuestionnaireLine>()
                .Where(pq => pq.ProjectId == projectId && questionnaireIds.Contains(pq.Id))
                .ToListAsync(cancellationToken);

            if (questionnaires.Count != request.Items.Count)
            {
                return TypedResults.NotFound();
            }

            // Update sort orders
            foreach (var item in request.Items)
            {
                var questionnaire = questionnaires.First(q => q.Id == item.Id);
                questionnaire.SortOrder = item.SortOrder;
                questionnaire.ModifiedOn = DateTime.UtcNow;
                questionnaire.ModifiedBy = "system"; // TODO: Replace with actual user
            }

            await context.SaveChangesAsync(cancellationToken);

            return TypedResults.NoContent();
        })
        .WithName("UpdateQuestionnaireLinesSortOrder")
        .WithTags("QuestionnaireLines");
    }
}
