using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.MetricGroups;

public static class GetMetricGroupsEndpoint
{
    public static void MapGetMetricGroupsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/metric-groups", HandleAsync)
            .WithName("GetMetricGroups")
            .WithSummary("Get Metric Groups")
            .WithTags("MetricGroups");
    }

    public static async Task<List<GetMetricGroupsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var metricGroupsQuery = db.MetricGroups.Where(mg => mg.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            metricGroupsQuery = metricGroupsQuery.Where(mg => EF.Functions.ILike(mg.Name, pattern));
        }

        return await metricGroupsQuery
            .Select(mg => new GetMetricGroupsResponse(mg.Id, mg.Name))
            .ToListAsync(cancellationToken);
    }
}
