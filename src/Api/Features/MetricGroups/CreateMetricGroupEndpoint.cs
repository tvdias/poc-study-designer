using Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace Api.Features.MetricGroups;

public static class CreateMetricGroupEndpoint
{
    public static void MapCreateMetricGroupEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/metric-groups", HandleAsync)
            .WithName("CreateMetricGroup")
            .WithSummary("Create Metric Group")
            .WithTags("MetricGroups");
    }

    public static async Task<Results<CreatedAtRoute<CreateMetricGroupResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateMetricGroupRequest request,
        ApplicationDbContext db,
        IValidator<CreateMetricGroupRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var metricGroup = new MetricGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System" // TODO: Replace with real user when auth is available
        };

        db.MetricGroups.Add(metricGroup);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Different database providers use different error codes for unique constraint violations.
            // This is a generic check that looks for common keywords in the inner exception.
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                return TypedResults.Conflict($"Metric Group '{request.Name}' already exists.");
            }

            throw;
        }

        return TypedResults.CreatedAtRoute(new CreateMetricGroupResponse(metricGroup.Id, metricGroup.Name, metricGroup.IsActive), "GetMetricGroupById", new { id = metricGroup.Id });
    }
}
