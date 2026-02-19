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

    public static async Task<Results<Ok, NotFound, ValidationProblem, ProblemHttpResult>> HandleAsync(
        Guid studyId,
        UpdateStudyRequest request,
        ApplicationDbContext db,
        IStudyService studyService,
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

        // Validate name uniqueness if changing name
        var trimmedName = request.Name.Trim();
        if (study.Name != trimmedName)
        {
            try 
            {
                await studyService.ValidateStudyNameUniquenessAsync(study.ProjectId, trimmedName, study.MasterStudyId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Study Name Conflict"
                );
            }
        }

        study.Name = trimmedName;
        study.Category = request.Category.Trim();
        study.MaconomyJobNumber = request.MaconomyJobNumber.Trim();
        study.ProjectOperationsUrl = request.ProjectOperationsUrl.Trim();
        study.ScripterNotes = request.ScripterNotes;
        study.FieldworkMarketId = request.FieldworkMarketId;
        
        if (request.Status.HasValue)
        {
            study.Status = request.Status.Value;
        }

        study.ModifiedOn = DateTime.UtcNow;
        study.ModifiedBy = "System"; // TODO: Get from auth context

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
