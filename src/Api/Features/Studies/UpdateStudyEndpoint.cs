using Api.Data;
using Api.Features.Studies.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Studies;

public static class UpdateStudyEndpoint
{
    public static void MapUpdateStudyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/studies/{studyId:guid}", HandleAsync)
            .WithName("UpdateStudy")
            .WithSummary("Update study name, description, or status")
            .WithTags("Studies");
    }

    public static async Task<Results<Ok, NotFound, ValidationProblem>> HandleAsync(
        Guid studyId,
        UpdateStudyRequest request,
        ApplicationDbContext db,
        IValidator<UpdateStudyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var study = await db.Studies.FindAsync([studyId], cancellationToken);
        if (study == null)
        {
            return TypedResults.NotFound();
        }

        study.Name = request.Name;
        study.Description = request.Description;
        
        if (request.Status.HasValue)
        {
            study.Status = request.Status.Value;
            study.StatusReason = request.StatusReason;
        }

        study.ModifiedOn = DateTime.UtcNow;
        study.ModifiedBy = "System"; // TODO: Get from auth context

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
