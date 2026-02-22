using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Clients;

public static class UpdateClientEndpoint
{
    public static void MapUpdateClientEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/clients/{id}", HandleAsync)
            .WithName("UpdateClient")
            .WithSummary("Update Client")
            .WithTags("Clients");
    }

    public static async Task<Results<Ok<UpdateClientResponse>, NotFound, ValidationProblem>> HandleAsync(
        Guid id,
        UpdateClientRequest request,
        IClientService clientService,
        IValidator<UpdateClientRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await clientService.UpdateClientAsync(id, request, "System", cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
