using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Features.ConfigurationQuestions;

public static class DeleteDependencyRuleEndpoint
{
    public static void MapDeleteDependencyRuleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/dependency-rules/{id:guid}", HandleAsync)
            .WithName("DeleteDependencyRule")
            .WithSummary("Delete Dependency Rule")
            .WithTags("Dependency Rules");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var rule = await db.DependencyRules.FindAsync([id], cancellationToken);

        if (rule == null)
        {
            return TypedResults.NotFound();
        }

        db.DependencyRules.Remove(rule);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
