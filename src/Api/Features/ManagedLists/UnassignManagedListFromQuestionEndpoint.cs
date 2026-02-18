using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ManagedLists;

public static class UnassignManagedListFromQuestionEndpoint
{
    public static void MapUnassignManagedListFromQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/managedlists/{managedListId:guid}/unassign/{questionnaireLineId:guid}", HandleAsync)
            .WithName("UnassignManagedListFromQuestion")
            .WithSummary("Unassign Managed List from Question")
            .WithTags("ManagedLists");
    }

    public static async Task<Results<NoContent, NotFound<string>>> HandleAsync(
        Guid managedListId,
        Guid questionnaireLineId,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var assignment = await db.QuestionManagedLists
            .FirstOrDefaultAsync(qml => qml.ManagedListId == managedListId && qml.QuestionnaireLineId == questionnaireLineId, cancellationToken);
        
        if (assignment == null)
        {
            return TypedResults.NotFound($"Assignment between managed list '{managedListId}' and question '{questionnaireLineId}' not found.");
        }

        db.QuestionManagedLists.Remove(assignment);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
