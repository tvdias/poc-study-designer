using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.MetricGroups;

public static class GetMetricGroupByIdEndpoint
{
    public static void MapGetMetricGroupByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/metric-groups/{id}", HandleAsync)
            .WithName("GetMetricGroupById")
            .WithSummary("Get Metric Group By Id")
            .WithTags("MetricGroups");
    }

    public static async Task<Results<Ok<GetMetricGroupByIdResponse>, NotFound>> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var metricGroup = await db.MetricGroups
            .AsNoTracking()
            .Where(mg => mg.IsActive)
            .FirstOrDefaultAsync(mg => mg.Id == id, cancellationToken);

        if (metricGroup == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new GetMetricGroupByIdResponse(metricGroup.Id, metricGroup.Name, metricGroup.IsActive));
    }
}
