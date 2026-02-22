using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Projects;

public record UpdateProjectRequest(
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    Guid? ProductId,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled
);

public record UpdateProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    Guid? ProductId,
    string? Owner,
    ProjectStatus Status,
    bool CostManagementEnabled,
    DateTime ModifiedOn
);

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Owner)
            .MaximumLength(100).WithMessage("Owner must not exceed 100 characters");
    }
}

public static class UpdateProjectEndpoint
{
    public static void MapUpdateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/projects/{id:guid}", async Task<Results<Ok<UpdateProjectResponse>, ValidationProblem, NotFound, ProblemHttpResult>> (
            Guid id,
            UpdateProjectRequest request,
            IProjectService projectService,
            IValidator<UpdateProjectRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var response = await projectService.UpdateProjectAsync(id, request, "system", ct);
                return TypedResults.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Project Name Conflict"
                );
            }
        })
        .WithName("UpdateProject")
        .WithOpenApi();
    }
}
