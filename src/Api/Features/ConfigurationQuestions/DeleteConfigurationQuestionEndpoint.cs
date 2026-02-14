using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class DeleteConfigurationQuestionEndpoint
{
    public static void MapDeleteConfigurationQuestionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/configuration-questions/{id:guid}", HandleAsync)
            .WithName("DeleteConfigurationQuestion")
            .WithSummary("Delete Configuration Question")
            .WithTags("Configuration Questions");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var question = await db.ConfigurationQuestions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
        {
            return TypedResults.NotFound();
        }

        // Delete associated dependency rules first
        var dependencyRules = await db.DependencyRules
            .Where(dr => dr.ConfigurationQuestionId == id)
            .ToListAsync(cancellationToken);
        
        db.DependencyRules.RemoveRange(dependencyRules);

        // Delete the question (and answers will be cascade deleted)
        db.ConfigurationQuestions.Remove(question);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
