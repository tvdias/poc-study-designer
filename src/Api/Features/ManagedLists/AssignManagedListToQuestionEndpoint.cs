using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.ManagedLists;

public static class AssignManagedListToQuestionEndpoint
{
    public static void MapAssignManagedListToQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/managedlists/assign", HandleAsync)
            .WithName("AssignManagedListToQuestion")
            .WithSummary("Assign Managed List to Question")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<CreatedAtRoute<AssignManagedListToQuestionResponse>, ValidationProblem, NotFound<string>, Conflict<string>>> HandleAsync(
        AssignManagedListToQuestionRequest request,
        ApplicationDbContext db,
        IValidator<AssignManagedListToQuestionRequest> validator,
        IAutoAssociationService autoAssociationService,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if questionnaire line exists
        var questionnaireLineExists = await db.QuestionnaireLines.AnyAsync(ql => ql.Id == request.QuestionnaireLineId, cancellationToken);
        if (!questionnaireLineExists)
        {
            return TypedResults.NotFound($"Questionnaire line with ID '{request.QuestionnaireLineId}' not found.");
        }

        // Check if managed list exists and is active
        var managedList = await db.ManagedLists.FindAsync(new object[] { request.ManagedListId }, cancellationToken);
        if (managedList == null)
        {
            return TypedResults.NotFound($"Managed list with ID '{request.ManagedListId}' not found.");
        }

        if (managedList.Status != ManagedListStatus.Active)
        {
            return TypedResults.Conflict("Cannot assign inactive managed list to a question.");
        }

        // Check if assignment already exists
        var existingAssignment = await db.QuestionManagedLists
            .AnyAsync(qml => qml.QuestionnaireLineId == request.QuestionnaireLineId && qml.ManagedListId == request.ManagedListId, cancellationToken);
        
        if (existingAssignment)
        {
            return TypedResults.Conflict("This managed list is already assigned to the question.");
        }

        var assignment = new QuestionManagedList
        {
            Id = Guid.NewGuid(),
            QuestionnaireLineId = request.QuestionnaireLineId,
            ManagedListId = request.ManagedListId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.QuestionManagedLists.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        // Trigger auto-association for Draft Studies (US5 - AC-AUTO-02)
        await autoAssociationService.OnManagedListAssignedToQuestionAsync(
            assignment.QuestionnaireLineId, 
            assignment.ManagedListId, 
            "System", 
            cancellationToken);

        var response = new AssignManagedListToQuestionResponse(
            assignment.Id,
            assignment.QuestionnaireLineId,
            assignment.ManagedListId);

        return TypedResults.CreatedAtRoute(response, "GetManagedListById", new { id = assignment.ManagedListId });
    }
}
