using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.ManagedLists;

public static class GetManagedListsEndpoint
{
    public static void MapGetManagedListsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/managedlists", HandleAsync)
            .WithName("GetManagedLists")
            .WithSummary("Get Managed Lists")
            .WithTags("ManagedLists");
    }

    public static async Task<List<GetManagedListsResponse>> HandleAsync(
        Guid? projectId,
        string? query,
        bool? includeInactive,
        int? page,
        int? pageSize,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        // Set default values
        var effectivePage = (page ?? 0) <= 0 ? 1 : page!.Value;
        var effectivePageSize = (pageSize ?? 0) <= 0 ? 50 : Math.Min(pageSize!.Value, 100);
        var effectiveIncludeInactive = includeInactive ?? false;

        var managedListsQuery = db.ManagedLists.AsQueryable();

        // Filter by project if provided
        if (projectId.HasValue)
        {
            managedListsQuery = managedListsQuery.Where(ml => ml.ProjectId == projectId.Value);
        }

        // Filter by status
        if (!effectiveIncludeInactive)
        {
            managedListsQuery = managedListsQuery.Where(ml => ml.Status == ManagedListStatus.Active);
        }

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            managedListsQuery = managedListsQuery.Where(ml => 
                EF.Functions.ILike(ml.Name, pattern) || 
                (ml.Description != null && EF.Functions.ILike(ml.Description, pattern)));
        }

        return await managedListsQuery
            .OrderBy(ml => ml.Name)
            .Skip((effectivePage - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .Select(ml => new GetManagedListsResponse(
                ml.Id,
                ml.ProjectId,
                ml.Name,
                ml.Description,
                ml.Status,
                ml.Items.Count(i => i.IsActive),
                ml.QuestionAssignments.Count,
                ml.CreatedOn,
                ml.CreatedBy))
            .ToListAsync(cancellationToken);
    }
}
