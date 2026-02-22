using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Projects;

public record CreateProjectRequest(
    string Name,
    string? Description,
    Guid? ClientId,
    Guid? CommissioningMarketId,
    Methodology? Methodology,
    Guid? ProductId,
    string? Owner,
    ProjectStatus? Status,
    bool? CostManagementEnabled
);

public record CreateProjectResponse(
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
    DateTime CreatedOn
);

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
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

public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/projects", async Task<Results<CreatedAtRoute<CreateProjectResponse>, ValidationProblem, ProblemHttpResult>> (
            CreateProjectRequest request,
            IProjectService projectService,
            IValidator<CreateProjectRequest> validator,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            try
            {
                var response = await projectService.CreateProjectAsync(request, "system", ct);
                return TypedResults.CreatedAtRoute(response, "GetProjectById", new { id = response.Id });
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Project Name Conflict"
                );
            }
        })
        .WithName("CreateProject");
    }
}
