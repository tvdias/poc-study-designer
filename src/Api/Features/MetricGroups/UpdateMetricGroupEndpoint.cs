using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.MetricGroups;

public static class UpdateMetricGroupEndpoint
{
    public static void MapUpdateMetricGroupEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/metric-groups/{id}", HandleAsync)
            .WithName("UpdateMetricGroup")
            .WithSummary("Update Metric Group")
            .WithTags("MetricGroups");
    }

    public static async Task<Results<Ok<UpdateMetricGroupResponse>, NotFound, ValidationProblem, Conflict<string>>> HandleAsync(
        Guid id,
        UpdateMetricGroupRequest request,
        ApplicationDbContext db,
        IValidator<UpdateMetricGroupRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var metricGroup = await db.MetricGroups.FindAsync([id], cancellationToken);

        if (metricGroup is null)
        {
            return TypedResults.NotFound();
        }

        metricGroup.Name = request.Name;
        metricGroup.IsActive = request.IsActive;
        metricGroup.ModifiedOn = DateTime.UtcNow;
        metricGroup.ModifiedBy = "System"; // TODO: Replace with real user

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Metric Group '{request.Name}' already exists.");
            }
            throw;
        }

        return TypedResults.Ok(new UpdateMetricGroupResponse(metricGroup.Id, metricGroup.Name, metricGroup.IsActive));
    }
}
