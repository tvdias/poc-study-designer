using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ConfigurationQuestions;

public static class DeleteConfigurationAnswerEndpoint
{
    public static void MapDeleteConfigurationAnswerEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/configuration-answers/{id:guid}", HandleAsync)
            .WithName("DeleteConfigurationAnswer")
            .WithSummary("Delete Configuration Answer")
            .WithTags("Configuration Answers");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var answer = await db.ConfigurationAnswers.FindAsync([id], cancellationToken);

        if (answer == null)
        {
            return TypedResults.NotFound();
        }

        // The database foreign key is configured with OnDelete(DeleteBehavior.SetNull)
        // so TriggeringAnswerId will be automatically set to null when this answer is deleted
        db.ConfigurationAnswers.Remove(answer);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
