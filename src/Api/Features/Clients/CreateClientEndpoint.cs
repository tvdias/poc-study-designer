using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Api.Features.Clients;

public static class CreateClientEndpoint
{
    public static void MapCreateClientEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/clients", HandleAsync)
            .WithName("CreateClient")
            .WithSummary("Create Client")
            .WithTags("Clients");
    }

    public static async Task<Results<CreatedAtRoute<CreateClientResponse>, ValidationProblem, Conflict<string>>> HandleAsync(
        CreateClientRequest request,
        IClientService clientService,
        IValidator<CreateClientRequest> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await clientService.CreateClientAsync(request, "System", cancellationToken);
            return TypedResults.CreatedAtRoute(response, "GetClientById", new { id = response.Id });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(ex.Message);
        }
    }
}
