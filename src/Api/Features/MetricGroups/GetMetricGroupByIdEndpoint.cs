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
        var metricGroupResponse = await db.MetricGroups
            .Where(mg => mg.IsActive)
            .Where(mg => mg.Id == id)
            .Select(mg => new GetMetricGroupByIdResponse(mg.Id, mg.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (metricGroupResponse == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(metricGroupResponse);
    }
}
