using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.MetricGroups;

public static class DeleteMetricGroupEndpoint
{
    public static void MapDeleteMetricGroupEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/metric-groups/{id}", HandleAsync)
            .WithName("DeleteMetricGroup")
            .WithSummary("Delete Metric Group")
            .WithTags("MetricGroups");
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var metricGroup = await db.MetricGroups.FindAsync([id], cancellationToken);

        if (metricGroup is null)
        {
            return TypedResults.NotFound();
        }

        // Soft delete
        metricGroup.IsActive = false;
        metricGroup.ModifiedOn = DateTime.UtcNow;
        metricGroup.ModifiedBy = "System"; // TODO: Replace with real user

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
